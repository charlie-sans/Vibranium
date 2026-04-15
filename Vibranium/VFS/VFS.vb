Imports System.IO
Imports System.Collections.Generic
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Json
Imports System.Text

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
    
    ' --- VFS metadata: symlinks and permissions ---
    <DataContract>
    Private Class VFSMetadata
        <DataMember>
        Public Property Symlinks As Dictionary(Of String, String)
        <DataMember>
        Public Property Permissions As Dictionary(Of String, PermissionSet)

        Public Sub New()
            Symlinks = New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
            Permissions = New Dictionary(Of String, PermissionSet)(StringComparer.OrdinalIgnoreCase)
        End Sub
    End Class

    <DataContract>
    Public Class PermissionSet
        <DataMember>
        Public Property Read As Boolean
        <DataMember>
        Public Property Write As Boolean
        <DataMember>
        Public Property Execute As Boolean

        Public Sub New()
        End Sub
        Public Sub New(r As Boolean, w As Boolean, x As Boolean)
            Read = r
            Write = w
            Execute = x
        End Sub
    End Class

    Private _metadata As VFSMetadata = Nothing
    Private ReadOnly Property MetadataPath As String
        Get
            Return Path.Combine(_rootPath, "vfs_metadata.json")
        End Get
    End Property

    Private Sub EnsureMetadataLoaded()
        If _metadata IsNot Nothing Then Return
        If System.IO.File.Exists(MetadataPath) Then
            Try
                Dim ser = New DataContractJsonSerializer(GetType(VFSMetadata))
                Using fs = System.IO.File.OpenRead(MetadataPath)
                    _metadata = CType(ser.ReadObject(fs), VFSMetadata)
                End Using
            Catch
                _metadata = New VFSMetadata()
            End Try
        Else
            _metadata = New VFSMetadata()
        End If
    End Sub

    Private Sub SaveMetadata()
        Try
            Dim ser = New DataContractJsonSerializer(GetType(VFSMetadata))
            Using fs = System.IO.File.Create(MetadataPath)
                ser.WriteObject(fs, _metadata)
            End Using
        Catch
        End Try
    End Sub

    ' Create a symlink in VFS: virtualPath -> (type:value), e.g. type=function,value=Bash or type=path,value=/bin/bash.dll
    Public Sub CreateSymlink(ByVal virtualPath As String, ByVal linkType As String, ByVal value As String)
        EnsureMetadataLoaded()
        Dim key = virtualPath.Replace("\", "/")
        Dim stored = linkType & ":" & value
        _metadata.Symlinks(key) = stored
        SaveMetadata()
    End Sub

    Public Class SymlinkTarget
        Public Property Type As String
        Public Property Value As String
    End Class

    Public Function ResolveSymlink(ByVal virtualPath As String) As SymlinkTarget
        EnsureMetadataLoaded()
        Dim key = virtualPath.Replace("\", "/")
        If _metadata.Symlinks.ContainsKey(key) Then
            Dim stored = _metadata.Symlinks(key)
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

    ' Expose physical path resolution for other components
    Public Function MapToPhysical(ByVal virtualPath As String) As String
        Return GetPhysicalPath(virtualPath)
    End Function

    ' Permissions management
    Public Sub SetPermissions(ByVal virtualDir As String, ByVal read As Boolean, ByVal write As Boolean, ByVal execute As Boolean)
        EnsureMetadataLoaded()
        Dim dirKey = virtualDir.Replace("\", "/")
        _metadata.Permissions(dirKey) = New PermissionSet(read, write, execute)
        SaveMetadata()
    End Sub

    Public Function GetPermissions(ByVal virtualDir As String) As PermissionSet
        EnsureMetadataLoaded()
        Dim dirKey = virtualDir.Replace("\", "/")
        If _metadata.Permissions.ContainsKey(dirKey) Then
            Return _metadata.Permissions(dirKey)
        End If
        Return New PermissionSet(True, True, True)
    End Function

    Public Function CheckPermission(ByVal virtualPath As String, ByVal perm As String) As Boolean
        ' perm: "r", "w", "x"
        Dim dir = virtualPath.Replace("\", "/")
        If Not dir.EndsWith("/") Then
            Dim idx = dir.LastIndexOf("/"c)
            If idx >= 0 Then dir = dir.Substring(0, idx + 1) Else dir = "/"
        End If
        Dim ps = GetPermissions(dir)
        Select Case perm.ToLower()
            Case "r"
                Return ps.Read
            Case "w"
                Return ps.Write
            Case "x"
                Return ps.Execute
            Case Else
                Return False
        End Select
    End Function
End Class