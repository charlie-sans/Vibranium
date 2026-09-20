Imports System.IO
Imports Vibranium.Kernel, Vibranium, Vibranium.Sys, Vibranium.Kernel.ProcessManager
Namespace Apps

    Public Class ShProcess
        Implements IProcess

        Public Property PID As Integer Implements IProcess.PID
        Public Property Name As String Implements IProcess.Name
        Public Property PPID As Integer Implements IProcess.PPID
        Public Property IsRunning As Boolean Implements IProcess.IsRunning
        Private CurrentDirectory As String = "/"

        Public Sub New()
            Name = "sh"
        End Sub

        Private Function ResolveVirtualPath(ByVal arg As String) As String
            If String.IsNullOrEmpty(arg) Then Return CurrentDirectory
            If arg.StartsWith("/") Then
                Return arg
            ElseIf arg = "." Then
                Return CurrentDirectory
            ElseIf arg = ".." Then
                If CurrentDirectory = "/" Then Return "/"
                Dim parts = CurrentDirectory.TrimEnd("/"c).Split("/"c)
                If parts.Length <= 1 Then Return "/"
                Return String.Join("/", parts, 0, parts.Length - 1)
            Else
                If CurrentDirectory.EndsWith("/") Then
                    Return CurrentDirectory & arg
                Else
                    Return CurrentDirectory & "/" & arg
                End If
            End If
        End Function

        Public Sub Tick() Implements IProcess.Tick
           
        End Sub

        Public Sub Start() Implements IProcess.Start
            ' Register thread to PID mapping
            While (IsRunning)

                Vibranium.Sys.Environment.RegisterThread(PID)
                Try
                    Dim CMD = IO.ReadLine("$: ")
                    Dim parts As String() = Split(Trim(CMD), " ")
                    Dim cmdd = parts(0).ToLowerInvariant()
                    Dim arg = If(parts.Length > 1, parts(1), "")
                    Try
                        Select Case cmdd
                            Case "exit"
                                Vibranium.Sys.Environment.ExitCurrentProcess()
                            Case "about"
                                Core.ProcessManager.StartByPath(VFS, "/bin/about")
                            Case "pwd"
                                IO.WriteLine(CurrentDirectory)
                            Case "cd"
                                If String.IsNullOrEmpty(arg) Then
                                    CurrentDirectory = "/"
                                Else
                                    Dim target = ResolveVirtualPath(arg)
                                    If VFS.Exists(target) Then
                                        CurrentDirectory = target
                                    Else
                                        IO.WriteLine("Directory not found: " & target)
                                    End If
                                End If
                            Case "ls"
                                Dim target = If(String.IsNullOrEmpty(arg), CurrentDirectory, ResolveVirtualPath(arg))
                                Try
                                    Dim entries = VFS.GetEntries(target)
                                    For Each e In entries
                                        IO.WriteLine(e)
                                    Next
                                Catch ex As Exception
                                    IO.WriteLine("ls: error reading " & target)
                                End Try
                            Case "cat"
                                If String.IsNullOrEmpty(arg) Then
                                    IO.WriteLine("Usage: cat <file>")
                                Else
                                    Dim fp = ResolveVirtualPath(arg)
                                    Try
                                        Dim text = VFS.ReadAllText(fp)
                                        IO.WriteLine(text)
                                    Catch ex As Exception
                                        IO.WriteLine("cat: cannot read " & fp)
                                    End Try
                                End If
                            Case "touch"
                                If String.IsNullOrEmpty(arg) Then
                                    IO.WriteLine("Usage: touch <file>")
                                Else
                                    Dim fp = ResolveVirtualPath(arg)
                                    Try
                                        VFS.WriteAllText(fp, "")
                                    Catch ex As Exception
                                        IO.WriteLine("touch: failed to create " & fp)
                                    End Try
                                End If
                            Case "mkdir"
                                If String.IsNullOrEmpty(arg) Then
                                    IO.WriteLine("Usage: mkdir <dir>")
                                Else
                                    Dim dp = ResolveVirtualPath(arg)
                                    Try
                                        Dim physical = VFS.MapToPhysical(dp)
                                        If Not Directory.Exists(physical) Then Directory.CreateDirectory(physical)
                                    Catch ex As Exception
                                        IO.WriteLine("mkdir: failed to create " & dp)
                                    End Try
                                End If
                            Case "rm"
                                If String.IsNullOrEmpty(arg) Then
                                    IO.WriteLine("Usage: rm <path>")
                                Else
                                    Dim rp = ResolveVirtualPath(arg)
                                    Try
                                        Dim physical = VFS.MapToPhysical(rp)
                                        If File.Exists(physical) Then
                                            File.Delete(physical)
                                        ElseIf Directory.Exists(physical) Then
                                            Directory.Delete(physical, True)
                                        Else
                                            IO.WriteLine("rm: not found " & rp)
                                        End If
                                    Catch ex As Exception
                                        IO.WriteLine("rm: failed " & rp)
                                    End Try
                                End If
                            Case "find"
                                If String.IsNullOrEmpty(arg) Then
                                    IO.WriteLine("Usage: find <name>")
                                Else
                                    Dim startV = CurrentDirectory
                                    Dim startP = VFS.MapToPhysical(startV)
                                    Try
                                        For Each f In Directory.GetFiles(startP, "*" & arg & "*", SearchOption.AllDirectories)
                                            ' Convert back to virtual path when possible
                                            Dim rel = f.Replace(startP, "").Replace("\", "/")
                                            Dim virt = If(rel.StartsWith("/"), startV.TrimEnd("/"c) & rel, startV & "/" & rel)
                                            IO.WriteLine(virt)
                                        Next
                                    Catch ex As Exception
                                        IO.WriteLine("find: error searching from " & startV)
                                    End Try
                                End If
                            Case "help"
                                IO.WriteLine("commands: exit about pwd cd ls cat touch mkdir rm find help")
                            Case Else
                                IO.WriteLine("Invalid input: " & CMD)
                        End Select
                    Catch ex As Exception
                        Throw New KernelPanic(CMD & " is not a thing that can be entered")
                    End Try
                Catch
                End Try
            End While
        End Sub

        Public Sub Kill() Implements IProcess.Kill
            IsRunning = False
            Vibranium.Sys.Environment.UnregisterThread()
        End Sub

    End Class
End Namespace
