﻿Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.DocumentFormat.Csv.Extensions
Imports Microsoft.VisualBasic.DataVisualization.Network.Abstract
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports System.Text

Namespace FileStream

    Public Class Network : Inherits Network(Of Node, NetworkEdge)
        Sub New()
        End Sub

        Sub New(nodes As IEnumerable(Of Node), edges As IEnumerable(Of NetworkEdge))
            Call MyBase.New()

            Me.Nodes = nodes.ToArray
            Me.Edges = edges.ToArray
        End Sub

        Sub New(edges As IEnumerable(Of NetworkEdge), nodes As IEnumerable(Of Node))
            Call MyBase.New()

            Me.Nodes = nodes.ToArray
            Me.Edges = edges.ToArray
        End Sub

        ''' <summary>
        ''' 获取指定节点的连接数量
        ''' </summary>
        ''' <param name="node"></param>
        ''' <returns></returns>
        Public Function Links(node As String) As Integer
            Dim LQuery = (From x As NetworkEdge
                          In Edges
                          Where x.Contains(node)
                          Select x).Count
            Return LQuery
        End Function

        Public Overloads Shared Function Load(DIR As String) As Network
            Return New Network With {
                .Edges = $"{DIR}/Edges.csv".LoadCsv(Of NetworkEdge),
                .Nodes = $"{DIR}/Nodes.csv".LoadCsv(Of Node)
            }
        End Function
    End Class

    ''' <summary>
    ''' The network csv data information with specific type of the datamodel
    ''' </summary>
    ''' <typeparam name="T_Node"></typeparam>
    ''' <typeparam name="T_Edge"></typeparam>
    ''' <remarks></remarks>
    Public Class Network(Of T_Node As Node, T_Edge As NetworkEdge) : Inherits Language.UnixBash.File
        Implements IKeyValuePairObject(Of T_Node(), T_Edge())
        Implements ISaveHandle

        Public Property Nodes As T_Node() Implements IKeyValuePairObject(Of T_Node(), T_Edge()).Identifier
            Get
                If __nodes Is Nothing Then
                    __nodes = New Dictionary(Of T_Node)
                End If
                Return __nodes.Values.ToArray
            End Get
            Set(value As T_Node())
                If value Is Nothing Then
                    __nodes = New Dictionary(Of T_Node)
                Else
                    __nodes = value.ToDictionary
                End If
            End Set
        End Property
        Public Property Edges As T_Edge() Implements IKeyValuePairObject(Of T_Node(), T_Edge()).Value
            Get
                If __edges Is Nothing Then
                    __edges = New List(Of T_Edge)
                End If
                Return __edges.ToArray
            End Get
            Set(value As T_Edge())
                If value Is Nothing Then
                    __edges = New List(Of T_Edge)
                Else
                    __edges = value.ToList
                End If
            End Set
        End Property

        Sub New()
            __nodes = New Dictionary(Of T_Node)
            __edges = New List(Of T_Edge)
        End Sub

        Dim __nodes As Dictionary(Of T_Node)
        Dim __edges As List(Of T_Edge)

        ''' <summary>
        ''' 移除的重复的边
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub RemoveDuplicated()
            Dim LQuery = (From edge As T_Edge
                          In Edges.AsParallel
                          Let uu As String() = (From s As String
                                                In {edge.FromNode, edge.ToNode}
                                                Select s
                                                Order By s Ascending).ToArray
                          Select id = String.Join(";", uu),
                              edge = edge
                          Group By id Into Group).ToArray
            Edges = (From gpEdge
                     In LQuery
                     Select gpEdge.Group.First.edge).ToArray
        End Sub

        ''' <summary>
        ''' 移除自身与自身的边
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub RemoveSelfLoop()
            Dim LQuery = (From item As T_Edge
                          In Edges.AsParallel
                          Where Not item.SelfLoop
                          Select item).ToArray
            Edges = LQuery
        End Sub

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="outDIR">The data directory for the data export, if the value of this directory is null then the data
        ''' will be exported at the current work directory.
        ''' (进行数据导出的文件夹，假若为空则会保存数据至当前的工作文件夹之中)</param>
        ''' <param name="encoding">The file encoding of the exported node and edge csv file.</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function Save(Optional outDIR As String = "", Optional encoding As Encoding = Nothing) As Boolean Implements ISaveHandle.Save
            If String.IsNullOrEmpty(outDIR) Then outDIR = My.Computer.FileSystem.CurrentDirectory

            Call Nodes.SaveTo(String.Format("{0}/Nodes.csv", outDIR), False, encoding)
            Call Edges.SaveTo(String.Format("{0}/Edges.csv", outDIR), False, encoding)

            Return True
        End Function

        Public Shared Function Load(DIR As String) As Network(Of T_Node, T_Edge)
            Return New Network(Of T_Node, T_Edge) With {
                .Edges = $"{DIR}/Edges.csv".LoadCsv(Of T_Edge),
                .Nodes = $"{DIR}/Nodes.csv".LoadCsv(Of T_Node)
            }
        End Function

        Public Shared Operator +(net As Network(Of T_Node, T_Edge), x As T_Node) As Network(Of T_Node, T_Edge)
            Call net.__nodes.Add(x)
            Return net
        End Operator

        Public Shared Operator -(net As Network(Of T_Node, T_Edge), x As T_Node) As Network(Of T_Node, T_Edge)
            Call net.__nodes.Remove(x)
            Return net
        End Operator

        Public Shared Operator +(net As Network(Of T_Node, T_Edge), x As T_Edge) As Network(Of T_Node, T_Edge)
            Call net.__edges.Add(x)
            Return net
        End Operator

        Public Shared Operator -(net As Network(Of T_Node, T_Edge), x As T_Edge) As Network(Of T_Node, T_Edge)
            Call net.__edges.Remove(x)
            Return net
        End Operator

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="x">由于会调用ToArray，所以这里建议使用Iterator</param>
        ''' <returns></returns>
        Public Shared Operator +(net As Network(Of T_Node, T_Edge), x As IEnumerable(Of T_Node)) As Network(Of T_Node, T_Edge)
            Call net.__nodes.AddRange(x.ToArray)
            Return net
        End Operator

        Public Shared Operator -(net As Network(Of T_Node, T_Edge), lst As IEnumerable(Of T_Node)) As Network(Of T_Node, T_Edge)
            For Each x In lst
                Call net.__nodes.Remove(x)
            Next

            Return net
        End Operator

        ''' <summary>
        '''
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="x">由于会调用ToArray，所以这里建议使用Iterator</param>
        ''' <returns></returns>
        Public Shared Operator +(net As Network(Of T_Node, T_Edge), x As IEnumerable(Of T_Edge)) As Network(Of T_Node, T_Edge)
            Call net.__edges.AddRange(x.ToArray)
            Return net
        End Operator

        Public Shared Operator -(net As Network(Of T_Node, T_Edge), lst As IEnumerable(Of T_Edge)) As Network(Of T_Node, T_Edge)
            For Each x In lst
                Call net.__edges.Remove(x)
            Next

            Return net
        End Operator

        ''' <summary>
        ''' Network contains node?
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="node"></param>
        ''' <returns></returns>
        Public Shared Operator ^(net As Network(Of T_Node, T_Edge), node As String) As Boolean
            Return net.__nodes.ContainsKey(node)
        End Operator

        ''' <summary>
        ''' Network contains node?
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="node"></param>
        ''' <returns></returns>
        Public Shared Operator ^(net As Network(Of T_Node, T_Edge), node As T_Node) As Boolean
            Return net ^ node.Identifier
        End Operator

        ''' <summary>
        ''' GET node
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="node"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator &(net As Network(Of T_Node, T_Edge), node As String) As T_Node
            If net.__nodes.ContainsKey(node) Then
                Return net.__nodes(node)
            Else
                Return Nothing
            End If
        End Operator

        ''' <summary>
        ''' Select nodes from the network based on the input identifers <paramref name="nodes"/>
        ''' </summary>
        ''' <param name="net"></param>
        ''' <param name="nodes"></param>
        ''' <returns></returns>
        Public Shared Operator <=(net As Network(Of T_Node, T_Edge), nodes As IEnumerable(Of String)) As T_Node()
            Dim LQuery = (From sId As String In nodes Select net.__nodes(sId)).ToArray
            Return LQuery
        End Operator

        Public Shared Operator >=(net As Network(Of T_Node, T_Edge), nodes As IEnumerable(Of String)) As T_Node()
            Return net <= nodes
        End Operator

        Public Function GetNode(name As String) As T_Node
            Return Me & name
        End Function
    End Class
End Namespace