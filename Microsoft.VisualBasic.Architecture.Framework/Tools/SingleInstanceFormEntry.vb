﻿Imports System.Windows.Forms
Imports System.Drawing

Public Class SingleInstanceFormEntry(Of TForm As System.Windows.Forms.Form)

    Dim _clickEntry As Control
    Dim __getPosition As Func(Of Control, System.Windows.Forms.Form, Point)
    Dim _parentForm As System.Windows.Forms.Form
    Dim _ShowModel As Boolean

    Public ReadOnly Property Form As TForm

    Public Property Arguments As Object()

    Sub New(ControlEntry As Control,
           Optional ParentForm As System.Windows.Forms.Form = Nothing,
           Optional GetPosition As Func(Of Control, System.Windows.Forms.Form, Point) = Nothing,
           Optional ShowModelForm As Boolean = True)

        _clickEntry = ControlEntry
        __getPosition = GetPosition
        _parentForm = ParentForm
        _ShowModel = ShowModelForm

        AddHandler _clickEntry.Click, AddressOf __invokeEntry

        If GetPosition Is Nothing AndAlso Not _parentForm Is Nothing Then
            __getPosition = AddressOf __getDefaultPos
        End If
    End Sub

    ''' <summary>
    ''' 不做任何位置的设置操作
    ''' </summary>
    ''' <remarks></remarks>
    Sub New(Optional ShowModelForm As Boolean = True)
        _ShowModel = ShowModelForm
    End Sub

    Public Sub [AddHandler](Handle As Action(Of Object, EventArgs))
        AddHandler _clickEntry.Click, New EventHandler(Sub(obj, args) Call Handle(obj, args))
    End Sub

    ''' <summary>
    ''' 默认位置是控件的中间
    ''' </summary>
    ''' <param name="Control"></param>
    ''' <param name="Form"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Shared Function __getDefaultPos(Control As UserControl, Form As Form) As Point
        Dim Pt As Point = Form.PointToScreen(Control.Location)
        Pt = New Point(CInt(Pt.X + Control.Width / 2), CInt(Pt.Y + Control.Height / 2))
        Return Pt
    End Function

    Public Sub Invoke(ParamArray InvokeSets As KeyValuePair(Of String, Object)())
        __invokeSets = InvokeSets
        Call __invokeEntry(Nothing, Nothing)
    End Sub

    Dim __invokeSets As KeyValuePair(Of String, Object)()

    Private Sub __invokeEntry(sender As Object, EVtargs As EventArgs)
        If Not Form Is Nothing Then Return

        _Form = DirectCast(Activator.CreateInstance(GetType(TForm), Arguments), TForm)

        If Not __getPosition Is Nothing Then
            Dim pt As Point = __getPosition(_clickEntry, Form)
            Form.Location = pt
        End If

        If Not __invokeSets.IsNullOrEmpty Then
            For Each Entry In __invokeSets
                Call Form.InvokeSet(Of Object)(Entry.Key, Entry.Value)
            Next
        End If

        If _ShowModel Then
            Call Form.ShowDialog()
            Call Form.Free()
        Else
            Call Form.Show()
            AddHandler Form.FormClosed, Sub() Call Form.Free()
        End If
    End Sub
End Class
