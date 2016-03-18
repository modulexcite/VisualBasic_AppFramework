﻿Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Marshal
Imports Microsoft.VisualBasic.Mathematical.Types
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Mathematical.Helpers.Arithmetic
Imports Microsoft.VisualBasic.Mathematical.Expression
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' Parser for complex expressions
''' </summary>
Public Module ExpressionParser

    ''' <summary>
    ''' Using defaul engine
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns></returns>
    Public Function TryParse(s As String) As SimpleExpression
        Dim tokens = TokenIcer.TryParse(s.ClearOverlapOperator) 'Get all of the number that appears in this expression including factoral operator.

        If tokens.Count = 1 Then
            Dim token As Token(Of Tokens) = tokens.First

            If token.Type = Mathematical.Tokens.Number Then
                Return New SimpleExpression(Val(token.Text))
            Else  ' Syntax error
                Throw New SyntaxErrorException(s)
            End If
        Else
            Return New Pointer(Of Token(Of Tokens))(tokens).TryParse
        End If
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="var">Constant or variable name.</param>
    ''' <returns></returns>
    Public Delegate Function GetValue(var As String) As Double

    <Extension> Public Function TryParse(tokens As Pointer(Of Token(Of Tokens))) As SimpleExpression
        Return tokens.TryParse(AddressOf DefaultEngine.GetValue, AddressOf DefaultEngine.Functions.Evaluate, False)
    End Function

    ''' <summary>
    ''' 这个解析器还需要考虑Stack的问题
    ''' </summary>
    ''' <param name="tokens"></param>
    ''' <returns></returns>
    <Extension> Public Function TryParse(tokens As Pointer(Of Token(Of Tokens)), getValue As GetValue, evaluate As IFuncEvaluate, funcStack As Boolean) As SimpleExpression
        Dim sep As New SimpleExpression
        Dim e As Token(Of Tokens)
        Dim meta As MetaExpression = Nothing
        Dim o As Char
        Dim pre As Token(Of Tokens) = Nothing
        Dim func As Func = Nothing

        Do While Not tokens.EndRead
            e = +tokens

            Select Case e.Type
                Case Mathematical.Tokens.OpenBracket, Mathematical.Tokens.OpenStack
                    If pre Is Nothing Then  ' 前面不是一个未定义的标识符，则在这里是一个括号表达式
                        meta = New MetaExpression(TryParse(tokens, getValue, evaluate, False))
                    Else
                        func = New Func(pre.Text, evaluate)  ' Get function name, and then removes the last of the expression
                        o = sep.RemoveLast().Operator

                        Do While True
                            Dim exp = TryParse(tokens, getValue, evaluate, True)
                            If exp.IsNullOrEmpty Then
                                Exit Do
                            Else
                                func.Params.Add(exp)
                            End If
                        Loop

                        meta = New MetaExpression(AddressOf func.Evaluate)
                        o = If(tokens.EndRead, "+"c, (+tokens).Text.First)
                        meta.Operator = o
                        pre = Nothing
                        Call sep.Add(meta)

                        Continue Do
                    End If
                Case Mathematical.Tokens.CloseStack, Mathematical.Tokens.CloseBracket, Mathematical.Tokens.Delimiter
                    Return sep ' 退出递归栈
                Case Mathematical.Tokens.Number
                    meta = New MetaExpression(Val(e.Text))
                Case Mathematical.Tokens.UNDEFINE
                    Dim x As String = e.Text
                    meta = New MetaExpression(Function() getValue(x))
                    pre = e ' probably is a function name
            End Select

            If tokens.EndRead Then
                meta.Operator = "+"c
                Call sep.Add(meta)
            Else
                o = (+tokens).Text.First  ' tokens++ 移动指针到下一个元素

                If o = "!"c Then
                    Dim stackMeta = New MetaExpression(Function() Factorial(meta.LEFT, 0))

                    If tokens.EndRead Then
                        Call sep.Add(stackMeta)
                        Exit Do
                    Else
                        o = (+tokens).Text.First
                        If o = ")"c Then
                            e = -tokens
                            stackMeta.Operator = "+"c
                            Call sep.Add(stackMeta)
                            e = (-tokens)
                            Return sep
                        ElseIf o = ","c Then
                            meta.Operator = "+"c
                            Call sep.Add(meta)
                            ' e = (-tokens)
                            Exit Do ' 退出递归栈
                        Else
                            stackMeta.Operator = o
                            Call sep.Add(stackMeta)
                            Continue Do
                        End If
                    End If
                ElseIf o = ","c Then
                    meta.Operator = "+"c
                    Call sep.Add(meta)
                    ' e = (-tokens)
                    Exit Do ' 退出递归栈
                ElseIf IsCloseStack(o) Then
                    meta.Operator = "+"c
                    Call sep.Add(meta)
                    If funcStack Then
                        e = (-tokens)
                    End If
                    Exit Do ' 退出递归栈
                ElseIf IsOpenStack(o) Then
                    e = -tokens  ' 指针回退一步
                End If

                meta.Operator = o
                Call sep.Add(meta)
            End If
        Loop

        Return sep
    End Function
End Module