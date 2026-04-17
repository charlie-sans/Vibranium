Namespace Vibranium.Services

    Public Class ServiceHandler

        Dim Services As List(Of Service) = New List(Of Service)

        Public Sub Init()
            For Each SV As Service In Services
                SV.Init()
            Next
        End Sub

        Public Sub AddService(ByVal Srv As Service)
            Services.Add(Srv)
        End Sub


        Public Function FindService(ByVal name As String)
            For Each Service In Services
                If Service.Name = name Then
                    Return Service
                End If
            Next
            Return Nothing
        End Function

        Sub Tick()

        End Sub

    End Class
End Namespace
