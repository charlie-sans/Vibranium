Imports System
Imports System.Collections.Generic

Public Class ProgramRegistry
    Private Shared _instance As ProgramRegistry
    Public Shared ReadOnly Property Instance As ProgramRegistry
        Get
            If _instance Is Nothing Then
                _instance = New ProgramRegistry()
            End If
            Return _instance
        End Get
    End Property

    Private ReadOnly factories As New Dictionary(Of String, Func(Of String, IProcess))(StringComparer.OrdinalIgnoreCase)

    Public Sub Register(ByVal name As String, ByVal factory As Func(Of String, IProcess))
        If String.IsNullOrEmpty(name) Then Throw New ArgumentNullException("class canot be registered")
        If factory Is Nothing Then Throw New ArgumentNullException("it broke, the factory srory.")
        factories(name) = factory
    End Sub

    Public Function CreateByName(ByVal name As String, ByVal virtualPath As String) As IProcess
        If String.IsNullOrEmpty(name) Then Return Nothing
        If factories.ContainsKey(name) Then
            Try
                Return factories(name)(virtualPath)
            Catch
                Return Nothing
            End Try
        End If
        Return Nothing
    End Function
End Class
