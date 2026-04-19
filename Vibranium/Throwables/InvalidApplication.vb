<Serializable()>
Public Class InvalidApplicationException
    Inherits Exception

    ' 1. Default constructor
    Public Sub New()
        MyBase.New("App does not contain required framework")
    End Sub

    ' 2. Constructor that accepts a custom message
    Public Sub New(message As String)
        MyBase.New(message)
    End Sub

    ' 3. Constructor that accepts a message and an inner exception
    Public Sub New(message As String, inner As Exception)
        MyBase.New(message, inner)
    End Sub

End Class
