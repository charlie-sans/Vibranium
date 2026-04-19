Imports System.Reflection
Imports System.IO

Namespace Vibranium.Kernel.ProcessManager

    Public Class ProcessManager
        Dim nextPID As Integer = 1
        Dim processes As New System.Collections.Generic.Dictionary(Of Integer, ProcessNode)
        Public ReadOnly Root As ProcessNode

        Public Sub New()
            Root = New ProcessNode() With {.PID = 0, .Name = "<root>", .PPID = -1}
            processes(0) = Root
        End Sub

        Public Function CreateProcess(proc As IProcess, Optional parentPID As Integer = 0) As Integer
            Dim pid = System.Threading.Interlocked.Increment(nextPID) - 1
            Dim node = New ProcessNode() With {.PID = pid, .PPID = parentPID, .Process = proc, .Name = If(proc IsNot Nothing, proc.Name, "proc-" & pid)}
            processes(pid) = node

            If processes.ContainsKey(parentPID) Then
                node.Parent = processes(parentPID)
                processes(parentPID).Children.Add(node)
            Else
                node.Parent = Root
                Root.Children.Add(node)
            End If

            If proc IsNot Nothing Then
                Try
                    proc.PID = pid
                    proc.PPID = parentPID
                Catch
                End Try
                ' Start the process on a new thread
                Dim threadProc As New System.Threading.Thread(Sub()
                                                                  Try
                                                                      ' Register thread to PID mapping
                                                                      Sys.Environment.RegisterThread(pid)
                                                                      ' Wait for required Sys globals to be initialized
                                                                      Dim waited As Integer = 0
                                                                      While (Core.VFS Is Nothing OrElse Core.ProcessManager Is Nothing _
                                                                          OrElse Core.ServiceHandler Is Nothing OrElse Core.Kernel Is Nothing)
                                                                          If waited Mod 10 = 0 Then
                                                                              Console.WriteLine(String.Format("[ProcessManager]: Waiting for Sys globals in process thread for PID={0}...", pid))

                                                                              If Core.VFS Is Nothing Then Console.WriteLine("[ProcessManager]: Sys.VFS is Nothing")
                                                                              If Core.ProcessManager Is Nothing Then Console.WriteLine("[ProcessManager]: Sys.ProcessManager is Nothing")
                                                                              If Core.ServiceHandler Is Nothing Then Console.WriteLine("[ProcessManager]: Sys.ServiceHandler is Nothing")
                                                                              If Core.Kernel Is Nothing Then Console.WriteLine("[ProcessManager]: Sys.Kernel is Nothing")
                                                                          End If
                                                                          System.Threading.Thread.Sleep(50)
                                                                          waited += 1
                                                                          ' Optional: timeout after 10 seconds
                                                                          If waited > 200 Then
                                                                              Console.WriteLine(String.Format("[ProcessManager]: Timeout waiting for Sys globals in process thread for PID={0}. Aborting.", pid))
                                                                              Sys.Environment.UnregisterThread()
                                                                              Return
                                                                          End If
                                                                      End While
                                                                      ' Only run Start + Tick loop if all required globals are present
                                                                      If Core.VFS IsNot Nothing AndAlso Core.ProcessManager IsNot Nothing _
                                                                          AndAlso Core.ServiceHandler IsNot Nothing AndAlso Core.Kernel IsNot Nothing Then
                                                                          Try
                                                                              ' Call one-time startup hook
                                                                              Try
                                                                                  proc.Start()
                                                                              Catch exStart As Exception
                                                                                  Console.WriteLine(String.Format("[ProcessManager]: Exception in proc.Start for PID={0}: {1}", pid, exStart.Message))
                                                                                  Console.WriteLine("[ProcessManager]: StackTrace: " & exStart.ToString())
                                                                              End Try

                                                                              ' If the process remains running, enter the Tick loop (for interactive/long-lived processes)
                                                                              While proc.IsRunning
                                                                                  Try
                                                                                      proc.Tick()
                                                                                  Catch exTick As Exception
                                                                                      Console.WriteLine(String.Format("[ProcessManager]: Exception in proc.Tick for PID={0}: {1}", pid, exTick.Message))
                                                                                      Console.WriteLine("[ProcessManager]: StackTrace: " & exTick.ToString())
                                                                                      Exit While
                                                                                  End Try
                                                                              End While
                                                                          Catch
                                                                              Console.WriteLine(String.Format("[ProcessManager]: Aborting process thread for PID={0} due to missing globals.", pid))
                                                                          End Try
                                                                      Else
                                                                          Console.WriteLine(String.Format("[ProcessManager]: Aborting process thread for PID={0} due to missing globals.", pid))
                                                                      End If
                                                                  Catch ex As Exception
                                                                      Console.WriteLine(String.Format("[ProcessManager]: Exception in process thread for PID={0}: {1}", pid, ex.Message))
                                                                      Console.WriteLine("[ProcessManager]: StackTrace: " & ex.ToString())
                                                                  Finally
                                                                      Sys.Environment.UnregisterThread()
                                                                  End Try
                                                              End Sub)
            threadProc.IsBackground = True
            threadProc.Start()
        End If

        Return pid
    End Function

    Public Sub KillProcess(pid As Integer)
        If processes.ContainsKey(pid) Then
            processes(pid).Kill()
        End If
    End Sub

    Public Sub Tick()
        Root.Tick()
    End Sub

    Public Function GetProcess(pid As Integer) As ProcessNode
        If processes.ContainsKey(pid) Then Return processes(pid)
        Return Nothing
    End Function

    ' Start a process by VFS path. Resolves symlinks via VFS metadata. If symlink -> function, use ProgramRegistry.
    ' If target is a DLL, attempts to load the assembly and instantiate a type implementing IProcess.
    Public Function StartByPath(ByVal vfs As Object, ByVal virtualPath As String, Optional ByVal parentPID As Integer = 0) As Integer
        Try
            If vfs Is Nothing Then Return -1

            ' Try resolve symlink first
            Dim symlinkTarget = Nothing
            Try
                symlinkTarget = CallByName(vfs, "ResolveSymlink", Microsoft.VisualBasic.CallType.Method, virtualPath)
            Catch
                symlinkTarget = Nothing
            End Try

            If symlinkTarget IsNot Nothing Then
                Dim tType = CallByName(symlinkTarget, "Type", Microsoft.VisualBasic.CallType.Get)
                Dim tVal = CallByName(symlinkTarget, "Value", Microsoft.VisualBasic.CallType.Get)
                If String.Equals(CStr(tType), "function", StringComparison.OrdinalIgnoreCase) OrElse String.Equals(CStr(tType), "func", StringComparison.OrdinalIgnoreCase) Then
                    Dim proc = ProgramRegistry.Instance.CreateByName(CStr(tVal), virtualPath)
                    If proc IsNot Nothing Then
                        Try
                            proc.IsRunning = True
                        Catch
                        End Try
                        Return CreateProcess(proc, parentPID)
                    End If
                    Return -1
                ElseIf String.Equals(CStr(tType), "path", StringComparison.OrdinalIgnoreCase) Then
                    virtualPath = CStr(tVal)
                End If
            End If

            ' If path points to a dll, attempt to load assembly and find IProcess
            Dim isFile = False
            Try
                isFile = CallByName(vfs, "IsFile", Microsoft.VisualBasic.CallType.Method, virtualPath)
            Catch
                isFile = False
            End Try

            If Not isFile Then
                Return -1
            End If

            Dim physicalPath = CStr(CallByName(vfs, "MapToPhysical", Microsoft.VisualBasic.CallType.Method, virtualPath))
            If String.IsNullOrEmpty(physicalPath) Then Return -1

            Dim ext = Path.GetExtension(physicalPath)
            If String.Equals(ext, ".dll", StringComparison.OrdinalIgnoreCase) Then
                Dim asm = Assembly.LoadFrom(physicalPath)
                For Each t In asm.GetTypes()
                    If Not t.IsAbstract AndAlso GetType(IProcess).IsAssignableFrom(t) Then
                        Dim obj = Activator.CreateInstance(t)
                        Dim proc = TryCast(obj, IProcess)
                        If proc IsNot Nothing Then
                            Try
                                proc.IsRunning = True
                            Catch
                            End Try
                            Return CreateProcess(proc, parentPID)
                        End If
                    End If
                Next
            End If

            Return -1
        Catch
            Return -1
        End Try
    End Function

    Sub StartProcessFromVFS(p1 As String)
        Throw New NotImplementedException()
    End Sub

End Class
End Namespace