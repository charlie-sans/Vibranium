Public Interface Service
    Property Name As String
    Property Instance
    Property ServiceFilePath As String ' TODO: Replace with proper VFS implementation.

    Sub Init()
    Sub Update()
    Sub DeInit()
End Interface