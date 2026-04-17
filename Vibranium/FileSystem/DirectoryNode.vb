Namespace Vibranium.FileSystem

    Public Class DirectoryNode
        Inherits VNode

        Public Property Children As New Dictionary(Of String, VNode)
        Public Sub New(ByVal p As String)
            MyBase.New(p)
        End Sub
        Public Sub Add(node As VNode)
            node.Parent = Me
            Children(node.Name) = node
        End Sub

        Public Function GetChild(name As String) As VNode
            Return Children(name)
        End Function
    End Class

End Namespace
