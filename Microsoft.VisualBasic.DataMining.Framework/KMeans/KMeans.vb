﻿Imports System.Data
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.Framework.ComponentModel
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace KMeans

    ''' <summary>
    ''' This class implement a KMeans clustering algorithm.(请注意，实体对象的属性必须要长度一致)
    ''' </summary>
    Public Module KMeansAlgorithm

        ''' <summary>
        ''' Calculates the Euclidean Distance Measure between two data points
        ''' </summary>
        ''' <param name="X">An array with the values of an object or datapoint</param>
        ''' <param name="Y">An array with the values of an object or datapoint</param>
        ''' <returns>Returns the Euclidean Distance Measure Between Points X and Points Y</returns>
        Public Function EuclideanDistance(X As Double(), Y As Double()) As Double
            Dim count As Integer = 0
            Dim sum As Double = 0.0

            If X.GetUpperBound(0) <> Y.GetUpperBound(0) Then
                Throw New ArgumentException(DimNotAgree)
            Else
                count = X.Length
            End If

            For i As Integer = 0 To count - 1
                sum = sum + Math.Pow(Math.Abs(X(i) - Y(i)), 2)
            Next

            Dim distance As Double = Math.Sqrt(sum)
            Return distance
        End Function

        Const DimNotAgree As String = "The number of elements in X must match the number of elements in Y!"

        ''' <summary>
        ''' Calculates the Manhattan Distance Measure between two data points
        ''' </summary>
        ''' <param name="X">An array with the values of an object or datapoint</param>
        ''' <param name="Y">An array with the values of an object or datapoint</param>
        ''' <returns>Returns the Manhattan Distance Measure Between Points X and Points Y</returns>
        Public Function ManhattanDistance(X As Double(), Y As Double()) As Double
            Dim count As Integer = 0
            Dim sum As Double = 0.0

            If X.GetUpperBound(0) <> Y.GetUpperBound(0) Then
                Throw New ArgumentException(DimNotAgree)
            Else
                count = X.Length
            End If

            For i As Integer = 0 To count - 1
                sum = sum + Math.Abs(X(i) - Y(i))
            Next

            Return sum
        End Function

        ''' <summary>
        ''' Calculates The Mean Of A Cluster OR The Cluster Center
        ''' </summary>
        ''' <param name="cluster">
        ''' A two-dimensional array containing a dataset of numeric values
        ''' </param>
        ''' <returns>
        ''' Returns an Array Defining A Data Point Representing The Cluster Mean or Centroid
        ''' </returns>
        Public Function ClusterMean(cluster As Double(,)) As Double()
            Dim rowCount As Integer = 0
            Dim fieldCount As Integer = 0
            Dim dataSum As Double(,)
            Dim centroid As Double()

            rowCount = cluster.GetUpperBound(0) + 1
            fieldCount = cluster.GetUpperBound(1) + 1
            dataSum = New Double(0, fieldCount - 1) {}
            centroid = New Double(fieldCount - 1) {}

            '((20+30)/2), ((170+160)/2), ((80+120)/2)
            For j As Integer = 0 To fieldCount - 1
                For i As Integer = 0 To rowCount - 1
                    dataSum(0, j) = dataSum(0, j) + cluster(i, j)
                Next

                centroid(j) = (dataSum(0, j) / rowCount)
            Next

            Return centroid
        End Function

        ''' <summary>
        ''' Seperates a dataset into clusters or groups with similar characteristics
        ''' </summary>
        ''' <param name="clusterCount">The number of clusters or groups to form</param>
        ''' <param name="source">An array containing data that will be clustered</param>
        ''' <returns>A collection of clusters of data</returns>
        <Extension> Public Function ClusterDataSet(Of T As EntityBase(Of Double))(clusterCount As Integer, source As IEnumerable(Of T)) As ClusterCollection(Of T)
            Dim data As T() = source.ToArray
            Dim clusterNumber As Integer = 0
            Dim rowCount As Integer = data.Length
            Dim fieldCount As Integer = data(Scan0).Length
            Dim stableClustersCount As Integer = 0
            Dim iterationCount As Integer = 0
            Dim cluster As Cluster(Of T) = Nothing
            Dim clusters As New ClusterCollection(Of T)
            Dim clusterNumbers As New List(Of Integer)
            Dim Random As Random = New Random

            If clusterCount >= rowCount Then
                Throw New Exception($"{clusterCount} >= {rowCount}, this will caused a dead loop!")
            End If

            While clusterNumbers.Count < clusterCount
                clusterNumber = Random.[Next](0, rowCount - 1)

                If Not clusterNumbers.Contains(clusterNumber) Then
                    cluster = New Cluster(Of T)
                    clusterNumbers.Add(clusterNumber)
                    cluster.Add(data(clusterNumber))
                    clusters.Add(cluster)
                End If
            End While

            Dim _stop As Integer = clusterCount * rowCount

            While stableClustersCount <> clusters.NumOfCluster
                stableClustersCount = 0

                Dim newClusters As ClusterCollection(Of T) = ClusterDataSet(clusters, data)

                For clusterIndex As Integer = 0 To clusters.NumOfCluster - 1
                    Dim x As Cluster(Of T) = newClusters(clusterIndex)
                    Dim y As Cluster(Of T) = clusters(clusterIndex)

                    If x.NumOfEntity = 0 OrElse y.NumOfEntity = 0 Then
                        Continue For ' ??? 为什么有些聚类是0？？
                    End If

                    If (EuclideanDistance(x.ClusterMean, y.ClusterMean)) = 0 Then
                        stableClustersCount += 1
                    End If
                Next

                iterationCount += 1
                clusters = newClusters

                If iterationCount > _stop Then
                    Exit While
                End If
            End While

            Return clusters
        End Function

        ''' <summary>
        ''' Seperates a dataset into clusters or groups with similar characteristics
        ''' </summary>
        ''' <param name="clusters">A collection of data clusters</param>
        ''' <param name="source">An array containing data to b eclustered</param>
        ''' <returns>A collection of clusters of data</returns>
        Public Function ClusterDataSet(Of T As EntityBase(Of Double))(clusters As ClusterCollection(Of T), source As IEnumerable(Of T)) As ClusterCollection(Of T)
            Dim data As T() = source.ToArray
            Dim clusterMean As Double()
            Dim firstClusterDistance As Double = 0.0
            Dim secondClusterDistance As Double = 0.0
            Dim rowCount As Integer = data.Length
            Dim fieldCount As Integer = data(Scan0).Length
            Dim position As Integer = 0
            Dim newClusters As New ClusterCollection(Of T)     ' create a new collection of clusters

            For count As Integer = 0 To clusters.NumOfCluster - 1
                Dim newCluster As New Cluster(Of T)
                newClusters.Add(newCluster)
            Next

            If clusters.NumOfCluster <= 0 Then
                Throw New SystemException("Cluster Count Cannot Be Zero!")
            End If

            '((20+30)/2), ((170+160)/2), ((80+120)/2)
            For row As Integer = 0 To rowCount - 1
                Dim dataPoint As T = data(row)

                For cluster As Integer = 0 To clusters.NumOfCluster - 1
                    Dim x As Cluster(Of T) = clusters(cluster)
                    If x.NumOfEntity = 0 Then
                        clusterMean = 0R.CopyVector(dataPoint.Length)
                    Else
                        clusterMean = x.ClusterMean
                    End If

                    If cluster = 0 Then
                        firstClusterDistance = EuclideanDistance(dataPoint.Properties, clusterMean)
                        position = cluster
                    Else
                        secondClusterDistance = EuclideanDistance(dataPoint.Properties, clusterMean)

                        If firstClusterDistance > secondClusterDistance Then
                            firstClusterDistance = secondClusterDistance
                            position = cluster
                        End If
                    End If
                Next

                newClusters(position).Add(dataPoint)
            Next

            Return newClusters
        End Function

        ''' <summary>
        ''' Converts a System.Data.DataTable to a 2-dimensional array
        ''' </summary>
        ''' <param name="table">A System.Data.DataTable containing data to cluster</param>
        ''' <returns>A 2-dimensional array containing data to cluster</returns>
        <Extension> Public Function ToFloatMatrix(table As DataTable) As Double(,)
            Dim rowCount As Integer = table.Rows.Count
            Dim fieldCount As Integer = table.Columns.Count
            Dim dataPoints As Double(,)
            Dim fieldValue As Double = 0.0
            Dim row As DataRow

            dataPoints = New Double(rowCount - 1, fieldCount - 1) {}

            For rowPosition As Integer = 0 To rowCount - 1
                row = table.Rows(rowPosition)

                For fieldPosition As Integer = 0 To fieldCount - 1
                    Try
                        fieldValue = Double.Parse(row(fieldPosition).ToString())
                    Catch ex As System.Exception
                        Dim msg As String = $"Invalid row at {rowPosition.ToString()} and field {fieldPosition.ToString()}"
                        ex = New InvalidCastException(msg, ex)
                        ex.PrintException

                        Throw ex
                    End Try

                    dataPoints(rowPosition, fieldPosition) = fieldValue
                Next
            Next

            Return dataPoints
        End Function
    End Module
End Namespace
