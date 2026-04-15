Public Class Sh
    Public Shared Sub Tick(ByVal args As String())
        Dim Contents = Sys.IO.ReadLine()
        Sys.IO.WriteLine(Contents)
    End Sub

End Class

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
        Try
            Sh.Tick(New String() {})
        Catch
        End Try
    End Sub

    Public Sub Kill() Implements IProcess.Kill
        IsRunning = False
    End Sub

End Class
