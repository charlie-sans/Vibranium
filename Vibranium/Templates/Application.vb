Public Interface Application

    Property Name As String
    Property Description As String
    Property Version As String
    ''' <summary>
    ''' runs when the app is started, Use this if you want a free update loop and when Update doesn't make sense.
    ''' </summary>
    ''' <remarks></remarks>
    Sub Init()

    ''' <summary>
    ''' runs when the app is updated in the SysProc updating loop
    ''' </summary>
    ''' <remarks></remarks>
    Sub Update()

    ''' <summary>
    ''' Runs when the app is either unloaded / stopped or killed from the SysProc tree
    ''' </summary>
    ''' <remarks></remarks>
    Sub Deconstruct()

End Interface
