﻿Module Module1

    Sub Main()

        Pause()

        Dim user As New Microsoft.VisualBasic.Net.NETProtocol.User(New Microsoft.VisualBasic.Net.IPEndPoint("127.0.0.1", 6354), 1234)
        AddHandler user.PushMessage, Sub(x)
                                         Call x.GetUTF8String.__DEBUG_ECHO
                                     End Sub


        Pause()

    End Sub

End Module
