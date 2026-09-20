Imports Vibranium, Vibranium.Sys, Vibranium.Kernel
Namespace Sys
    Public MustInherit Class Application
        Implements IProcess

        Public Property IsRunning As Boolean Implements IProcess.IsRunning


        Public Overridable Sub Kill() Implements IProcess.Kill
            Vibranium.Sys.Environment.UnregisterThread()
        End Sub


        Public Property Name As String Implements IProcess.Name


        Public Property PID As Integer Implements IProcess.PID


        Public Property PPID As Integer Implements IProcess.PPID


        Public Overridable Sub Tick() Implements IProcess.Tick
            Vibranium.Sys.Environment.RegisterThread(PID)
        End Sub

            Public Overridable Sub Start() Implements IProcess.Start
                ' Default no-op start hook for processes that do not need one-time startup actions.
            End Sub
    End Class
End Namespace