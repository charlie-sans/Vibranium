
Imports Vibranium.Kernel, Vibranium
Public Module Startup
    Public Title As String = "  .·:'''''''''''''''''''''''''''''''''''''':·." + vbNewLine + ": : ░█░█░▀█▀░█▀▄░█▀▄░█▀█░█▀█░▀█▀░█░█░█▄█ : :" + vbNewLine + ": : ░▀▄▀░░█░░█▀▄░█▀▄░█▀█░█░█░░█░░█░█░█░█ : :" + vbNewLine + ": : ░░▀░░▀▀▀░▀▀░░▀░▀░▀░▀░▀░▀░▀▀▀░▀▀▀░▀░▀ : :" + vbNewLine + "'·:......................................:·" + vbNewLine




    Public Sub Startup()
        Try
            IO.SetConsoleColor(ConsoleColor.DarkYellow)
        Catch
        End Try

        IO.WriteLine(Title)

        Try
            IO.SetConsoleColor(ConsoleColor.White)
        Catch
        End Try

        ' Boot steps: show progress and initialize key subsystems.
        Dim steps As New System.Collections.Generic.List(Of String) From {"VFS", "IO", "ProcessManager", "Services", "Console"}

        For Each St In steps
            IO.WriteLine("-> Initializing " & St & "...")
            Vibranium.Debug.Log("Initializing " & St)

            Try
                Select Case St
                    Case "VFS"
                        If VFS Is Nothing Then Program.VFS = New Vibranium.FileSystem.VirtualFileSystem()
   
                    Case "ProcessManager"
                        If Core.ProcessManager Is Nothing Then


                            Core.ProcessManager = New Vibranium.Kernel.ProcessManager.ProcessManager()

                            IO.WriteLine("started Processmanager")
                        End If
                    Case "Services"
                        If Core.ServiceHandler Is Nothing Then Core.ServiceHandler = New Vibranium.Services.ServiceHandler()
                        Try
                            ServiceHandler.Init()
                        Catch
                        End Try
                    Case "Console"
                        Try
                            ' start registered shell
                            Dim shellPid = ProcessManager.StartByPath(VFS, "/bin/sh", parentPID:=0)
                            If shellPid = -1 Then
                                Debug.Log("Failed to start /bin/sh" & vbNewLine)
                            Else
                                Debug.Log("Started /bin/sh PID=" & shellPid & vbNewLine)
                            End If
                        Catch
                        End Try
                End Select
            Catch
            End Try

            System.Threading.Thread.Sleep(150)
        Next

        IO.WriteLine()
        Try
            IO.SetConsoleColor(ConsoleColor.Green)
        Catch
        End Try
        IO.WriteLine("Boot complete." + vbNewLine)
        Try
            IO.SetConsoleColor(ConsoleColor.White)
        Catch
        End Try

    End Sub
End Module
