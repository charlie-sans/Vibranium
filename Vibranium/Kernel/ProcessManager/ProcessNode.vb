Public Class ProcessNode
    Public Property PID As Integer
    Public Property PPID As Integer
    Public Property Name As String
    Public Property Process As IProcess
    Public Property Parent As ProcessNode
    Public ReadOnly Children As New System.Collections.Generic.List(Of ProcessNode)

    Public Function IsRunning() As Boolean
        Return Process IsNot Nothing AndAlso Process.IsRunning
    End Function

    Public Sub Tick()
        If Process IsNot Nothing AndAlso Process.IsRunning Then
            Try
                Process.Tick()
            Catch ex As Exception
            End Try
        End If

        For Each c In Children.ToArray()
            If c IsNot Nothing Then
                c.Tick()
            End If
        Next
    End Sub

    Public Sub Kill()
        If Process IsNot Nothing Then
            Try
                Process.Kill()
            Catch
            End Try
        End If

        For Each c In Children.ToArray()
            c.Kill()
        Next
    End Sub
End Class
