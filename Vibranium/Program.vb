
Imports Vibranium.Kernel, Vibranium

Module Program

    Public Kernel As Vibranium.Kernel.Kernel
    Public ServiceHandler As Vibranium.Services.ServiceHandler

    Public VFS As Vibranium.FileSystem.VirtualFileSystem
    Public ProcessManager As Vibranium.Kernel.ProcessManager.ProcessManager

    Sub Main(ByVal args As String())
        ' instance core services
        Kernel = New Vibranium.Kernel.Kernel()
        ServiceHandler = New Vibranium.Services.ServiceHandler()

        VFS = New Vibranium.FileSystem.VirtualFileSystem()
        ProcessManager = New Vibranium.Kernel.ProcessManager.ProcessManager()
        ' Ensure the shared Core module has the same runtime instances
        Try
            Vibranium.Core.VFS = VFS
            Vibranium.Core.ServiceHandler = ServiceHandler
            Vibranium.Core.Kernel = Kernel
            Vibranium.Core.ProcessManager = ProcessManager
        Catch
        End Try
        ' Register shell process if needed (update as appropriate)
        ' VFS.RegisterProcess("sh", Function(vpath) New ShProcess(), "/bin/sh")
        Debug.Log("registered SH to /bin/sh" + vbNewLine)

        ' launch Services
        ServiceHandler.Init()

        ' assume that our built-in terminal is going to get replaced.
        Kernel.Init()

        ' run Kernel loop
        Kernel.MainLoop()
    End Sub

End Module
