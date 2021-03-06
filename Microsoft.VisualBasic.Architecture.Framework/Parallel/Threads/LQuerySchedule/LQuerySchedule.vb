﻿Imports Microsoft.VisualBasic.Linq.Extensions

Namespace Parallel.Linq

    ''' <summary>
    ''' Parallel Linq query library for VisualBasic.
    ''' (用于高效率执行批量查询操作和用于检测操作超时的工具对象，请注意，为了提高查询的工作效率，请尽量避免在查询操作之中生成新的临时对象
    ''' 并行版本的LINQ查询和原始的线程操作相比具有一些性能上面的局限性)
    ''' </summary>
    ''' <remarks>
    ''' 在使用Parallel LINQ的时候，请务必要注意不能够使用Let语句操作共享变量，因为排除死锁的开销比较大
    ''' 
    ''' 在设计并行任务的时候应该遵循的一些原则:
    ''' 1. 假若每一个任务之间都是相互独立的话，则才可以进行并行化调用
    ''' 2. 在当前程序域之中只能够通过线程的方式进行并行化，对于时间较短的任务而言，非并行化会比并行化更加有效率
    ''' 3. 但是对于这些短时间的任务，仍然可以将序列进行分区合并为一个大型的长时间任务来产生并行化
    ''' 4. 对于长时间的任务，可以直接使用并行化Linq拓展执行并行化
    ''' 
    ''' 这个模块主要是针对大量的短时间的任务序列的并行化的，用户可以在这里配置线程的数量自由的控制并行化的程度
    ''' </remarks>
    Public Module LQuerySchedule

        ''' <summary>
        ''' Get the number of processors on the current machine.(获取当前的系统主机的CPU核心数)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property CPU_NUMBER As Integer
            Get
                Return Environment.ProcessorCount
            End Get
        End Property

        ''' <summary>
        ''' 假如小于0，则认为是自动配置，0被认为是单线程，反之直接返回
        ''' </summary>
        ''' <param name="n"></param>
        ''' <returns></returns>
        Public Function AutoConfig(n As Integer) As Integer
            If n < 0 Then
                Return CPU_NUMBER
            ElseIf n = 0 OrElse n = 1 Then
                Return 1
            Else
                Return n
            End If
        End Function

        ''' <summary>
        ''' The possible recommended threads of the linq based on you machine processors number, i'm not sure...
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Recommended_NUM_THREADS As Integer
            Get
                Return Environment.ProcessorCount * 10
            End Get
        End Property

        ''' <summary>
        ''' 将大量的短时间的任务进行分区，合并，然后再执行并行化
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="inputs"></param>
        ''' <param name="task"></param>
        ''' <param name="timeout">单个数据分区的计算的超时的时间，单位是秒</param>
        ''' <returns></returns>
        Public Iterator Function LQuery(Of T, TOut)(inputs As IEnumerable(Of T),
                                                    task As Func(Of T, TOut),
                                                    timeout As Double,
                                                    Optional parTokens As Integer = 20000) As IEnumerable(Of TOut)

            Call $"Start schedule task pool(timeout:={timeout}s) for {GetType(T).FullName}  -->  {GetType(TOut).FullName}".__DEBUG_ECHO

            Dim buf = TaskPartitions.Partitioning(inputs, parTokens, task)
            Dim LQueryInvoke = From part As Func(Of TOut())
                               In buf.AsParallel
                               Select New TimeoutModel(Of TOut) With {
                                   .timeout = timeout,
                                   .task = part
                               }.Invoke

            For Each part As TOut() In LQueryInvoke
                If part Is Nothing Then
                    Call VBDebugger.Warning("Parts of the data operation timeout!")
                    Continue For
                End If

                For Each x As TOut In part
                    Yield x
                Next
            Next

            Call $"Task job done!".__DEBUG_ECHO
        End Function

        ''' <summary>
        ''' 将大量的短时间的任务进行分区，合并，然后再执行并行化
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="inputs"></param>
        ''' <param name="task"></param>
        ''' <returns></returns>
        Public Iterator Function LQuery(Of T, TOut)(inputs As IEnumerable(Of T), task As Func(Of T, TOut), Optional parTokens As Integer = 20000) As IEnumerable(Of TOut)
            Call $"Start schedule task pool for {GetType(T).FullName}  -->  {GetType(TOut).FullName}".__DEBUG_ECHO

            Dim buf = TaskPartitions.Partitioning(inputs, parTokens, task)
            Dim LQueryInvoke = From part As Func(Of TOut())
                               In buf.AsParallel
                               Select part()

            For Each part As TOut() In LQueryInvoke
                For Each x As TOut In part
                    Yield x
                Next
            Next

            Call $"Task job done!".__DEBUG_ECHO
        End Function
    End Module
End Namespace