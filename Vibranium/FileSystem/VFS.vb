Imports System.IO
Imports System.Collections.Generic

' ==========================================================
'  VIRTUAL FILE SYSTEM
'  Supports:
'   - Physical files/folders
'   - Virtual directories
'   - Virtual text files
'   - Generated files (/proc style)
'   - Executable nodes (/bin/bash style)
' ==========================================================
Namespace Vibranium


    Public Class VirtualFileSystem

        Private ReadOnly _rootPath As String

        Private ReadOnly _virtualNodes As New Dictionary(Of String, VNode)(StringComparer.OrdinalIgnoreCase)

        Public Sub New()
            _rootPath = AppDomain.CurrentDomain.BaseDirectory

            ' register root
            RegisterNode("/", New DirectoryNode("/"))
        End Sub

#Region "NODE REGISTRATION"

        Public Sub RegisterExecutable(ByVal path As String, ByVal action As Action(Of String()))
            RegisterNode(path, New VExecutableNode(path, action))
        End Sub

        Public Sub RegisterFile(ByVal path As String, ByVal content As String)
            RegisterNode(path, New FileNode(path, content))
        End Sub

        Public Sub RegisterGeneratedFile(ByVal path As String, ByVal generator As Func(Of String))
            RegisterNode(path, New VGeneratedNode(path, generator))
        End Sub

        Public Sub RegisterDirectory(ByVal path As String)
            RegisterNode(path, New DirectoryNode(path))
        End Sub

        Public Sub RegisterNode(ByVal path As String, ByVal node As VNode)
            path = NormalizePath(path)
            _virtualNodes(path) = node
        End Sub

        ' --- Simple in-memory symlink support for process registration ---
        Private ReadOnly _symlinks As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)

        Public Sub CreateSymlink(ByVal virtualPath As String, ByVal linkType As String, ByVal value As String)
            Dim key = NormalizePath(virtualPath)
            _symlinks(key) = linkType & ":" & value
        End Sub

        Public Class SymlinkTarget
            Public Property Type As String
            Public Property Value As String
        End Class

        Public Function ResolveSymlink(ByVal virtualPath As String) As SymlinkTarget
            Dim key = NormalizePath(virtualPath)
            If _symlinks.ContainsKey(key) Then
                Dim stored = _symlinks(key)
                Dim idx = stored.IndexOf(":"c)
                If idx >= 0 Then
                    Dim t = stored.Substring(0, idx)
                    Dim v = stored.Substring(idx + 1)
                    Return New SymlinkTarget() With {.Type = t, .Value = v}
                Else
                    Return New SymlinkTarget() With {.Type = "path", .Value = stored}
                End If
            End If
            Return Nothing
        End Function

        ' Expose physical path resolution for components that need it
        Public Function MapToPhysical(ByVal virtualPath As String) As String
            Return GetPhysicalPath(virtualPath)
        End Function

        ' Convenience: register an IProcess factory and create a VFS symlink to it
        Public Sub RegisterProcess(ByVal name As String, ByVal factory As Func(Of String, IProcess), ByVal virtualPath As String)
            If String.IsNullOrEmpty(name) Then Throw New ArgumentNullException("name")
            If factory Is Nothing Then Throw New ArgumentNullException("factory")
            ProgramRegistry.Instance.Register(name, factory)
            CreateSymlink(virtualPath, "function", name)
        End Sub

#End Region

#Region "LOOKUP"

        Public Function Exists(ByVal virtualPath As String) As Boolean
            virtualPath = NormalizePath(virtualPath)

            If _virtualNodes.ContainsKey(virtualPath) Then Return True

            ' consider symlink entries (used for RegisterProcess)
            Try
                If ResolveSymlink(virtualPath) IsNot Nothing Then
                    Return True
                End If
            Catch
            End Try

            Dim physical = GetPhysicalPath(virtualPath)
            Return File.Exists(physical) OrElse Directory.Exists(physical)
        End Function

        Public Function Resolve(ByVal virtualPath As String) As VNode
            virtualPath = NormalizePath(virtualPath)

            If _virtualNodes.ContainsKey(virtualPath) Then
                Return _virtualNodes(virtualPath)
            End If

            Dim physical = GetPhysicalPath(virtualPath)

            If Directory.Exists(physical) Then
                Return New DirectoryNode(virtualPath)
            End If

            If File.Exists(physical) Then
                Return New FileNode(virtualPath, physical)
            End If

            Return Nothing
        End Function

#End Region

#Region "READ / WRITE"

        Public Function ReadAllText(ByVal virtualPath As String) As String
            Dim node = Resolve(virtualPath)

            If node Is Nothing Then
                Throw New FileNotFoundException(virtualPath)
            End If

            Return node.ReadText()
        End Function

        Public Sub WriteAllText(ByVal virtualPath As String, ByVal content As String)

            Dim node = Resolve(virtualPath)

            If node IsNot Nothing Then
                node.WriteText(content)
                Return
            End If

            ' fallback to physical disk
            Dim physical = GetPhysicalPath(virtualPath)
            Dim dir = Path.GetDirectoryName(physical)

            If Not Directory.Exists(dir) Then
                Directory.CreateDirectory(dir)
            End If

            File.WriteAllText(physical, content)
        End Sub

#End Region

#Region "EXECUTION"

        Public Sub Execute(ByVal virtualPath As String, ByVal args As String())

            Dim node = Resolve(virtualPath)

            If node Is Nothing Then
                Throw New Exception("Program not found: " & virtualPath)
            End If

            node.Execute(args)
        End Sub

#End Region

#Region "DIRECTORY LISTING"

        Public Function GetEntries(ByVal virtualPath As String) As List(Of String)

            virtualPath = NormalizePath(virtualPath)

            Dim output As New List(Of String)

            ' Physical files
            Dim physical = GetPhysicalPath(virtualPath)

            If Directory.Exists(physical) Then

                For Each Dir As String In Directory.GetDirectories(physical)
                    output.Add(Path.GetFileName(Dir))
                Next

                For Each file In Directory.GetFiles(physical)
                    output.Add(Path.GetFileName(file))
                Next

            End If

            ' Virtual files
            For Each kv In _virtualNodes

                If kv.Key <> "/" Then

                    Dim parent = GetParentPath(kv.Key)

                    If String.Equals(parent, virtualPath, StringComparison.OrdinalIgnoreCase) Then
                        output.Add(GetName(kv.Key))
                    End If

                End If

            Next

            Return output

        End Function

#End Region

#Region "PATH HELPERS"

        Private Function NormalizePath(ByVal path As String) As String

            If String.IsNullOrEmpty(path) Then Return "/"

            path = path.Replace("\", "/")

            If Not path.StartsWith("/") Then
                path = "/" & path
            End If

            While path.Contains("//")
                path = path.Replace("//", "/")
            End While

            If path.Length > 1 AndAlso path.EndsWith("/") Then
                path = path.Substring(0, path.Length - 1)
            End If

            Return path

        End Function

        Private Function GetParentPath(ByVal path As String) As String

            path = NormalizePath(path)

            If path = "/" Then Return "/"

            Dim i = path.LastIndexOf("/"c)

            If i <= 0 Then Return "/"

            Return path.Substring(0, i)

        End Function

        Private Function GetName(ByVal path As String) As String

            path = NormalizePath(path)

            If path = "/" Then Return "/"

            Dim i = path.LastIndexOf("/"c)

            Return path.Substring(i + 1)

        End Function

        Private Function GetPhysicalPath(ByVal virtualPath As String) As String

            virtualPath = NormalizePath(virtualPath)

            If virtualPath.StartsWith("/") Then
                virtualPath = virtualPath.Substring(1)
            End If

            Dim combined = Path.Combine(_rootPath, virtualPath)
            Return Path.GetFullPath(combined)

        End Function

#End Region

    End Class





End Namespace