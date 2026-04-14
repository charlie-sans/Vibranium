Public Class VGeneratedNode
    Inherits VNode

    Private ReadOnly _generator As Func(Of String)

    Public Sub New(ByVal p As String, ByVal gen As Func(Of String))
        MyBase.New(p)
        _generator = gen
    End Sub

    Public Overrides Function ReadText() As String
        Return _generator()
    End Function
End Class