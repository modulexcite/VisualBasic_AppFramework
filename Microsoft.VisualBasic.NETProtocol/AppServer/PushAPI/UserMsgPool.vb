﻿Imports Microsoft.VisualBasic.Net.Protocols

Namespace PushAPI

    ''' <summary>
    ''' 用来缓存消息信息的用户的消息池
    ''' </summary>
    Public Class UserMsgPool

        ''' <summary>
        ''' 按照先后顺序排列的用户消息队列
        ''' </summary>
        ReadOnly __msgs As New Dictionary(Of Long, Queue(Of RequestStream))

        ''' <summary>
        ''' 为新的用户分配存储空间
        ''' </summary>
        ''' <param name="uid"></param>
        Public Sub Allocation(uid As Long)
            Call __msgs.Add(uid, New Queue(Of RequestStream))
        End Sub

        ''' <summary>
        ''' 想用户消息池之中写入数据缓存
        ''' </summary>
        ''' <param name="uid"></param>
        ''' <param name="msg"></param>
        Public Sub Push(uid As Long, msg As RequestStream)
            Call __msgs(uid).Enqueue(msg)
        End Sub

        ''' <summary>
        ''' 读取一条数据
        ''' </summary>
        ''' <param name="uid"></param>
        ''' <returns></returns>
        Public Function Pop(uid As Long) As RequestStream
            Dim pool = __msgs(uid)
            If pool.Count = 0 Then
                Return Nothing
            Else
                Return pool.Dequeue()
            End If
        End Function
    End Class
End Namespace