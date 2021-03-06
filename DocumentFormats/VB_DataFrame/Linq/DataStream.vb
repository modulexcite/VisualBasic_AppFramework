﻿Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace DocumentStream.Linq

    Public Delegate Function GetOrdinal(Column As String) As Integer

    ''' <summary>
    ''' Buffered large text dataset Table reader
    ''' </summary>
    Public Class DataStream : Inherits BufferedStream
        Implements ISchema
        Implements IDisposable

        ReadOnly _schema As Dictionary(Of String, Integer)
        ReadOnly _title As RowObject

        Public ReadOnly Property SchemaOridinal As Dictionary(Of String, Integer) Implements ISchema.SchemaOridinal
            Get
                Return _schema
            End Get
        End Property

        Sub New()
            _schema = New Dictionary(Of String, Integer)
            _title = New RowObject
        End Sub

        Sub New(file As String, Optional encoding As Encoding = Nothing, Optional bufSize As Integer = 64 * 1024 * 1024)
            Call MyBase.New(file, encoding, bufSize)

            Dim first As String = file.ReadFirstLine

            _title = RowObject.TryParse(first)
            _schema = _title.ToArray(
                Function(colName, idx) New With {
                    .colName = colName,
                    .ordinal = idx}) _
                    .ToDictionary(Function(x) x.colName.ToLower,
                                  Function(x) x.ordinal)
            Me.FileName = file

            Call $"{file.ToFileURL} handle opened...".__DEBUG_ECHO
        End Sub

        Public Function GetOrdinal(Name As String) As Integer Implements ISchema.GetOrdinal
            If _schema.ContainsKey(Name.ToLower.ShadowCopy(Name)) Then
                Return _schema(Name)
            Else
                Return -1
            End If
        End Function

        Dim __firstBlock As Boolean = True

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 这个函数主要是为了处理第一行数据
        ''' 因为在构造函数部分已经读取了第一行来解析schema，所以在这里需要对第一个数据块做一些额外的处理
        ''' </remarks>
        Public Overrides Function BufferProvider() As String()
            Dim buffer As String() = MyBase.BufferProvider()

            If __firstBlock Then
                __firstBlock = False
                buffer = buffer.Skip(1).ToArray
            Else         '  不是第一个数据块，则不需要额外处理，直接返回
            End If

            Return buffer
        End Function

        Public Sub ForEach(Of T As Class)(invoke As Action(Of T))
            Dim line As String = ""
            Dim schema As SchemaProvider = SchemaProvider.CreateObject(Of T)(False).CopyWriteDataToObject
            Dim RowBuilder As New RowBuilder(schema)

            Call RowBuilder.Indexof(Me)

            Do While True
                Dim buffer As String() = BufferProvider()
                Dim p As Integer = 0

                Do While Not buffer.Read(p, out:=line) Is Nothing
                    Dim row As RowObject = RowObject.TryParse(line)
                    Dim obj As T = Activator.CreateInstance(Of T)
                    obj = RowBuilder.FillData(Of T)(row, obj)
                    Call invoke(obj)
                Loop

                If EndRead Then
                    Exit Do
                Else
                    Call Console.WriteLine("Process next block....")
                End If
            Loop
        End Sub

        ''' <summary>
        ''' Processing large dataset in block partitions.(以分块任务的形式来处理一个非常大的数据集)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="invoke"></param>
        ''' <param name="blockSize">行数</param>
        Public Sub ForEachBlock(Of T As Class)(invoke As Action(Of T()), Optional blockSize As Integer = 10240 * 5)
            Dim schema As SchemaProvider = SchemaProvider.CreateObject(Of T)(False).CopyWriteDataToObject
            Dim RowBuilder As New RowBuilder(schema)

            Call RowBuilder.Indexof(Me)

            Do While True
                Dim chunks As String()() = BufferProvider().Split(blockSize)

                Call $"{chunks.Length} data partitions, {NameOf(blockSize)}:={blockSize}..".__DEBUG_ECHO

                Dim i As Integer = 0

                For Each block As String() In chunks
                    Dim LQuery = (From line As String In block.AsParallel Select RowObject.TryParse(line)).ToArray
                    Dim values = (From row As RowObject In LQuery.AsParallel
                                  Let obj As T = Activator.CreateInstance(Of T)
                                  Select RowBuilder.FillData(row, obj)).ToArray
                    Call "Start processing block...".__DEBUG_ECHO
                    Call Time(AddressOf New __taskHelper(Of T)(values, invoke).RunTask)
                    Call $"{100 * i / chunks.Length}% ({i}/{chunks.Length})...".__DEBUG_ECHO
                    Call i.MoveNext
                Next

                If EndRead Then
                    Exit Do
                Else
                    Call Console.WriteLine("Process next block....")
                End If
            Loop
        End Sub

        ''' <summary>
        ''' 为了减少Lambda表达式所带来的性能损失而构建的一个任务运行帮助对象
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        Private Structure __taskHelper(Of T)
            Sub New(source As T(), invoke As Action(Of T()))
                Me.__source = source
                Me.__task = invoke
            End Sub

            Dim __task As Action(Of T())
            Dim __source As T()

            ''' <summary>
            ''' 运行当前的这个任务
            ''' </summary>
            Public Sub RunTask()
                Call __task(__source)
            End Sub
        End Structure

        ''' <summary>
        ''' Csv to LINQ
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <returns></returns>
        Public Iterator Function AsLinq(Of T As Class)() As IEnumerable(Of T)
            Dim schema As SchemaProvider = SchemaProvider.CreateObject(Of T)(False).CopyWriteDataToObject
            Dim RowBuilder As New RowBuilder(schema)

            Call RowBuilder.Indexof(Me)

            Do While Not EndRead
                Dim LQuery As IEnumerable(Of T) = From line As String
                                                  In BufferProvider()
                                                  Let row As RowObject = RowObject.TryParse(line)
                                                  Let obj As T = Activator.CreateInstance(Of T)
                                                  Select RowBuilder.FillData(row, obj)
                For Each x As T In LQuery
                    Yield x
                Next
            Loop

            Call Reset()
        End Function

        Public Shared Function OpenHandle(file As String, Optional encoding As Encoding = Nothing) As DataStream
            Return New DataStream(file, encoding)
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Overloads Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    __innerBuffer = Nothing
                    __innerStream = Nothing

                    Call FlushMemory()
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
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace