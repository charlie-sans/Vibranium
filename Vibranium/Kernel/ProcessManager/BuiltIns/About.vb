Imports Vibranium
Namespace Apps
    Public Class AboutWindow
        Inherits Sys.Application
        Dim IsShowing As Boolean = True
        Public Overrides Sub Tick()
            MyBase.Tick()
            ' No continuous ticking required for AboutWindow; Start() handles showing.
        End Sub
        Public Overrides Sub Start()
            MyBase.Start()

            Dim about = New AboutBox1()
            about.Show()
           
            ' About is a one-shot UI; mark process as not running so the manager won't enter Tick loop.
            Try
                Me.IsRunning = False
            Catch
            End Try
        End Sub
        Public Overrides Sub Kill()
            MyBase.Kill()
        End Sub
    End Class
End Namespace