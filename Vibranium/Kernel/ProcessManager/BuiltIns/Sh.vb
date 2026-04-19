Imports Vibranium.Kernel, Vibranium, Vibranium.Sys
Namespace Apps

    Public Class ShProcess
        Implements IProcess

        Public Property PID As Integer Implements IProcess.PID
        Public Property Name As String Implements IProcess.Name
        Public Property PPID As Integer Implements IProcess.PPID
        Public Property IsRunning As Boolean Implements IProcess.IsRunning

        Public Sub New()
            Name = "sh"
        End Sub

        Public Sub Tick() Implements IProcess.Tick
            ' Register thread to PID mapping
            Vibranium.Sys.Environment.RegisterThread(PID)
            Try
                Dim CMD = IO.ReadLine("$: ")
                Dim args As String() = Split(CMD, " ")
                Try
                    Select Case Trim(CMD)
                        Case "exit"
                            Vibranium.Sys.Environment.ExitCurrentProcess()
                        Case "about"
                            ProcessManager.StartByPath(VFS, "/bin/about")
                        Case Else
                            IO.WriteLine("Invalid input")
                    End Select
                Catch ex As Exception
                    Throw New KernelPanic(CMD & " is not a thing that can be entered")
                End Try
            Catch
            End Try
        End Sub

        Public Sub Start() Implements IProcess.Start
            ' No-op start for interactive shell; Tick will handle the interactive loop.
        End Sub

        Public Sub Kill() Implements IProcess.Kill
            IsRunning = False
            Vibranium.Sys.Environment.UnregisterThread()
        End Sub

    End Class
End Namespace
