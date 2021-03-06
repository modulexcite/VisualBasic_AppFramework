﻿Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Protocols

Namespace Serialization.BinaryDumping

    Public Structure Buffer : Implements ISerializable

        Dim Length As Long
        Dim buffer As Byte()

        Sub New(buf As Byte())
            Length = buf.Length
            buffer = buf
        End Sub

        Public ReadOnly Property TotalBytes As Long
            Get
                Return Length + RawStream.INT64
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{Length} bytes..."
        End Function

        Public Function Serialize() As Byte() Implements ISerializable.Serialize
            Dim buffer As Byte() = New Byte(TotalBytes - 1) {}
            Call Array.ConstrainedCopy(BitConverter.GetBytes(Length), Scan0, buffer, Scan0, RawStream.INT64)
            Call Array.ConstrainedCopy(Me.buffer, Scan0, buffer, RawStream.INT64, Me.buffer.Length)
            Return buffer
        End Function
    End Structure

    Public Delegate Function IGetBuffer(Of T)(x As T) As Byte()
    Public Delegate Function IGetObject(Of T)(buf As Byte()) As T

    ''' <summary>
    ''' 适用于对变长的流的操作
    ''' </summary>
    Public Module BufferAPI

        Public Function CreateBuffer(Of T)(source As IEnumerable(Of T), getBuf As IGetBuffer(Of T)) As Byte()
            Dim array As Buffer() = source.ToArray(Function(x) New Buffer(getBuf(x)))
            Dim buffer As Byte() = New Byte(array.Sum(Function(x) x.TotalBytes) - 1L) {}
            Dim i As Integer

            For Each x As Buffer In array
                Call System.Array.ConstrainedCopy(x.Serialize, Scan0, buffer, i, x.TotalBytes)
                i += x.TotalBytes
            Next

            Return buffer
        End Function

        Public Iterator Function GetBuffer(Of T)(raw As Byte(), getObj As IGetObject(Of T)) As IEnumerable(Of T)
            Dim length As Byte() = New Byte(RawStream.INT64 - 1) {}
            Dim l As Long
            Dim i As Long
            Dim temp As Byte()
            Dim x As T

            Do While True
                Call Array.ConstrainedCopy(raw, i.Move(RawStream.INT64), length, Scan0, RawStream.INT64)
                l = BitConverter.ToInt64(length, Scan0)
                temp = New Byte(l - 1) {}
                Call Array.ConstrainedCopy(raw, i.Move(l), temp, Scan0, l)
                x = getObj(temp)
                Yield x

                If i >= raw.Length - 1 Then
                    Exit Do
                End If
            Loop
        End Function
    End Module
End Namespace