
Imports Vibranium.Kernel, Vibranium

Module Program

    Sub Main(ByVal args As String())
        Vibranium.Core.Kernel = New Kernel()
        Core.Kernel.Init()

        ' run Kernel loop
        Core.Kernel.MainLoop()
    End Sub

End Module
