Imports Vibranium
Namespace Apps
    Module RegisterBuiltins
        Sub Startup()
            ' Register shell process so /bin/sh resolves to the built-in shell
            Register("sh", "/bin/sh", Function(vpath) New Apps.ShProcess())
            Register("about", "/bin/about", Function(vpath) New Apps.AboutWindow())
            Try
                ' start builtin shell *can be replaced but not now
                Dim shellPid = ProcessManager.StartByPath(VFS, "/bin/sh", parentPID:=0)
                If shellPid = -1 Then
                    Debug.Log(LogLevel.Err, "Failed to start /bin/sh" & vbNewLine)
                Else
                    Debug.Log(LogLevel.Info, "Started /bin/sh PID=" & shellPid & vbNewLine)
                End If
            Catch
            End Try
        End Sub
        Sub Register(ByVal Name As String, ByVal path As String, ByVal factory As Func(Of String, IProcess))
            Try
                VFS.RegisterProcess(Name, factory, path)
                Debug.Log(LogLevel.Info, "registered " + Name + " to " + path + vbNewLine)
            Catch
                Debug.Log(LogLevel.Err, "Failed to register " + Name + " to " + path + vbNewLine)
            End Try

        End Sub
    End Module

End Namespace