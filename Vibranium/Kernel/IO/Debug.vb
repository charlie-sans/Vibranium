Public Module Debug
    Public Sub Log(ByVal ParamArray Itm As String())
        Dim stackTrace As New System.Diagnostics.StackTrace()
        Dim callingMethod As System.Reflection.MethodBase = stackTrace.GetFrame(1).GetMethod()
        Dim callerClassName As String = callingMethod.DeclaringType.Name

  
        If Itm IsNot Nothing Then
            Sys.IO.Write(My.Computer.Clock.LocalTime.ToString() + "]", " [", callerClassName, "]: ")
            For Each cts As String In Itm
                Sys.IO.Write(cts)
            Next
        Else
            Sys.IO.WriteLine(vbNewLine)
        End If
        Sys.IO.WriteLine(vbNewLine)
    End Sub
End Module
