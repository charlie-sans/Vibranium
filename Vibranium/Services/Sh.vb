Imports Vibranium.Kernel
Namespace Vibranium.Services

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
            Sys.Environment.RegisterThread(PID)
            Try
                Dim CMD = IO.ReadLine("$: ")
                Dim args As String() = Split(CMD, " ")
                Try
                    Select Case Trim(CMD)
                        Case "exit"
                            Sys.Environment.ExitCurrentProcess()

                        Case Else
                            IO.WriteLine("Invalid input")
                    End Select
                Catch ex As Exception
                    Throw New KernelPanic(CMD & " is not a thing that can be entered")
                End Try
            Catch
            End Try
        End Sub

        Public Sub Kill() Implements IProcess.Kill
            IsRunning = False
            Sys.Environment.UnregisterThread()
        End Sub

    End Class
End Namespace
