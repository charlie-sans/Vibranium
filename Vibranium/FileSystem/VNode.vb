Namespace Vibranium.FileSystem

    Public MustInherit Class VNode

        Private _p As String

        Protected Sub New(p As String)
            _p = p
        End Sub
        Protected Sub New()

        End Sub
        Public Property Name As String
        Public Property Parent As DirectoryNode

        Public Overridable ReadOnly Property FullPath As String
            Get
                If Parent Is Nothing Then Return "/"
                Return Parent.FullPath.TrimEnd("/"c) & "/" & Name
            End Get
        End Property

        Public Overridable Function CanRead() As Boolean
            Return False
        End Function

        Public Overridable Function CanWrite() As Boolean
            Return False
        End Function

        Public Overridable Function CanExecute() As Boolean
            Return False
        End Function

        Public Overridable Function ReadText() As String
            Throw New Exception("Node is not readable.")
        End Function

        Public Overridable Sub WriteText(ByVal content As String)
            Throw New Exception("Node is not writable.")
        End Sub

        Public Overridable Sub Execute(ByVal args As String())
            Throw New Exception("Node is not executable.")
        End Sub
    End Class

End Namespace
