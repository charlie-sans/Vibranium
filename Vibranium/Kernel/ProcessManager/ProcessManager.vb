Imports System.Reflection
Imports System.IO
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
        Throw New NotImplementedException
    End Sub

End Class
