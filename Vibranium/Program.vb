Imports Vibranium.Vibranium

Module Program

    Sub Main(ByVal args As String())


        ' instance core services
        Sys.Kernel = New Kernel()
        Sys.ServiceHandler = New ServiceHandler()
        Sys.IO = New IOSystem()
        Sys.VFS = New VirtualFileSystem()
        Sys.ProcessManager = New ProcessManager()
        Sys.VFS.RegisterProcess("sh", Function(vpath) New ShProcess(), "/bin/sh")
        Debug.Log("registered SH to /bin/sh" + vbNewLine)

        ' start registered shell
        Dim shellPid = Sys.ProcessManager.StartByPath(Sys.VFS, "/bin/sh", parentPID:=0)
        If shellPid = -1 Then
            Debug.Log("Failed to start /bin/sh" & vbNewLine)
        Else
            Debug.Log("Started /bin/sh PID=" & shellPid & vbNewLine)
        End If


        ' launch Services
        Sys.ServiceHandler.Init()

   
        ' assume that our builtl in terminal is going to get replaced.
        Sys.Kernel.Init()

        ' run Kernel loop
        Sys.Kernel.MainLoop()
    End Sub

End Module
