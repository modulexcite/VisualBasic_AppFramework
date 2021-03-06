﻿'Namespace BasicR.Solvers

'    Public Class GaussianElimination : Implements BasicR.Solvers.ISolver, System.IDisposable

'        ''' <summary>
'        ''' a*b=0 -> x
'        ''' </summary>
'        ''' <param name="a"></param>
'        ''' <param name="b"></param>
'        ''' <returns>x</returns>
'        ''' <remarks></remarks>
'        Public Function Solve(A As MATRIX, b As VECTOR) As VECTOR Implements BasicR.Solvers.ISolver.Solve
'            Dim n As Integer = b.Length
'            Dim TMP As Double
'            Dim Ab As MATRIX = New MATRIX(Height:=n, Width:=n + 1)

'            For i As Integer = 0 To n - 1
'                For j As Integer = 0 To n - 1
'                    Ab(i, j) = A(i, j)
'                Next
'                Ab(i, n) = b(i)
'            Next

'            For k As Integer = 0 To n - 2 'Gaussian Elimination Core
'                For i = k + 1 To n - 1
'                    TMP = Ab(i, k) / Ab(k, k)
'                    For j = 0 To n
'                        Ab(i, j) = Ab(i, j) - TMP * Ab(k, j)
'                    Next
'                Next
'            Next

'            For i = 0 To n - 1
'                For j = 0 To n - 1
'                    A(i, j) = Ab(i, j)
'                Next
'                b(i) = Ab(i, n)
'            Next

'            Return UpTri(A, b)
'        End Function

'        ''' <summary>
'        ''' 上三角矩阵方程解法
'        ''' </summary>
'        ''' <param name="A"></param>
'        ''' <param name="b"></param>
'        ''' <returns></returns>
'        ''' <remarks></remarks>
'        Public Function UpTri(A As MATRIX, b As VECTOR) As VECTOR
'            Dim N As Integer = A.Height, x As VECTOR = New VECTOR(N)

'            x(N - 1) = b(N - 1) / A(N - 1, N - 1)

'            For i As Integer = N - 2 To 0 Step -1
'                x(i) = b(i)
'                For j As Integer = i + 1 To N - 1
'                    x(i) -= A(i, j) * x(j)
'                Next
'                x(i) /= A(i, i)
'            Next
'            Return x
'        End Function

'        Public Overrides Function ToString() As String
'            Return "BasicR -> Solver(GaussianElimination)"
'        End Function

'#Region "IDisposable Support"
'        Private disposedValue As Boolean ' 检测冗余的调用

'        ' IDisposable
'        Protected Overridable Sub Dispose(disposing As Boolean)
'            If Not Me.disposedValue Then
'                If disposing Then
'                    ' TODO:  释放托管状态(托管对象)。
'                End If

'                ' TODO:  释放非托管资源(非托管对象)并重写下面的 Finalize()。
'                ' TODO:  将大型字段设置为 null。
'            End If
'            Me.disposedValue = True
'        End Sub

'        ' TODO:  仅当上面的 Dispose( disposing As Boolean)具有释放非托管资源的代码时重写 Finalize()。
'        'Protected Overrides Sub Finalize()
'        '    ' 不要更改此代码。    请将清理代码放入上面的 Dispose( disposing As Boolean)中。
'        '    Dispose(False)
'        '    MyBase.Finalize()
'        'End Sub

'        ' Visual Basic 添加此代码是为了正确实现可处置模式。
'        Public Sub Dispose() Implements IDisposable.Dispose
'            ' 不要更改此代码。    请将清理代码放入上面的 Dispose (disposing As Boolean)中。
'            Dispose(True)
'            GC.SuppressFinalize(Me)
'        End Sub
'#End Region

'    End Class
'End Namespace