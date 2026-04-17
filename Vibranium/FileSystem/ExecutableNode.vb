Namespace Vibranium.FileSystem

    Public Class VExecutableNode
        Inherits VNode

        Private ReadOnly _action As Action(Of String())

        Public Sub New(ByVal p As String, ByVal action As Action(Of String()))
            MyBase.New(p)
            _action = action
        End Sub

        Public Overrides Sub Execute(ByVal args As String())
            _action(args)
        End Sub

    End Class

End Namespace
