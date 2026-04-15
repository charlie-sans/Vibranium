Public Interface IProcess
    Property PID As Integer
    Property Name As String
    Property PPID As Integer
    Property IsRunning As Boolean
    Sub Tick()
    Sub Kill()
End Interface
