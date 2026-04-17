Imports System.IO

Namespace Vibranium.FileSystem

    Public Class FileNode
        Inherits VNode

        Private ReadOnly _physical As String

        Public Sub New(ByVal vPath As String, ByVal physical As String)
            MyBase.New(vPath)
            _physical = physical
        End Sub

        Public Overrides Function ReadText() As String
            Return File.ReadAllText(_physical)
        End Function

        Public Overrides Sub WriteText(ByVal content As String)
            File.WriteAllText(_physical, content)
        End Sub

    End Class

End Namespace