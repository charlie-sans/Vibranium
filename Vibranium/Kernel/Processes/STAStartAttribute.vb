Imports System

Namespace Vibranium.Kernel

    <AttributeUsage(AttributeTargets.Method Or AttributeTargets.Class, Inherited:=True, AllowMultiple:=False)>
    Public Class STAStartAttribute
        Inherits Attribute
    End Class

End Namespace
