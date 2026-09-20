Imports Vibranium
Namespace Apps
    Module RegisterBuiltins
        Sub Startup()
            ' Register shell process so /bin/sh resolves to the built-in shell
            Register("sh", "/bin/sh", Function(vpath) New Apps.ShProcess())
            Register("about", "/bin/about", Function(vpath) New Apps.AboutWindow())
        End Sub
        Sub Register(ByVal Name As String, ByVal path As String, ByVal factory As Func(Of String, IProcess))
            Try
                VFS.RegisterProcess(Name, factory, path)
                Debug.Log(LogLevel.Info, "registered " + Name + " to " + path + vbNewLine)
            Catch
                Debug.Log(LogLevel.Err, "Failed to register " + Name + " to " + path + vbNewLine)
            End Try

        End Sub
        Function StartApp(ByVal path As String, Optional ByVal PPID As Integer = 0)
            Try
                ' start builtin shell *can be replaced but not now
                Dim Pid = ProcessManager.StartByPath(Core.VFS, path, parentPID:=PPID)
                If Pid = -1 Then
                    Debug.Log(LogLevel.Err, "Failed to start " & path & vbNewLine)
                    Return False
                Else
                    Debug.Log(LogLevel.Info, "Started" & path & " PID=" & Pid & vbNewLine)
                    Return True
                End If

            Catch
            End Try
            Return False
        End Function
    End Module

End Namespace