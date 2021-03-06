﻿Imports System.IO.MemoryMappedFiles
Imports Microsoft.VisualBasic.Parallel

Namespace MMFProtocol.MapStream

    Public MustInherit Class IMapBase : Implements IDisposable

        Public ReadOnly Property URI As String

        Protected _chunkBuffer As Byte()
        Protected _mmfileStream As MemoryMappedFile

        Sub New(uri As String, ChunkSize As Long)
            Me._URI = uri
            Me._chunkBuffer = New Byte(ChunkSize - 1) {}
        End Sub

        Public Function Read() As MMFStream
            Call _mmfileStream.CreateViewStream.Read(_chunkBuffer, Scan0, _chunkBuffer.Length)
            Return New MMFStream(_chunkBuffer)
        End Function

        Public Overrides Function ToString() As String
            Return URI
        End Function

#Region "IDisposable Support"
        Protected disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class

    Public Class MSIOReader : Inherits IMapBase
        Implements IDisposable

        ''' <summary>
        ''' 内存映射文件的更新标识符
        ''' </summary>
        ''' <remarks></remarks>
        Dim _udtBadge As Long
        Dim _mappedStream As MMFStream

        ReadOnly _dataArrivals As DataArrival

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="uri"></param>
        ''' <param name="callback"></param>
        ''' <param name="ChunkSize">内存映射文件的数据块的预分配大小</param>
        Sub New(uri As String, callback As DataArrival, ChunkSize As Long)
            Call MyBase.New(uri, ChunkSize)
            _mmfileStream = MemoryMappedFile.OpenExisting(uri)
            _dataArrivals = callback

            Call Parallel.RunTask(AddressOf __threadElapsed)
        End Sub

        Public Overrides Function ToString() As String
            Return $"{URI} ===> {NameOf(_udtBadge)}:={_udtBadge}"
        End Function

        Public Sub Update(thisUpdate As Long)
            Me._udtBadge = thisUpdate
        End Sub

        ''' <summary>
        ''' 由于考虑到可能会传递很大的数据块，所以在这里检测数据更新的话只读取头部的8个字节的数据
        ''' </summary>
        ''' <returns></returns>
        Public Function ReadBadge() As Long
            Dim buf As Byte() = New Byte(MMFStream.INT64 - 1) {}
            Call _mmfileStream.CreateViewStream.Read(buf, Scan0, buf.Length)
            Dim n As Long = BitConverter.ToInt64(buf, Scan0)
            Return n
        End Function

        Private Sub __threadElapsed()
            Do While Not Me.disposedValue
                Call __clientThreadElapsed()
                Call Threading.Thread.Sleep(1)
            Loop
        End Sub

        Private Sub __clientThreadElapsed()
            Dim flag As Long = ReadBadge()

            If flag <= Me._udtBadge Then
                Return
            Else ' 当从数据流中所读取到的更新标识符大于对象实例中的更新标识符的时候，认为数据发生了更新
                Me._mappedStream = Read()
                Me._udtBadge = _mappedStream.udtBadge
                Me._dataArrivals(_mappedStream.byteData)
            End If
        End Sub
    End Class
End Namespace