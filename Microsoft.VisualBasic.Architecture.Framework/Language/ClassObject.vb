﻿Imports System.Runtime.InteropServices
Imports System.Web.Script.Serialization
Imports System.Xml.Serialization

Namespace Language

    ''' <summary>
    ''' The base class object in VisualBasic
    ''' </summary>
    Public Class ClassObject

        ''' <summary>
        ''' The extension property.(为了节省内存的需要，这个附加属性尽量不要被自动初始化)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' Dummy field for solve the problem of xml serialization >>>simpleContent&lt;&lt;&lt;   
        ''' 
        ''' http://stackoverflow.com/questions/2501466/xmltext-attribute-in-base-class-breakes-serialization
        ''' 
        ''' So I think you could make it work by adding a dummy property or field that you never use in the LookupItem class. 
        ''' If you're never assign a value to it, it will remain null and will not be serialized, but it will prevent your 
        ''' class from being treated as simpleContent. I know it's a dirty workaround, but I see no other easy way...
        ''' </remarks>
        <XmlIgnore> <ScriptIgnore> Public Overridable Property Extension As ExtendedProps

        ''' <summary>
        ''' Default is display the json value of the object class
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function ToString() As String
            Return Serialization.GetJson(Me, [GetType])
        End Function

        ''' <summary>
        ''' String source for operator <see cref="ClassObject.Operator &(ClassObject, String)"/>
        ''' </summary>
        ''' <returns>Default is using <see cref="ToString"/> method as provider</returns>
        Protected Friend Overridable Function __toString() As String
            Return ToString()
        End Function

        ''' <summary>
        ''' Contact this class object with other string value
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="s"></param>
        ''' <returns></returns>
        Public Shared Operator &(x As ClassObject, s As String) As String
            Return x.__toString & s
        End Operator

        ''' <summary>
        ''' Contact this class object with other string value
        ''' </summary>
        ''' <param name="s"></param>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Shared Operator &(s As String, x As ClassObject) As String
            Return s & x.__toString
        End Operator

        Protected Sub Copy(ByRef x As ClassObject)
            x = Me
        End Sub
    End Class
End Namespace