﻿Imports System.Text

Namespace Helpers

    ''' <summary>
    ''' Mathematics function calculation engine
    ''' (数学函数计算引擎) 
    ''' </summary>
    ''' <remarks></remarks>
    Public Class [Function] : Inherits MemoryCollection(Of Func(Of Double(), Double))

        ''' <summary>
        ''' The mathematics calculation delegates collection with its specific name.
        ''' (具有特定名称的数学计算委托方法的集合) 
        ''' </summary>
        ''' <remarks></remarks>
        Protected Shared ReadOnly SystemPrefixFunctions As Dictionary(Of String, Func(Of Double(), Double)) =
            New Dictionary(Of String, Func(Of Double(), Double)) From {
 _
                {"abs", Function(args) Math.Abs(args(Scan0))},
                {"acos", Function(args) Math.Acos(args(Scan0))},
                {"asin", Function(args) Math.Asin(args(Scan0))},
                {"atan", Function(args) Math.Atan(args(Scan0))},
                {"atan2", Function(args) Math.Atan2(args(Scan0), args(1))},
                {"bigmul", Function(args) Math.BigMul(args(Scan0), args(1))},
                {"ceiling", Function(args) Math.Ceiling(args(Scan0))},
                {"cos", Function(args) Math.Cos(args(Scan0))},
                {"cosh", Function(args) Math.Cosh(args(Scan0))},
                {"exp", Function(args) Math.Exp(args(Scan0))},
                {"floor", Function(args) Math.Floor(args(Scan0))},
                {"ieeeremainder", Function(args) Math.IEEERemainder(args(Scan0), args(1))},
                {"log", Function(args) Math.Log(args(Scan0))},
                {"log10", Function(args) Math.Log10(args(Scan0))},
                {"max", Function(args) Math.Max(args(Scan0), args(1))},
                {"min", Function(args) Math.Min(args(Scan0), args(1))},
                {"pow", Function(args) Math.Pow(args(Scan0), args(1))},
                {"round", Function(args) Math.Round(args(Scan0))},
                {"sign", Function(args) Math.Sign(args(Scan0))},
                {"sin", Function(args) Math.Sin(args(Scan0))},
                {"sinh", Function(args) Math.Sinh(args(Scan0))},
                {"sqrt", Function(args) Math.Sqrt(args(Scan0))},
                {"tan", Function(args) Math.Tan(args(Scan0))},
                {"tanh", Function(args) Math.Tanh(args(Scan0))},
                {"truncate", Function(args) Math.Truncate(args(Scan0))},
                {"rnd", Function(args) RND(args(Scan0), args(1))},
                {"int", Function(args) CType(args(Scan0), Integer)},
                {String.Empty, Function(args) args(Scan0)} ' If no function name, then return the paramenter a directly. 
        }

        REM (经过以函数名长度降序排序的在VisualBasic中可用的函数名称的字符串集合) 
        Public ReadOnly Property FunctionList As String()
            Get
                Return MyBase.Objects
            End Get
        End Property

        Sub New(engine As Expression)
            Call MyBase.New(engine)

            For Each item In SystemPrefixFunctions
                Call MyBase.Add(item.Key, item.Value, False, False)
            Next
            Call __buildCache()   ' A string list of available function name in visualbasic, it was sort by the length of the each function name.
        End Sub

        ''' <summary>
        ''' 大小写不敏感
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="args"></param>
        ''' <returns></returns>
        Public Function Evaluate(name As String, args As Double()) As Double
            Return _objHash(name.ToLower)(args)
        End Function

        Public Overloads Sub Add(name As String, handle As Func(Of Double(), Double))
            Call MyBase.Add(name, handle, True, False)
        End Sub

        ''' <summary>
        ''' Parsing the use function definition from the user input value on the console 
        ''' and then add it to the function dictionary.
        ''' (从终端上面输入的用户函数的申明语句中解析出表达式，然后将其加入到用户字典中)
        ''' </summary>
        ''' <param name="statement">[function name](args) expression</param>
        ''' <remarks>function [function name] expression</remarks>
        Public Overloads Sub Add(statement As String)
            Dim model As Func = FuncParser.TryParse(statement)
            Dim handle As Func(Of Double(), Double) = model.GetExpression(__engine)
            Call Add(model.Name.ToLower, handle)
        End Sub

        ''' <summary>
        ''' This function return a random number, you can specific the boundary of the random number in the parameters. 
        ''' </summary>
        ''' <param name="UpBound">
        ''' If this parameter is empty or value is zero, then return the randome number between 0 and 1.
        ''' (如果这个参数为空或者其值为0，那么函数就会返回0和1之间的随机数)
        ''' </param>
        ''' <param name="LowBound"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function RND(LowBound As Double, UpBound As Double) As Double
            Dim rand As New Random(1000)
            If UpBound = 0R OrElse UpBound < LowBound Then
                Return rand.NextDouble
            Else
                Return LowBound + rand.NextDouble * (UpBound - LowBound)
            End If
        End Function
    End Class
End Namespace