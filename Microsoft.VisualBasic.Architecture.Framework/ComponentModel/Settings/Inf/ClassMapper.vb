﻿Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.Settings.Inf

    ''' <summary>
    ''' 定义在Ini配置文件之中的Section的名称
    ''' </summary>
    <AttributeUsage(AttributeTargets.Class, AllowMultiple:=False, Inherited:=True)>
    Public Class ClassName : Inherits Attribute

        Public ReadOnly Property Name As String

        ''' <summary>
        ''' Defines the section name in the ini profile data.(定义在Ini配置文件之中的Section的名称)
        ''' </summary>
        ''' <param name="name"></param>
        Sub New(name As String)
            Me.Name = name
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function
    End Class

    ''' <summary>
    ''' 使用这个属性来标记需要进行序列化的对象属性: <see cref="DataFrameColumnAttribute"/>
    ''' </summary>
    Public Module ClassMapper

        Public Function MapParser(Of T As Class)() As NamedValue(Of BindProperty(Of DataFrameColumnAttribute)())
            Return GetType(T).MapParser
        End Function

        ''' <summary>
        ''' Get mapping data, includes section name and keys
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        <Extension>
        Public Function MapParser(type As Type) As NamedValue(Of BindProperty(Of DataFrameColumnAttribute)())
            Dim nameCLS As ClassName = type.GetAttribute(Of ClassName)
            Dim name As String

            If nameCLS Is Nothing Then
                name = type.Name
            Else
                name = nameCLS.Name
            End If

            Dim source = DataFrameColumnAttribute.LoadMapping(type)
            Dim binds As BindProperty(Of DataFrameColumnAttribute)() =
                source.Values.ToArray

            Return New NamedValue(Of BindProperty(Of DataFrameColumnAttribute)()) With {
                .Name = name,
                .x = binds
            }
        End Function

        ''' <summary>
        ''' Read data from ini file.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="ini"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function ClassWriter(Of T As Class)(ini As IniFile) As T
            Dim obj As Object = ClassWriter(ini, GetType(T))
            Return DirectCast(obj, T)
        End Function

        Public Function ClassWriter(ini As IniFile, type As Type) As Object
            Dim maps As NamedValue(Of BindProperty(Of DataFrameColumnAttribute)()) =
                MapParser(type)
            Dim obj As Object = Activator.CreateInstance(type)

            For Each map In maps.x
                Dim key As String = map.Column.Name
                Dim value As String = ini.ReadValue(maps.Name, key)
                Dim o As Object = Scripting.CTypeDynamic(value, map.Type)
                Call map.SetValue(obj, o)
            Next

            Return obj
        End Function

        <Extension>
        Public Sub ClassDumper(Of T As Class)(x As T, ini As IniFile)
            Call ClassDumper(x, GetType(T), ini)
        End Sub

        Public Sub ClassDumper(x As Object, type As Type, ini As IniFile)
            Dim maps As NamedValue(Of BindProperty(Of DataFrameColumnAttribute)()) =
                MapParser(type)

            For Each map In maps.x
                Dim key As String = map.Column.Name
                Dim value As String = Scripting.ToString(map.GetValue(x))
                Call ini.WriteValue(maps.Name, key, value)
            Next
        End Sub

        ''' <summary>
        ''' Load a ini section profile data from a ini file.
        ''' </summary>
        ''' <typeparam name="T">The section mapper</typeparam>
        ''' <param name="path">*.ini file</param>
        ''' <returns></returns>
        <Extension>
        Public Function LoadIni(Of T As Class)(path As String) As T
            Return New IniFile(path).ClassWriter(Of T)
        End Function

        ''' <summary>
        ''' Write ini section into data file.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="x">A section class in the ini profile file.</param>
        ''' <param name="ini"></param>
        ''' <returns></returns>
        <Extension>
        Public Function WriteClass(Of T As Class)(x As T, ini As String) As Boolean
            Try
                Call x.ClassDumper(New IniFile(ini))
            Catch ex As Exception
                ex = New Exception(ini, ex)
                ex = New Exception(GetType(T).FullName, ex)
                Call ex.PrintException
                Call App.LogException(ex)
                Return False
            End Try

            Return True
        End Function
    End Module
End Namespace