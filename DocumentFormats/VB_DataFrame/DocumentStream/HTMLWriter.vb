﻿Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace DocumentStream

    <PackageNamespace("Csv.HTML.Writer")>
    Public Module HTMLWriter

        <Extension> Public Function ToHTML(Of T As Class)(source As Generic.IEnumerable(Of T), Optional Title As String = "", Optional describ As String = "", Optional css As String = "") As String
            Dim Csv As DocumentStream.File = source.ToCsvDoc(False)

            If String.IsNullOrEmpty(describ) Then
                describ = GetType(T).Description
            End If
            If String.IsNullOrEmpty(Title) Then
                Title = $"Document for {GetType(T).FullName}"
            End If

            Return Csv.ToHTML(Title, describ, css)
        End Function

        <ExportAPI("ToHTML")>
        <Extension> Public Function ToHTML(doc As DocumentStream.File, Optional Title As String = "", Optional describ As String = "", Optional css As String = "") As String
            If String.IsNullOrEmpty(css) Then
                css = My.Resources.foundation
            End If

            Dim html As StringBuilder = New StringBuilder(My.Resources.HTML_Template)
            Call html.Replace("{Title}", Title)
            Call html.Replace("{CSS}", css)

            Dim innerDoc As StringBuilder = New StringBuilder($"<p>{describ}</p>")
            Call innerDoc.AppendLine(doc.ToHTMLTable)

            Call html.Replace("{doc}", innerDoc.ToString)

            Return html.ToString
        End Function

        <Extension> Public Function ToHTMLTable(Of T As Class)(source As Generic.IEnumerable(Of T), Optional className As String = "", Optional width As String = "") As String
            Dim Csv As DocumentStream.File = source.ToCsvDoc(False)
            Return Csv.ToHTMLTable(className, width)
        End Function

        ''' <summary>
        ''' 只是生成table，而非完整的html文档
        ''' </summary>
        ''' <param name="doc"></param>
        ''' <param name="width">100%|px</param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("ToHTML.Table")>
        <Extension> Public Function ToHTMLTable(doc As DocumentStream.File, Optional className As String = "", Optional width As String = "") As String
            Dim innerDoc As New StringBuilder("<table", 4096)

            If Not String.IsNullOrEmpty(className) Then
                Call innerDoc.Append($" class=""{className}""")
            End If
            If Not String.IsNullOrEmpty(width) Then
                Call innerDoc.Append($" width=""{width}""")
            End If

            Call innerDoc.Append(">")
            Call innerDoc.AppendLine(doc.First.__titleRow)
            For Each row As RowObject In doc.Skip(1)
                Call innerDoc.AppendLine(row.__contentRow)
            Next
            Call innerDoc.AppendLine("</table>")

            Return innerDoc.ToString
        End Function

        <Extension> Private Function __titleRow(row As RowObject) As String
            Dim doc As StringBuilder = New StringBuilder

            Call doc.AppendLine("<tr>")
            Call doc.AppendLine(row.ToArray(Function(x) $"<td><strong>{x}</strong></td>").JoinBy(vbCrLf))
            Call doc.AppendLine("</tr>")

            Return doc.ToString
        End Function

        <Extension> Private Function __contentRow(row As RowObject) As String
            Dim doc As StringBuilder = New StringBuilder

            Call doc.AppendLine("<tr>")
            Call doc.AppendLine(row.ToArray(Function(x) $"<td>{x}</td>").JoinBy(vbCrLf))
            Call doc.AppendLine("</tr>")

            Return doc.ToString
        End Function
    End Module
End Namespace