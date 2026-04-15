Imports System.IO

Public Class IOSystem
    Dim Input As TextReader = Console.In
    Dim Out As TextWriter = Console.Out

    Public Function Read(Optional ByVal chars As String = "")
        Out.Write(chars)
        Return Input.Read()
    End Function
    Public Function ReadLine(Optional ByVal chars As String = "")
        Out.Write(chars)
        Return Input.ReadLine()
    End Function

    Public Sub Write(ByVal ParamArray input As Object())
        For Each i In input
            Out.Write(i.ToString())
        Next
    End Sub

    Public Sub WriteLine(ByVal ParamArray input As Object())
        For Each Item In input
            Out.Write(Item.ToString())
        Next
    End Sub
    Public Sub SetConsoleColor(ByVal Color As ConsoleColor)
        Console.ForegroundColor = Color
    End Sub
End Class
