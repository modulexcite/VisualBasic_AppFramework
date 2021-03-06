﻿Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language

Namespace DocumentStream.Linq

    Public Module BatchQueue

        ''' <summary>
        ''' {<see cref="IO.Path.GetFileNameWithoutExtension(String)"/>, <typeparamref name="T"/>()}
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="files"></param>
        ''' <returns></returns>
        ''' <remarks>在服务器上面可能会出现IO很慢的情况，这个时候可以试一下这个函数进行批量数据加载</remarks>
        Public Iterator Function ReadQueue(Of T As Class)(
                                 files As IEnumerable(Of String),
                                 Optional encoding As Encodings = Encodings.Default) As IEnumerable(Of NamedValue(Of T()))

            Call "Wait for the IO queue.....".__DEBUG_ECHO

            Dim sw As Stopwatch = Stopwatch.StartNew
            Dim encode As Encoding = encoding.GetEncodings
            Dim IO As IEnumerable(Of NamedValue(Of String())) = From path As String
                                                                In files.AsParallel
                                                                Let echoIni As String = $"{path.ToFileURL} init start...".__DEBUG_ECHO
                                                                Let buf As String() = path.ReadAllLines(encode)
                                                                Let echo As String = $"{path.ToFileURL} I/O read done!".__DEBUG_ECHO
                                                                Let name As String = path.BaseName
                                                                Select New NamedValue(Of String())(name, buf) ' 不清楚为什么服务器上面有时候的IO会非常慢，则在这里可以一次性的先读完所有数据，然后再返回数据

            Call $"All I/O queue job done!   {sw.ElapsedMilliseconds}ms...".__DEBUG_ECHO

            For Each data As NamedValue(Of String()) In IO
                Dim buf As T() = data.x.LoadStream(Of T)(False)

                Yield New NamedValue(Of T())(data.Name, buf)

                Call GC.SuppressFinalize(data.x)    ' 数据量非常大的话，在这里进行内存的释放
                Call GC.SuppressFinalize(data)
                Call Console.Write(".")
            Next

            Call GC.SuppressFinalize(IO)
        End Function

#Region "How this workflow works?"

        ' The data loading workflow can be explained by the example code show below:
        ' ===========================================================================

        ' Sub Main()
        '    For Each x As String In TestWorkflowBase()
        '        Console.WriteLine(x)
        '    Next
        ' End Sub

        ' Private Iterator Function TestWorkflowBase() As IEnumerable(Of String)
        '    Dim LQuery As IEnumerable(Of String) = From s As Integer
        '                                           In {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15}.AsParallel
        '                                           Let exc = MsgBox(s)
        '                                           Select CStr(s)
        '    For Each s As String In LQuery
        '        Yield s
        '    Next
        ' End Function
#End Region
    End Module
End Namespace