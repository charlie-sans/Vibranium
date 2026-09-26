
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

        If Core.ProcessManager Is Nothing Then
            Core.ProcessManager = New ProcessManager()
            Debug.Log(LogLevel.Warning, "Core.Process manager was null!")
        End If
        If Core.ServiceHandler Is Nothing Then Core.ServiceHandler = New Vibranium.Services.ServiceHandler()
        Try
            ServiceHandler.Init()
        Catch
        End Try
        If Core.ProcessManager Is Nothing Then

            Core.ProcessManager = New ProcessManager()

            IO.WriteLine("started Processmanager")
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
        End If

    End Sub
End Module
