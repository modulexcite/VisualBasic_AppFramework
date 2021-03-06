﻿Imports Microsoft.VisualBasic.ComponentModel

Namespace Emit.Marshal

    Public Class Pointer(Of T) : Inherits DataStructures.Pointer(Of T)

        Protected __innerRaw As T()

        Public ReadOnly Property Current As T
            Get
                Return Value(Scan0)  ' 当前的位置是指相对于当前的位置offset为0的位置就是当前的位置
            End Get
        End Property

        Public ReadOnly Property Length As Integer
            Get
                Return __innerRaw.Length
            End Get
        End Property

        ''' <summary>
        ''' 相对于当前的指针的位置而言的
        ''' </summary>
        ''' <param name="p">相对于当前的位置的offset偏移量</param>
        ''' <returns></returns>
        Default Public Property Value(p As Integer) As T
            Get
                p += __index

                If p < 0 OrElse p >= __innerRaw.Length Then
                    Return Nothing
                Else
                    Return __innerRaw(p)
                End If
            End Get
            Set(value As T)
                p += __index

                If p < 0 OrElse p >= __innerRaw.Length Then
                    Throw New MemberAccessException(p & " reference to invalid memory region!")
                Else
                    __innerRaw(p) = value
                End If
            End Set
        End Property

        Public ReadOnly Property Raw As T()
            Get
                Return __innerRaw
            End Get
        End Property

        Public ReadOnly Property NullEnd(Optional offset As Integer = 0) As Boolean
            Get
                Return __index >= (__innerRaw.Length - 1 - offset)
            End Get
        End Property

        Public ReadOnly Property EndRead As Boolean
            Get
                Return __index >= __innerRaw.Length
            End Get
        End Property

        Sub New(ByRef array As T())
            __innerRaw = array
        End Sub

        Sub New(array As List(Of T))
            __innerRaw = array.ToArray
        End Sub

        Sub New(source As IEnumerable(Of T))
            __innerRaw = source.ToArray
        End Sub

        Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return $"* {GetType(T).Name} + {__index} --> {Current}  // {Scan0.ToString}"
        End Function

        ''' <summary>
        ''' 前移<paramref name="offset"/>个单位，然后返回值，这个和Peek的作用一样，不会改变指针位置
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="offset"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator <=(p As Pointer(Of T), offset As Integer) As T
            Return p(-offset)
        End Operator

        ''' <summary>
        ''' 后移<paramref name="offset"/>个单位，然后返回值，这个和Peek的作用一样，不会改变指针位置
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="offset"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator >=(p As Pointer(Of T), offset As Integer) As T
            Return p(offset)
        End Operator

        Public Overloads Shared Narrowing Operator CType(p As Pointer(Of T)) As T
            Return p.Current
        End Operator

        ''' <summary>
        ''' 前移<paramref name="offset"/>个单位，然后返回值，这个和Peek的作用一样，不会改变指针位置
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="offset"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator <(p As Pointer(Of T), offset As Integer) As T
            Return p(-offset)
        End Operator

        ''' <summary>
        ''' 后移<paramref name="offset"/>个单位，然后返回值，这个和Peek的作用一样，不会改变指针位置
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="offset"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator >(p As Pointer(Of T), offset As Integer) As T
            Return p(offset)
        End Operator

        Public Overloads Shared Widening Operator CType(raw As T()) As Pointer(Of T)
            Return New Pointer(Of T)(raw)
        End Operator

        Public Overloads Shared Operator +(ptr As Pointer(Of T), d As Integer) As Pointer(Of T)
            ptr.__index += d
            Return ptr
        End Operator

        Public Overloads Shared Operator -(ptr As Pointer(Of T), d As Integer) As Pointer(Of T)
            ptr.__index -= d
            Return ptr
        End Operator

        ''' <summary>
        ''' Pointer move to next and then returns is <see cref="EndRead"/>
        ''' </summary>
        ''' <returns></returns>
        Public Function MoveNext() As Boolean
            __index += 1
            Return Not EndRead
        End Function

        ''' <summary>
        ''' Pointer move to next and then returns the previous value
        ''' </summary>
        ''' <param name="ptr"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator +(ptr As Pointer(Of T)) As T
            Dim i As Integer = ptr.__index
            ptr.__index += 1
            Return ptr.__innerRaw(i)
        End Operator

        Public Overloads Shared Operator -(ptr As Pointer(Of T)) As T
            Dim i As Integer = ptr.__index
            ptr.__index -= 1
            Return ptr.__innerRaw(i)
        End Operator
    End Class
End Namespace