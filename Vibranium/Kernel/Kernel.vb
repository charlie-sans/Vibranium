Imports System.Collections.Generic, System, System.Threading.Tasks
Public Class Kernel
    Dim Running As Boolean = True
    Public Function Init() As Boolean
        Startup.Startup()
        Return True
    End Function
    Public Function MainLoop() As Boolean
        Try
            While (Running)
                ' Tick services and the process tree each loop
                Try
                    Sys.ServiceHandler.Tick()
                Catch
                End Try

                If Sys.ProcessManager IsNot Nothing Then
                    Try
                        Sys.ProcessManager.Tick()
                    Catch
                    End Try
                End If

                System.Threading.Thread.Sleep(10)

            End While

        Catch ex As Exception
            Console.WriteLine(ex)
        End Try
        Return True
    End Function
End Class
