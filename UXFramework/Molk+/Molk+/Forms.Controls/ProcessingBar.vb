﻿Imports Microsoft.VisualBasic.Imaging

Public Class ProcessingBar

    ''' <summary>
    ''' 0 - 100
    ''' </summary>
    ''' <returns></returns>
    Public Property PercentageValue As Integer
        Get
            Return Percentage
        End Get
        Set(value As Integer)

            If Percentage <> value Then
                Percentage = value
                Call Timer1_Tick(Nothing, Nothing)
            End If



        End Set
    End Property

    Dim Percentage As Integer

    Dim accomplish As Image
    Dim _render As Image

    Public Property Render As Image
        Get
            Return _render
        End Get
        Set(value As Image)
            Me._render = value
            Me.Height = Render.Height

            Dim gr = New Size(8000, height:=value.Height).CreateGDIDevice()

            Dim x As Integer = 1

            Do While x < gr.Width
                Call gr.Graphics.DrawImage(value, x, 0, value.Width, value.Height)
                x += value.Width
            Loop

            Me.accomplish = gr.ImageResource

            '   Call Me.accomplish.Save("./tes.png")

        End Set
    End Property

    Private Sub ProcessingBar_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Render = My.Resources.ProcessingbarRes
    End Sub

    Private Sub ProcessingBar_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        Render = _render
    End Sub

    Public Sub StartRollAnimation()
        Me.pbAnimatedTmr.Interval = 10
        Me.pbAnimatedTmr.Enabled = True
        Me.pbAnimatedTmr.Start()
    End Sub

    Public Sub StopRollAnimation()
        Me.pbAnimatedTmr.Enabled = False
        Me.pbAnimatedTmr.Stop()
    End Sub

    Dim _Animated As Integer

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles pbAnimatedTmr.Tick

        Dim Gr = Me.Size.CreateGDIDevice(Me.BackColor)

        '绘制已经完成的部分
        Dim Length As Integer = (Me.PercentageValue / 100) * Width + 1

        _Animated += 1

        If _Animated + Length >= Me.accomplish.Width Then
            _Animated = 1
        End If

        Dim res = ImageCorping.Corping(Me.accomplish, New Rectangle(New Point(_Animated + 1, 0), New Size(Length, Me.Height)))
        Call Gr.Graphics.DrawImage(res, 0, 0, res.Width, res.Height)

        Me.BackgroundImage = Gr.ImageResource
    End Sub
End Class
