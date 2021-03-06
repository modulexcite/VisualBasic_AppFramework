﻿Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Net.Protocols.Reflection

Namespace Protocols

    Module InvokeAPI

        ''' <summary>
        ''' 其他的服务器模块向消息推送服务发送更新数据的协议
        ''' </summary>
        Public Enum Protocols
            ''' <summary>
            ''' Push data to user
            ''' </summary>
            PushToUser
        End Enum

        Public ReadOnly Property ProtocolEntry As Long =
            New Protocol(GetType(Protocols)).EntryPoint

        <Extension> Public Function PushData(data As Byte()) As RequestStream
            Return New RequestStream(ProtocolEntry, Protocols.PushToUser, data)
        End Function

        <Extension> Public Sub PushData(API As IPEndPoint, data As Byte())
            Dim req As RequestStream = data.PushData   ' 创建协议
            Dim invoke As AsynInvoke = New AsynInvoke(API)  ' 创建socket
            Dim rep As RequestStream = invoke.SendMessage(req) ' 发送消息
        End Sub
    End Module
End Namespace
