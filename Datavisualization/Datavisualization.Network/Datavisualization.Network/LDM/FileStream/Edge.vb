﻿Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.DocumentFormat.Csv.Extensions
Imports Microsoft.VisualBasic.DataVisualization.Network.Abstract

Namespace FileStream

    ''' <summary>
    ''' The edge between the two nodes in the network.(节点与节点之间的相互关系)
    ''' </summary>
    ''' <remarks></remarks>
    <XmlType("VisualizeNode")>
    Public Class NetworkEdge : Inherits INetComponent
        Implements IInteraction, INetworkEdge

        Public Function Contains(Interactor As String) As Boolean
            Return String.Equals(Interactor, FromNode, StringComparison.OrdinalIgnoreCase) OrElse
                String.Equals(Interactor, ToNode, StringComparison.OrdinalIgnoreCase)
        End Function

        Public Sub New()
        End Sub

        Sub New(from As String, target As String, confi As Double)
            Me.FromNode = from
            Me.ToNode = target
            Me.Confidence = confi
        End Sub

        <Column("fromNode")> <XmlAttribute("source")>
        Public Overridable Property FromNode As String Implements IInteraction.source
        <Column("toNode")> <XmlAttribute("target")>
        Public Overridable Property ToNode As String Implements IInteraction.target
        <XmlAttribute("confidence")>
        Public Overridable Property Confidence As Double Implements INetworkEdge.Confidence
        <Column("InteractionType")>
        Public Overridable Property InteractionType As String Implements INetworkEdge.InteractionType

        Public Iterator Function Nodes() As IEnumerable(Of String)
            Yield FromNode
            Yield ToNode
        End Function

        ''' <summary>
        ''' 返回没有方向性的统一标识符
        ''' </summary>
        ''' <returns></returns>
        Public Function GetNullDirectedGuid() As String
            Dim array = {FromNode, ToNode}.OrderBy(Function(s) s)
            Return String.Format("[{0}] {1};{2}", InteractionType, array.First, array.Last)
        End Function

        Public Function GetDirectedGuid() As String
            Return $"{FromNode} {InteractionType} {ToNode}"
        End Function

        ''' <summary>
        ''' 起始节点是否是终止节点
        ''' </summary>
        ''' <returns></returns>
        <Ignored> Public ReadOnly Property SelfLoop As Boolean
            Get
                Return String.Equals(FromNode, ToNode, StringComparison.OrdinalIgnoreCase)
            End Get
        End Property

        ''' <summary>
        ''' 假若存在连接则返回相对的节点，否则返回空字符串
        ''' </summary>
        ''' <param name="Node"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetConnectedNode(Node As String) As String
            Return Abstract.GetConnectedNode(Me, Node)
        End Function

        Public Overloads Function Equals(Id1 As String, Id2 As String) As Boolean
            Return (String.Equals(FromNode, Id1) AndAlso
                String.Equals(ToNode, Id2)) OrElse
                (String.Equals(FromNode, Id2) AndAlso
                String.Equals(ToNode, Id1))
        End Function

        Public Function IsEqual(OtherNode As NetworkEdge) As Boolean
            Return String.Equals(FromNode, OtherNode.FromNode) AndAlso
                String.Equals(ToNode, OtherNode.ToNode) AndAlso
                String.Equals(InteractionType, OtherNode.InteractionType) AndAlso
                Confidence = OtherNode.Confidence
        End Function

        Public Overrides Function ToString() As String
            If String.IsNullOrEmpty(ToNode) Then
                Return FromNode
            Else
                If String.IsNullOrEmpty(InteractionType) Then
                    Return String.Format("{0} --> {1}", FromNode, ToNode)
                Else
                    Return String.Format("{0} {1} {2}", FromNode, InteractionType, ToNode)
                End If
            End If
        End Function

        Public Shared Function GetNode(Node1 As String, Node2 As String, Network As NetworkEdge()) As NetworkEdge
            Dim LQuery = (From Node As NetworkEdge
                          In Network
                          Where String.Equals(Node1, Node.FromNode) AndAlso
                              String.Equals(Node2, Node.ToNode)
                          Select Node).ToArray

            If LQuery.Length > 0 Then Return LQuery(Scan0)

            Dim Found = (From Node As NetworkEdge
                         In Network
                         Where String.Equals(Node1, Node.ToNode) AndAlso
                              String.Equals(Node2, Node.FromNode)
                         Select Node).FirstOrDefault
            Return Found
        End Function

        Public Shared Operator +(list As List(Of NetworkEdge), x As NetworkEdge) As List(Of NetworkEdge)
            Call list.Add(x)
            Return list
        End Operator

        Public Shared Operator -(list As List(Of NetworkEdge), x As NetworkEdge) As List(Of NetworkEdge)
            Call list.Remove(x)
            Return list
        End Operator
    End Class
End Namespace