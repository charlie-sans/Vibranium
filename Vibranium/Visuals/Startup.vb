Imports Vibranium.Vibranium

Public Module Startup
    Public Title As String = "  .·:'''''''''''''''''''''''''''''''''''''':·." + vbNewLine + ": : ░█░█░▀█▀░█▀▄░█▀▄░█▀█░█▀█░▀█▀░█░█░█▄█ : :" + vbNewLine + ": : ░▀▄▀░░█░░█▀▄░█▀▄░█▀█░█░█░░█░░█░█░█░█ : :" + vbNewLine + ": : ░░▀░░▀▀▀░▀▀░░▀░▀░▀░▀░▀░▀░▀▀▀░▀▀▀░▀░▀ : :" + vbNewLine + "'·:......................................:·" + vbNewLine


    Public Sub Startup()
        Try
            Sys.IO.SetConsoleColor(ConsoleColor.DarkYellow)
        Catch
        End Try

        Sys.IO.WriteLine(Title)

        Try
            Sys.IO.SetConsoleColor(ConsoleColor.White)
        Catch
        End Try

        ' Boot steps: show progress and initialize key subsystems.
        Dim steps As New System.Collections.Generic.List(Of String) From {"VFS", "IO", "ProcessManager", "Services", "Shell", "Console"}

        For Each St In steps
            Sys.IO.WriteLine("-> Initializing " & St & "...")
            Debug.Log("Initializing " & St)

            Try
                Select Case St
                    Case "VFS"
                        If Sys.VFS Is Nothing Then Sys.VFS = New VirtualFileSystem()
                    Case "IO"
                        If Sys.IO Is Nothing Then Sys.IO = New IOSystem()
                    Case "ProcessManager"
                        If Sys.ProcessManager Is Nothing Then Sys.ProcessManager = New ProcessManager()
                    Case "Services"
                        If Sys.ServiceHandler Is Nothing Then Sys.ServiceHandler = New ServiceHandler()
                        Try
                            Sys.ServiceHandler.Init()
                        Catch
                        End Try
                    Case "Shell"
                        Dim shellPid As Integer = -1
                        Try
                            If Sys.VFS IsNot Nothing AndAlso Sys.VFS.Exists("/bin/sh") Then
                                shellPid = Sys.ProcessManager.StartByPath(Sys.VFS, "/bin/sh", parentPID:=0)
                            End If
                        Catch
                        End Try

                        If shellPid = -1 Then
                            ' fallback: instantiate ShProcess directly
                            Try
                                Dim sh = New ShProcess()
                                shellPid = Sys.ProcessManager.CreateProcess(sh, parentPID:=0)
                            Catch
                            End Try
                        End If

                        If shellPid = -1 Then
                            Debug.Log("Shell failed to start")
                            Sys.IO.WriteLine("[warning] shell not started")
                        Else
                            Debug.Log("Shell started PID=" & shellPid)
                            Sys.IO.WriteLine("[ok] shell PID=" & shellPid)
                        End If
                    Case "Console"
                        Try
                            Dim k = New ShProcess()
                            Dim kp = Sys.ProcessManager.CreateProcess(k, parentPID:=0)
                            Debug.Log("Console started PID=" & kp)
                        Catch
                        End Try
                End Select
            Catch
            End Try

            System.Threading.Thread.Sleep(150)
        Next

        Sys.IO.WriteLine()
        Try
            Sys.IO.SetConsoleColor(ConsoleColor.Green)
        Catch
        End Try
        Sys.IO.WriteLine("Boot complete.")
        Try
            Sys.IO.SetConsoleColor(ConsoleColor.White)
        Catch
        End Try
    End Sub
End Module
