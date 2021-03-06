' Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
'    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. 

Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks

Namespace SoftwareToolkits.XmlDoc.Assembly

    ''' <summary>
    ''' A namespace within a project -- typically a collection of related types.  Equates to a .net Namespace.
    ''' </summary>
    Public Class ProjectNamespace
        Private project As Project

        Private m_types As Dictionary(Of [String], ProjectType)

        Public Property Path() As [String]

        Public ReadOnly Property Types() As IEnumerable(Of ProjectType)
            Get
                Return Me.m_types.Values
            End Get
        End Property
        Public Sub New(project As Project)
            Me.project = project
            Me.m_types = New Dictionary(Of String, ProjectType)()
        End Sub

        Public Overloads Function [GetType](typeName As [String]) As ProjectType
            If Me.m_types.ContainsKey(typeName.ToLower()) Then
                Return Me.m_types(typeName.ToLower())
            End If

            Return Nothing
        End Function

        Public Function EnsureType(typeName As [String]) As ProjectType
            Dim pt As ProjectType = Me.[GetType](typeName)

            If pt Is Nothing Then
                pt = New ProjectType(Me)
                pt.Name = typeName

                Me.m_types.Add(typeName.ToLower(), pt)
            End If

            Return pt
        End Function

        Public Sub ExportMarkdownFile(folderPath As [String], pageTemplate As [String])
            Dim typeList As New StringBuilder()

            Dim projectTypes As SortedList(Of [String], ProjectType) = New SortedList(Of String, ProjectType)()

            For Each pt As ProjectType In Me.Types
                projectTypes.Add(pt.Name, pt)
            Next

            For Each pt As ProjectType In projectTypes.Values
                typeList.AppendLine("[" & pt.Name & "](T-" & Me.Path & "." & pt.Name & ".md)")
            Next

            Dim text As [String] = [String].Format(vbCr & vbLf & "# {0}" & vbCr & vbLf & vbCr & vbLf & "{1}" & vbCr & vbLf, Me.Path, typeList.ToString())

            If pageTemplate IsNot Nothing Then
                text = pageTemplate.Replace("[content]", text)
            End If

            Call text.SaveTo(folderPath & "/N-" & Me.Path & ".md", Encoding.UTF8)
        End Sub
    End Class
End Namespace