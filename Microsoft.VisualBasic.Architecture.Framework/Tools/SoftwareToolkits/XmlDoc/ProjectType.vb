' Copyright (c) Bendyline LLC. All rights reserved. Licensed under the Apache License, Version 2.0.
'    You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0. 


Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.IO
Imports System.Xml

Namespace SoftwareToolkits.XmlDoc.Assembly

    ''' <summary>
    ''' A type within a project namespace.
    ''' </summary>
    Public Class ProjectType
        Private projectNamespace As ProjectNamespace
        Private fields As Dictionary(Of [String], ProjectMember)
        Private properties As Dictionary(Of [String], ProjectMember)
        Private methods As Dictionary(Of [String], ProjectMember)

        Public ReadOnly Property [Namespace]() As ProjectNamespace
            Get
                Return Me.projectNamespace
            End Get
        End Property

        Public Property Name() As [String]
        Public Property Summary() As [String]

        Public Sub New(projectNamespace As ProjectNamespace)
            Me.projectNamespace = projectNamespace

            Me.fields = New Dictionary(Of String, ProjectMember)()
            Me.properties = New Dictionary(Of String, ProjectMember)()
            Me.methods = New Dictionary(Of String, ProjectMember)()
        End Sub

        Public Function GetMethod(methodName As [String]) As ProjectMember
            If Me.methods.ContainsKey(methodName.ToLower()) Then
                Return Me.methods(methodName.ToLower())
            End If

            Return Nothing
        End Function

        Public Function EnsureMethod(methodName As [String]) As ProjectMember
            Dim pm As ProjectMember = Me.GetMethod(methodName)

            If pm Is Nothing Then
                pm = New ProjectMember(Me)
                pm.Name = methodName

                Me.methods.Add(methodName.ToLower(), pm)
            End If

            Return pm
        End Function

        Public Function GetProperty(propertyName As [String]) As ProjectMember
            If Me.properties.ContainsKey(propertyName.ToLower()) Then
                Return Me.properties(propertyName.ToLower())
            End If

            Return Nothing
        End Function

        Public Function EnsureProperty(propertyName As [String]) As ProjectMember
            Dim pm As ProjectMember = Me.GetProperty(propertyName)

            If pm Is Nothing Then
                pm = New ProjectMember(Me)
                pm.Name = propertyName

                Me.properties.Add(propertyName.ToLower(), pm)
            End If

            Return pm
        End Function

        Public Function GetField(fieldName As [String]) As ProjectMember
            If Me.fields.ContainsKey(fieldName.ToLower()) Then
                Return Me.fields(fieldName.ToLower())
            End If

            Return Nothing
        End Function

        Public Function EnsureField(fieldName As [String]) As ProjectMember
            Dim pm As ProjectMember = Me.GetField(fieldName)

            If pm Is Nothing Then
                pm = New ProjectMember(Me)
                pm.Name = fieldName

                Me.fields.Add(fieldName.ToLower(), pm)
            End If

            Return pm
        End Function

        Public Sub ExportMarkdownFile(folderPath As [String], pageTemplate As [String])
            Dim methodList As New StringBuilder()

            If Me.methods.Values.Count > 0 Then
                methodList.AppendLine("### Methods" & vbCr & vbLf)

                Dim sortedMembers As SortedList(Of [String], ProjectMember) = New SortedList(Of String, ProjectMember)()

                For Each pm As ProjectMember In Me.methods.Values
                    sortedMembers.Add(pm.Name, pm)
                Next

                For Each pm As ProjectMember In sortedMembers.Values
                    methodList.AppendLine("#### " & pm.Name)
                    methodList.AppendLine(CleanText(pm.Summary))

                    If pm.Returns IsNot Nothing Then
                        methodList.AppendLine("_returns: " & pm.Returns & "_")
                    End If
                Next
            End If

            Dim propertyList As New StringBuilder()

            If Me.properties.Count > 0 Then
                propertyList.AppendLine("### Properties" & vbCr & vbLf)

                Dim sortedMembers As SortedList(Of [String], ProjectMember) = New SortedList(Of String, ProjectMember)()

                For Each pm As ProjectMember In Me.properties.Values
                    sortedMembers.Add(pm.Name, pm)
                Next

                For Each pm As ProjectMember In sortedMembers.Values
                    propertyList.AppendLine("#### " & pm.Name)
                    propertyList.AppendLine(CleanText(pm.Summary))
                Next
            End If

            Dim text As [String] = [String].Format(vbCr & vbLf & "# {0}" & vbCr & vbLf & "_namespace: [{1}](N-{1}.md)_" & vbCr & vbLf & vbCr & vbLf & "{2}" & vbCr & vbLf & vbCr & vbLf & "{3}" & vbCr & vbLf & vbCr & vbLf & "{4}" & vbCr & vbLf, Me.Name, Me.[Namespace].Path, CleanText(Me._Summary), methodList.ToString(), propertyList.ToString())

            If pageTemplate IsNot Nothing Then
                text = pageTemplate.Replace("[content]", text)
            End If

            Call text.SaveTo(folderPath & "/T-" & Me.[Namespace].Path & "." & Me.Name & ".md", Encoding.UTF8)
        End Sub

        Public Sub LoadFromNode(xn As XmlNode)
            Dim summaryNode As XmlNode = xn.SelectSingleNode("summary")

            If summaryNode IsNot Nothing Then
                Me._Summary = summaryNode.InnerText
            End If
        End Sub


        Private Function CleanText(incomingText As [String]) As [String]
            If incomingText Is Nothing Then
                Return [String].Empty
            End If

            incomingText = incomingText.Replace(vbTab, "").Trim()

            Dim results As String = [String].Empty
            Dim lastCharWasSpace As Boolean = False
            For Each c As Char In incomingText
                If c <> " "c Then
                    lastCharWasSpace = False
                    results += c
                ElseIf Not lastCharWasSpace Then
                    lastCharWasSpace = True
                    results += c
                End If
            Next

            Return results
        End Function
    End Class
End Namespace