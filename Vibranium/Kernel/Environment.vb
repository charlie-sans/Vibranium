Imports System.Threading
Imports Vibranium

Namespace Vibranium.Sys
    Public Module Environment
        ' Maps thread IDs to process IDs
        Private threadToPid As New Dictionary(Of Integer, Integer)

        ' Call this when starting a process/thread
        Public Sub RegisterThread(ByVal pid As Integer)
            Dim tid = Thread.CurrentThread.ManagedThreadId
            SyncLock threadToPid
                threadToPid(tid) = pid
            End SyncLock
        End Sub

        ' Call this when ending a process/thread
        Public Sub UnregisterThread()
            Dim tid = Thread.CurrentThread.ManagedThreadId
            SyncLock threadToPid
                If threadToPid.ContainsKey(tid) Then
                    threadToPid.Remove(tid)
                End If
            End SyncLock
        End Sub

        ' Exits the current process (thread)
        Public Sub ExitCurrentProcess()
            Dim tid = Thread.CurrentThread.ManagedThreadId
            Dim pid As Integer = -1
            SyncLock threadToPid
                If threadToPid.ContainsKey(tid) Then
                    pid = threadToPid(tid)
                End If
            End SyncLock
            If pid >= 0 AndAlso Core.ProcessManager IsNot Nothing Then
                Core.ProcessManager.KillProcess(pid)
            End If
        End Sub

        ' Optionally, get the current process node
        Public Function GetCurrentProcessNode() As Object
            Dim tid = Thread.CurrentThread.ManagedThreadId
            Dim pid As Integer = -1
            SyncLock threadToPid
                If threadToPid.ContainsKey(tid) Then
                    pid = threadToPid(tid)
                End If
            End SyncLock
            If pid >= 0 AndAlso Core.ProcessManager IsNot Nothing Then
                Return Core.ProcessManager.GetProcess(pid)
            End If
            Return Nothing
        End Function
    End Module
End Namespace
