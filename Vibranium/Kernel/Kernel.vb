Imports System.Collections.Generic, System, System.Threading.Tasks
Public Class Kernel
    Dim Running As Boolean = True
    Dim Services As ServiceHandler
    Public Function Init() As Boolean
        Return True
    End Function
    Public Function MainLoop() As Boolean
        Try
            While (Running)

            End While

        Catch ex As Exception
            Console.WriteLine(ex)
        End Try
        Return True
    End Function
End Class
