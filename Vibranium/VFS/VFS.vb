Imports System.IO
Imports System.Collections.Generic

Public Class VirtualFileSystem
    Private ReadOnly _rootPath As String
    Private ReadOnly _virtualRoot As String = "/"

    Public Sub New()
        _rootPath = AppDomain.CurrentDomain.BaseDirectory
    End Sub

    ' Check if a path exists (file or directory)
    Public Function Exists(ByVal virtualPath As String) As Boolean
        Dim physicalPath = GetPhysicalPath(virtualPath)
        Return System.IO.File.Exists(physicalPath) OrElse System.IO.Directory.Exists(physicalPath)
    End Function

    ' Check if path is a directory
    Public Function IsDirectory(ByVal virtualPath As String) As Boolean
        Dim physicalPath = GetPhysicalPath(virtualPath)
        Return System.IO.Directory.Exists(physicalPath)
    End Function

    ' Check if path is a file
    Public Function IsFile(ByVal virtualPath As String) As Boolean
        Dim physicalPath = GetPhysicalPath(virtualPath)
        Return System.IO.File.Exists(physicalPath)
    End Function

    ' Read all text from a file
    Public Function ReadAllText(ByVal virtualPath As String) As String
        ValidateFilePath(virtualPath)
        Dim physicalPath = GetPhysicalPath(virtualPath)
        Return System.IO.File.ReadAllText(physicalPath)
    End Function

    ' Write text to a file
    Public Sub WriteAllText(ByVal virtualPath As String, ByVal content As String)
        ValidateFilePathForWrite(virtualPath)
        Dim physicalPath = GetPhysicalPath(virtualPath)

        ' Ensure directory exists
        Dim directory = Path.GetDirectoryName(physicalPath)
        If Not System.IO.Directory.Exists(directory) Then
            System.IO.Directory.CreateDirectory(directory)
        End If

        System.IO.File.WriteAllText(physicalPath, content)
    End Sub

    ' Read all bytes from a file
    Public Function ReadAllBytes(ByVal virtualPath As String) As Byte()
        ValidateFilePath(virtualPath)
        Dim physicalPath = GetPhysicalPath(virtualPath)
        Return System.IO.File.ReadAllBytes(physicalPath)
    End Function

    ' Write bytes to a file
    Public Sub WriteAllBytes(ByVal virtualPath As String, ByVal content As Byte())
        ValidateFilePathForWrite(virtualPath)
        Dim physicalPath = GetPhysicalPath(virtualPath)

        Dim directory = Path.GetDirectoryName(physicalPath)
        If Not System.IO.Directory.Exists(directory) Then
            System.IO.Directory.CreateDirectory(directory)
        End If

        System.IO.File.WriteAllBytes(physicalPath, content)
    End Sub

    ' Delete a file or directory
    Public Sub Delete(ByVal virtualPath As String)
        Dim physicalPath = GetPhysicalPath(virtualPath)

        If System.IO.File.Exists(physicalPath) Then
            System.IO.File.Delete(physicalPath)
        ElseIf System.IO.Directory.Exists(physicalPath) Then
            System.IO.Directory.Delete(physicalPath, True)
        End If
    End Sub

    ' Create a directory
    Public Sub CreateDirectory(ByVal virtualPath As String)
        Dim physicalPath = GetPhysicalPath(virtualPath)
        If Not System.IO.Directory.Exists(physicalPath) Then
            System.IO.Directory.CreateDirectory(physicalPath)
        End If
    End Sub

    ' Get files in a directory
    Public Function GetFiles(ByVal virtualPath As String, Optional ByVal searchPattern As String = "*") As List(Of String)
        ValidateDirectoryPath(virtualPath)
        Dim physicalPath = GetPhysicalPath(virtualPath)

        Dim files = System.IO.Directory.GetFiles(physicalPath, searchPattern)
        Dim virtualFiles As New List(Of String)

        For Each file In files
            virtualFiles.Add(GetVirtualPath(file))
        Next

        Return virtualFiles
    End Function

    ' Get directories in a directory
    Public Function GetDirectories(ByVal virtualPath As String, Optional ByVal searchPattern As String = "*") As List(Of String)
        ValidateDirectoryPath(virtualPath)
        Dim physicalPath = GetPhysicalPath(virtualPath)

        Dim directories = System.IO.Directory.GetDirectories(physicalPath, searchPattern)
        Dim virtualDirs As New List(Of String)

        For Each dir As String In directories
            virtualDirs.Add(GetVirtualPath(dir))
        Next

        Return virtualDirs
    End Function

    ' Copy a file
    Public Sub CopyFile(ByVal sourceVirtualPath As String, ByVal destVirtualPath As String, Optional ByVal overwrite As Boolean = False)
        ValidateFilePath(sourceVirtualPath)

        Dim sourcePhysical = GetPhysicalPath(sourceVirtualPath)
        Dim destPhysical = GetPhysicalPath(destVirtualPath)

        Dim destDir = Path.GetDirectoryName(destPhysical)
        If Not System.IO.Directory.Exists(destDir) Then
            System.IO.Directory.CreateDirectory(destDir)
        End If

        System.IO.File.Copy(sourcePhysical, destPhysical, overwrite)
    End Sub

    ' Move a file
    Public Sub MoveFile(ByVal sourceVirtualPath As String, ByVal destVirtualPath As String)
        ValidateFilePath(sourceVirtualPath)

        Dim sourcePhysical = GetPhysicalPath(sourceVirtualPath)
        Dim destPhysical = GetPhysicalPath(destVirtualPath)

        Dim destDir = Path.GetDirectoryName(destPhysical)
        If Not System.IO.Directory.Exists(destDir) Then
            System.IO.Directory.CreateDirectory(destDir)
        End If

        System.IO.File.Move(sourcePhysical, destPhysical)
    End Sub

    ' Get file info
    Public Function GetFileInfo(ByVal virtualPath As String) As FileInfo
        ValidateFilePath(virtualPath)
        Dim physicalPath = GetPhysicalPath(virtualPath)
        Return New FileInfo(physicalPath)
    End Function

    ' Get directory info
    Public Function GetDirectoryInfo(ByVal virtualPath As String) As DirectoryInfo
        ValidateDirectoryPath(virtualPath)
        Dim physicalPath = GetPhysicalPath(virtualPath)
        Return New DirectoryInfo(physicalPath)
    End Function

    ' Convert virtual path to physical path
    Private Function GetPhysicalPath(ByVal virtualPath As String) As String
        ' Normalize the virtual path
        Dim normalizedPath = virtualPath.Replace("\", "/")

        ' Remove leading slash if present
        If normalizedPath.StartsWith("/") Then
            normalizedPath = normalizedPath.Substring(1)
        End If

        ' Handle root path
        If String.IsNullOrEmpty(normalizedPath) Then
            normalizedPath = ""
        End If

        ' Combine with root path
        Dim physicalPath = Path.Combine(_rootPath, normalizedPath)

        ' Validate that the path is within the root directory (security)
        Dim fullPath = Path.GetFullPath(physicalPath)
        Dim fullRoot = Path.GetFullPath(_rootPath)

        If Not fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase) Then
            Throw New UnauthorizedAccessException("Access denied: Path is outside the virtual file system root.")
        End If

        Return fullPath
    End Function

    ' Convert physical path to virtual path (compatible with .NET 4)
    Private Function GetVirtualPath(ByVal physicalPath As String) As String
        ' Calculate relative path manually for .NET 4 compatibility
        Dim relativePath As String = ""

        If physicalPath.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase) Then
            relativePath = physicalPath.Substring(_rootPath.Length)
            ' Remove leading backslash
            If relativePath.StartsWith("\") Then
                relativePath = relativePath.Substring(1)
            End If
        Else
            relativePath = physicalPath
        End If

        Return "/" + relativePath.Replace("\", "/")
    End Function

    ' Validate that a path exists and is a file
    Private Sub ValidateFilePath(ByVal virtualPath As String)
        Dim physicalPath = GetPhysicalPath(virtualPath)
        If Not System.IO.File.Exists(physicalPath) Then
            Throw New FileNotFoundException("File not found: " & virtualPath)
        End If
    End Sub

    ' Validate path for write operations (file may not exist yet)
    Private Sub ValidateFilePathForWrite(ByVal virtualPath As String)
        If String.IsNullOrEmpty(virtualPath) Then
            Throw New ArgumentException("Path cannot be empty")
        End If

        ' Check if it's trying to write to a directory path
        If virtualPath.EndsWith("/") OrElse virtualPath.EndsWith("\") Then
            Throw New ArgumentException("Path appears to be a directory, not a file")
        End If
    End Sub

    ' Validate that a path exists and is a directory
    Private Sub ValidateDirectoryPath(ByVal virtualPath As String)
        Dim physicalPath = GetPhysicalPath(virtualPath)
        If Not System.IO.Directory.Exists(physicalPath) Then
            Throw New DirectoryNotFoundException("Directory not found: " & virtualPath)
        End If
    End Sub
End Class