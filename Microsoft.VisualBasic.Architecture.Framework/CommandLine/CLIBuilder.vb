﻿Imports System.ComponentModel
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.CommandLine.Reflection.Optional
Imports Microsoft.VisualBasic.Language

Namespace CommandLine

    ''' <summary>
    ''' The class object which can interact with the target commandline program.(与目标命令行程序进行命令行交互的编程接口，本类型的对象的作用主要是生成命令行参数)
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class InteropService : Inherits CLIBuilder

        ''' <summary>
        ''' Assembly path for the target invoked program.
        ''' </summary>
        ''' <remarks></remarks>
        Protected _executableAssembly As String

        Public Overrides Function ToString() As String
            If String.IsNullOrEmpty(_executableAssembly) Then
                Return MyBase.ToString()
            Else
                Return _executableAssembly
            End If
        End Function
    End Class

    <AttributeUsage(AttributeTargets.Field, AllowMultiple:=False, Inherited:=True)>
    Public Class NullOrDefault : Inherits Attribute

        Public ReadOnly Property value As String

        Sub New(Optional _default As String = "")
            value = _default
        End Sub

        Public Overrides Function ToString() As String
            Return value
        End Function
    End Class

    Public MustInherit Class CLIBuilder

        Public Overrides Function ToString() As String
            Return Me.GetCLI
        End Function
    End Class

    Public Module CLIBuildMethod

        ReadOnly _innerTypeInfo As System.Type = GetType([Optional])

        ''' <summary>
        ''' Generates the command line string value for the invoked target cli program using this interop services object instance.
        ''' (生成命令行参数)
        ''' </summary>
        ''' <typeparam name="TInteropService">
        ''' A class type object for interaction with a commandline program.
        ''' (与命令行程序进行交互的模块对象类型)
        ''' </typeparam>
        ''' <param name="Instance">目标交互对象的实例</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 依照类型<see cref="CLITypes"/>来生成参数字符串
        ''' 
        ''' <see cref="CLITypes.Boolean"/>, True => 参数名；
        ''' <see cref="CLITypes.Double"/>, <see cref="CLITypes.Integer"/>, <see cref="CLITypes.String"/>, => 参数名 + 参数值，假若字符串为空则不添加；
        ''' （假若是枚举值类型，可能还需要再枚举值之中添加<see cref="DescriptionAttribute"/>属性）
        ''' <see cref="CLITypes.File"/>, 假若字符串为空则不添加，有空格自动添加双引号，相对路径会自动转换为全路径。
        ''' </remarks>
        <Extension>
        Public Function GetCLI(Of TInteropService As Class)(Instance As TInteropService) As String
            Dim arguments = (From [property] As System.Reflection.PropertyInfo
                             In GetType(TInteropService).GetProperties
                             Let attrs As Object() = [property].GetCustomAttributes(attributeType:=_innerTypeInfo, inherit:=True)
                             Where Not attrs.IsNullOrEmpty
                             Let attr As [Optional] = DirectCast(attrs.First, [Optional])
                             Select attr, [property]).ToArray
            Dim sBuilder As StringBuilder = New StringBuilder(1024)

            For Each argum In arguments
                Dim getCLIToken As __getCLIToken = __getMethods(argum.attr.Type)
                Dim value As Object = argum.property.GetValue(Instance, Nothing)
                Dim cliToken As String = getCLIToken(value, argum.attr, argum.property)

                If Not String.IsNullOrEmpty(cliToken) Then
                    Call sBuilder.Append(cliToken & " ")
                End If
            Next

            Return sBuilder.ToString.TrimEnd
        End Function

        Public Function SimpleBuilder(name As String, args As IEnumerable(Of KeyValuePair(Of String, String))) As String
            Dim sbr As StringBuilder = New StringBuilder(name)

            For Each x In args
                If String.IsNullOrEmpty(x.Value) Then
                    Continue For
                End If

                Call sbr.Append(" ")
                Call sbr.Append(x.Key & " ")
                Call sbr.Append(x.Value.CliToken)
            Next

            Return sbr.ToString
        End Function

#Region ""

        Private ReadOnly __getMethods As IReadOnlyDictionary(Of CLITypes, __getCLIToken) =
            New Dictionary(Of CLITypes, __getCLIToken) From {
 _
            {CLITypes.Boolean, AddressOf CLIBuildMethod.__booleanRule},
            {CLITypes.Double, AddressOf CLIBuildMethod.__stringRule},
            {CLITypes.File, AddressOf CLIBuildMethod.__pathRule},
            {CLITypes.Integer, AddressOf CLIBuildMethod.__stringRule},
            {CLITypes.String, AddressOf CLIBuildMethod.__stringRule}
        }

        Private Delegate Function __getCLIToken(value As Object, attr As [Optional], prop As PropertyInfo) As String

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="value">只能是<see cref="System.String"/>类型的</param>
        ''' <param name="attr"></param>
        ''' <param name="prop"></param>
        ''' <returns></returns>
        Private Function __pathRule(value As Object, attr As [Optional], prop As PropertyInfo) As String
            Dim path As String = DirectCast(value, String)
            If Not String.IsNullOrEmpty(path) Then
                path = $"{attr.Name} {path.CliPath}"
            End If
            Return path
        End Function

        ''' <summary>
        ''' 可能包含有枚举值
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="attr"></param>
        ''' <param name="prop"></param>
        ''' <returns></returns>
        Private Function __stringRule(value As Object, attr As [Optional], prop As PropertyInfo) As String
            If prop.PropertyType.Equals(GetType(String)) Then
                Dim str As String = Scripting.ToString(value)
                If String.IsNullOrEmpty(str) Then
                    Return ""
                Else
                    Return $"{attr.Name} {str.CliToken}"
                End If
            ElseIf prop.PropertyType.IsInheritsFrom(GetType([Enum])) Then
                Return __stringEnumRule(value, attr, prop)
            Else
                Dim str As String = Scripting.ToString(value)
                Return $"{attr.Name} {str}"
            End If
        End Function

        Private Function __stringEnumRule(value As Object, attr As [Optional], prop As PropertyInfo) As String
            Dim enumValue As [Enum] = DirectCast(value, System.Enum)
            Dim type As Type = prop.PropertyType
            Dim enumFields As FieldInfo() = type.GetFields
            Dim nullGet = (From x As FieldInfo In enumFields
                           Let flag As NullOrDefault = x.GetAttribute(Of NullOrDefault)
                           Where Not flag Is Nothing
                           Select flag, x).FirstOrDefault
            If nullGet Is Nothing Then  '没有默认值
rtvl:           Dim strValue As String = enumValue.Description
                Return $"{attr.Name} {strValue.CliToken}"
            Else
                If nullGet.x.GetValue(Nothing).Equals(value) Then
                    Dim str As String = nullGet.flag.value      ' 是默认值，则返回默认值
                    If String.IsNullOrEmpty(str) Then
                        Return ""
                    Else
                        Return $"{attr.Name} {str}"
                    End If
                Else
                    GoTo rtvl
                End If
            End If
        End Function

        Private Function __booleanRule(value As Object, attr As [Optional], prop As PropertyInfo) As String
            Dim name As String = attr.Name
            Dim b As Boolean

            If prop.PropertyType.Equals(GetType(Boolean)) Then
                b = DirectCast(value, Boolean)
            Else
                Dim str As String = Scripting.ToString(value)
                b = str.getBoolean
            End If

            If b Then
                Return name
            Else
                Return ""
            End If
        End Function
#End Region

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <typeparam name="TInteropService"></typeparam>
        ''' <param name="inst"></param>
        ''' <returns>返回所重置的参数的个数</returns>
        ''' <remarks></remarks>
        <Extension>
        Public Function ClearParameters(Of TInteropService As Class)(inst As TInteropService) As Integer
            Dim n As Integer
            Dim lstProperty As PropertyInfo() = inst.GetType().GetProperties()

            Try
                For Each [Property] As PropertyInfo In lstProperty
                    Dim attrs As Object() = [Property].GetCustomAttributes(_innerTypeInfo, inherit:=False)
                    If Not (attrs Is Nothing OrElse attrs.Length = 0) Then
                        Call [Property].SetValue(inst, "", Nothing)
                        n += 1
                    End If
                Next
            Catch ex As Exception
                Throw New InvalidOperationException(InvalidOperation)
            End Try

            Return n
        End Function

        Const InvalidOperation As String = "The target type information is not the 'System.String'!"
    End Module
End Namespace