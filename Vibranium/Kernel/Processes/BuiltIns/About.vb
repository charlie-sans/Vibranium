Imports Vibranium
Imports System.Windows.Forms

Namespace Apps
    Public Class AboutWindow
        Inherits Sys.Application

        Private Window As Form

        Public Overrides Sub Tick()
            MyBase.Tick()
        End Sub

        Public Overrides Sub Start()
            MyBase.Start()

            IsRunning = True
            Window = New AboutBox1()

            Try
                Application.Run(Window)
            Finally
                IsRunning = False
            End Try
        End Sub

        Public Overrides Sub Kill()
            Try
                If Window IsNot Nothing Then
                    If Window.InvokeRequired Then
                        Window.Invoke(Sub() Window.Close())
                    Else
                        Window.Close()
                    End If
                End If
            Catch
            End Try

            MyBase.Kill()
            IsRunning = False
        End Sub
    End Class
End Namespace