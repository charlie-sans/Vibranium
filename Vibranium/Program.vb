Module Program

    Sub Main(ByVal args As String())
        Dim Knl As Kernel = New Kernel()

        ' launch Services
        ServiceHandlerSystem.Init()

        ' run Kernel loop
        Knl.MainLoop()
    End Sub

End Module
