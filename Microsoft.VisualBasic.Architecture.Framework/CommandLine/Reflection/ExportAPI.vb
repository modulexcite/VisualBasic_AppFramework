﻿Imports System.Text
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace CommandLine.Reflection

    ''' <summary>
    ''' A command object that with a specific name.(一个具有特定名称命令执行对象)
    ''' </summary>
    ''' <remarks></remarks>
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
    Public Class ExportAPIAttribute : Inherits Attribute
        Implements IExportAPI

        ''' <summary>
        ''' The name of the commandline object.(这个命令的名称)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Name As String Implements IExportAPI.Name
        ''' <summary>
        ''' Something detail of help information.(详细的帮助信息)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Info As String Implements IExportAPI.Info
        ''' <summary>
        ''' The usage of this command.(这个命令的用法，本属性仅仅是一个助记符，当用户没有编写任何的使用方法信息的时候才会使用本属性的值)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Usage As String Implements IExportAPI.Usage
        ''' <summary>
        ''' A example that to useing this command.(对这个命令的使用示例，本属性仅仅是一个助记符，当用户没有编写任何示例信息的时候才会使用本属性的值)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Example As String Implements IExportAPI.Example

        ''' <summary>
        ''' You are going to define a available export api for you application to another language or scripting program environment.
        ''' (定义一个命令行程序之中可以使用的命令)
        ''' </summary>
        ''' <param name="Name">The name of the commandline object or you define the exported API name here.(这个命令的名称)</param>
        ''' <remarks></remarks>
        Sub New(<Parameter("Command.Name", "The name of the commandline object.")> Name As String)
            _Name = Name
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function

        Public Function PrintView(HTML As Boolean) As String
            If HTML Then
                Return __printViewHTML()
            Else
                Return __printView()
            End If
        End Function

        Private Function __printView()
            Dim sbr As StringBuilder = New StringBuilder(1024)
            Call sbr.AppendLine($"{NameOf(Name)}    = ""{Name}""")
            Call sbr.AppendLine($"{NameOf(Info)}    = ""{Info}""")
            Call sbr.AppendLine($"{NameOf(Usage)}   = ""{Usage}""")
            Call sbr.AppendLine($"{NameOf(Example)} = ""{Example}""")

            Return sbr.ToString
        End Function

        Private Function __printViewHTML() As String
            Return ExportAPIAttribute.GenerateHtmlDoc(Me, "", "")
        End Function

        Public Shared Function GenerateHtmlDoc(Command As IExportAPI, addNode As String, addValue As String) As String
            Dim add As String = If(Not String.IsNullOrEmpty(addValue), $"           <tr>
    <td>{addNode}</td>
    <td>{addValue}</td>
  </tr>", "")

            Return $"<p>Help for ""{Command.Name}"":</p>
<table frame=""hsides"">
  <tr>
    <th>DocNode</th>
    <th>Content Text</th>
                 <th><a href=""#""><strong><font size=3>[&#8593;]</font></strong></a></th>
  </tr>
  <tr>
    <td><strong>{NameOf(Name)}</strong></td>
    <td><strong><a name=""{Command.Name}"">{Command.Name}</a></strong></td>
  </tr>
                <tr>
    <td>{NameOf(Info)}</td>
    <td>{Command.Info}</td>
  </tr>
                <tr>
    <td>{NameOf(Usage)}</td>
    <td>{Command.Usage}</td>
  </tr>
                <tr>
    <td>{NameOf(Example)}</td>
    <td>{Command.Example}</td>
  </tr>
    {add}
</table>"
        End Function

        Public Shared ReadOnly Property Type As Type = GetType(ExportAPIAttribute)
    End Class

    Public Interface IExportAPI

        ''' <summary>
        ''' The name of the commandline object.(这个命令的名称)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Name As String
        ''' <summary>
        ''' Something detail of help information.(详细的帮助信息)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Info As String
        ''' <summary>
        ''' The usage of this command.(这个命令的用法，本属性仅仅是一个助记符，当用户没有编写任何的使用方法信息的时候才会使用本属性的值)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Usage As String
        ''' <summary>
        ''' A example that to useing this command.(对这个命令的使用示例，本属性仅仅是一个助记符，当用户没有编写任何示例信息的时候才会使用本属性的值)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Example As String
    End Interface
End Namespace