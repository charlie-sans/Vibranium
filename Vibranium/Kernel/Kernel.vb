Imports System.Collections.Generic
Imports System
Imports System.Threading.Tasks

Namespace Vibranium.Kernel
    Public Class Kernel
        Public Kernel As Vibranium.Kernel.Kernel
        Public ServiceHandler As Vibranium.Services.ServiceHandler

        Public VFS As Vibranium.FileSystem.VirtualFileSystem
        'Public ProcessManager As Vibranium.
        Dim Running As Boolean = True
        Public Function Init() As Boolean


            ' instance core services

            ServiceHandler = New Vibranium.Services.ServiceHandler()

            VFS = New Vibranium.FileSystem.VirtualFileSystem()
            'ProcessManager = New ProcessManager()
            ' Ensure the shared Core module has the same runtime instances
            Try
                Vibranium.Core.VFS = VFS
                Vibranium.Core.ServiceHandler = ServiceHandler
                Vibranium.Core.ProcessManager = New Vibranium.Kernel.ProcessManager()
            Catch
            End Try

            ' assume that our built-in terminal is going to get replaced at somepoint too tbf.
            Apps.RegisterBuiltins.Startup()
            ' launch Services
            ServiceHandler.Init()
            Apps.RegisterBuiltins.StartApp("/bin/sh")
            Startup.Startup()
            Return True
        End Function
        Public Function MainLoop() As Boolean
            Try
                While (Running)
                    ' Tick services and the process tree each loop
                    If Core.ServiceHandler IsNot Nothing Then
                        Try
                            Core.ServiceHandler.Tick()
                        Catch
                        End Try
                    End If

                    If Core.ProcessManager IsNot Nothing Then
                        Try
                            Core.ProcessManager.Tick()
                        Catch
                        End Try

                        Dim p = Core.ProcessManager.GetProcess(1)
                        If p Is Nothing Then
                            ' PID 1 not present yet; continue looping
                        ElseIf p.IsRunning = False Then
                            Throw New KernelPanic("PID 1 stopped")
                        End If
                    End If
                    'System.Threading.Thread.Sleep(10)

                End While

            Catch ex As Exception
                Console.WriteLine(ex)
            End Try
            Return True
        End Function
    End Class
End Namespace
