Imports System.Collections.Generic
Imports System.Linq
Imports System.Runtime.CompilerServices

Namespace StatisticsMathExtensions

    Public Module EnumerableStatsCovariance
        '
        ' Summary:
        '     Computes the Covariance of a sequence of nullable System.Decimal values.
        '
        ' Parameters:
        '   source:
        '     A sequence of nullable System.Decimal values to calculate the Covariance of.
        '
        ' Returns:
        '     The Covariance of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Decimal.MaxValue.
        <Extension>
        Public Function Covariance(source As IEnumerable(Of System.Nullable(Of Decimal)), other As IEnumerable(Of System.Nullable(Of Decimal))) As System.Nullable(Of Decimal)
            Dim values As IEnumerable(Of Decimal) = source.Coalesce()
            If values.Any() Then
                Return values.Covariance(other.Coalesce())
            End If

            Return Nothing
        End Function
        '
        ' Summary:
        '     Computes the Covariance of a sequence of System.Decimal values.
        '
        ' Parameters:
        '   source:
        '     A sequence of System.Decimal values to calculate the Covariance of.
        '
        ' Returns:
        '     The Covariance of the sequence of values.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        '
        '   System.InvalidOperationException:
        '     source contains no elements.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Decimal.MaxValue.
        <Extension>
        Public Function Covariance(source As IEnumerable(Of Decimal), other As IEnumerable(Of Decimal)) As Decimal
            Return CDec(source.[Select](Function(x) CDbl(x)).Covariance(other.[Select](Function(x) CDbl(x))))
        End Function
        '
        ' Summary:
        '     Computes the Covariance of a sequence of nullable System.Double values.
        '
        ' Parameters:
        '   source:
        '     A sequence of nullable System.Double values to calculate the Covariance of.
        '
        ' Returns:
        '     The Covariance of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        <Extension>
        Public Function Covariance(source As IEnumerable(Of System.Nullable(Of Double)), other As IEnumerable(Of System.Nullable(Of Double))) As System.Nullable(Of Double)
            Dim values As IEnumerable(Of Double) = source.Coalesce()
            If values.Any() Then
                Return values.Covariance(other.Coalesce())
            End If

            Return Nothing
        End Function
        '
        ' Summary:
        '     Computes the Covariance of a sequence of System.Double values.
        '
        ' Parameters:
        '   source:
        '     A sequence of System.Double values to calculate the Covariance of.
        '
        ' Returns:
        '     The Covariance of the sequence of values.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        '
        '   System.InvalidOperationException:
        '     source contains no elements.
        <Extension>
        Public Function Covariance(source As IEnumerable(Of Double), other As IEnumerable(Of Double)) As Double
            Dim len As Integer = source.Count()

            If len <> other.Count() Then
                Throw New ArgumentException("Collections are not of the same length", "other")
            End If

            Dim avgSource As Double = source.Average()
            Dim avgOther As Double = other.Average()
            Dim covariance__1 As Double = 0

            For i As Integer = 0 To len - 1
                covariance__1 += (source.ElementAt(i) - avgSource) * (other.ElementAt(i) - avgOther)
            Next

            Return covariance__1 / len
        End Function
        '
        ' Summary:
        '     Computes the Covariance of a sequence of nullable System.Single values.
        '
        ' Parameters:
        '   source:
        '     A sequence of nullable System.Single values to calculate the Covariance of.
        '
        ' Returns:
        '     The Covariance of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        <Extension>
        Public Function Covariance(source As IEnumerable(Of System.Nullable(Of Single)), other As IEnumerable(Of System.Nullable(Of Single))) As System.Nullable(Of Single)
            Dim values As IEnumerable(Of Single) = source.Coalesce()
            If values.Any() Then
                Return values.Covariance(other.Coalesce())
            End If

            Return Nothing
        End Function
        '
        ' Summary:
        '     Computes the Covariance of a sequence of System.Single values.
        '
        ' Parameters:
        '   source:
        '     A sequence of System.Single values to calculate the Covariance of.
        '
        ' Returns:
        '     The Covariance of the sequence of values.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        '
        '   System.InvalidOperationException:
        '     source contains no elements.
        <Extension>
        Public Function Covariance(source As IEnumerable(Of Single), other As IEnumerable(Of Single)) As Single
            Return CSng(source.[Select](Function(x) CDbl(x)).Covariance(other.[Select](Function(x) CDbl(x))))
        End Function
        '
        ' Summary:
        '     Computes the Covariance of a sequence of nullable System.Int32 values.
        '
        ' Parameters:
        '   source:
        '     A sequence of nullable System.Int32values to calculate the Covariance of.
        '
        ' Returns:
        '     The Covariance of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Int64.MaxValue.
        <Extension>
        Public Function Covariance(source As IEnumerable(Of System.Nullable(Of Integer)), other As IEnumerable(Of System.Nullable(Of Integer))) As System.Nullable(Of Double)
            Dim values As IEnumerable(Of Integer) = source.Coalesce()
            If values.Any() Then
                Return values.Covariance(other.Coalesce())
            End If

            Return Nothing
        End Function
        '
        ' Summary:
        '     Computes the Covariance of a sequence of System.Int32 values.
        '
        ' Parameters:
        '   source:
        '     A sequence of System.Int32 values to calculate the Covariance of.
        '
        ' Returns:
        '     The Covariance of the sequence of values.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        '
        '   System.InvalidOperationException:
        '     source contains no elements.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Int64.MaxValue.
        <Extension>
        Public Function Covariance(source As IEnumerable(Of Integer), other As IEnumerable(Of Integer)) As Double
            Return source.[Select](Function(x) CDbl(x)).Covariance(other.[Select](Function(x) CDbl(x)))
        End Function
        '
        ' Summary:
        '     Computes the Covariance of a sequence of nullable System.Int64 values.
        '
        ' Parameters:
        '   source:
        '     A sequence of nullable System.Int64 values to calculate the Covariance of.
        '
        ' Returns:
        '     The Covariance of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Int64.MaxValue.
        <Extension>
        Public Function Covariance(source As IEnumerable(Of System.Nullable(Of Long)), other As IEnumerable(Of System.Nullable(Of Long))) As System.Nullable(Of Double)
            Dim values As IEnumerable(Of Long) = source.Coalesce()
            If values.Any() Then
                Return values.Covariance(other.Coalesce())
            End If

            Return Nothing
        End Function
        '
        ' Summary:
        '     Computes the Covariance of a sequence of System.Int64 values.
        '
        ' Parameters:
        '   source:
        '     A sequence of System.Int64 values to calculate the Covariance of.
        '
        ' Returns:
        '     The Covariance of the sequence of values.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source is null.
        '
        '   System.InvalidOperationException:
        '     source contains no elements.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Int64.MaxValue.
        <Extension>
        Public Function Covariance(source As IEnumerable(Of Long), other As IEnumerable(Of Long)) As Double
            Return source.[Select](Function(x) CDbl(x)).Covariance(other.[Select](Function(x) CDbl(x)))
        End Function
        '
        ' Summary:
        '     Computes the Covariance of a sequence of nullable System.Decimal values that
        '     are obtained by invoking a transform function on each element of the input
        '     sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Covariance of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Covariance of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source or selector is null.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Decimal.MaxValue.
        <Extension>
        Public Function Covariance(Of TSource)(source As IEnumerable(Of TSource), other As IEnumerable(Of TSource), selector As Func(Of TSource, System.Nullable(Of Decimal))) As System.Nullable(Of Decimal)
            Return source.[Select](selector).Covariance(other.[Select](selector))
        End Function
        '
        ' Summary:
        '     Computes the Covariance of a sequence of System.Decimal values that are obtained
        '     by invoking a transform function on each element of the input sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values that are used to calculate an Covariance.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Covariance of the sequence of values.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source or selector is null.
        '
        '   System.InvalidOperationException:
        '     source contains no elements.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Decimal.MaxValue.
        <Extension>
        Public Function Covariance(Of TSource)(source As IEnumerable(Of TSource), other As IEnumerable(Of TSource), selector As Func(Of TSource, Decimal)) As Decimal
            Return source.[Select](selector).Covariance(other.[Select](selector))
        End Function
        '
        ' Summary:
        '     Computes the Covariance of a sequence of nullable System.Double values that
        '     are obtained by invoking a transform function on each element of the input
        '     sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Covariance of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Covariance of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source or selector is null.
        <Extension>
        Public Function Covariance(Of TSource)(source As IEnumerable(Of TSource), other As IEnumerable(Of TSource), selector As Func(Of TSource, System.Nullable(Of Double))) As System.Nullable(Of Double)
            Return source.[Select](selector).Covariance(other.[Select](selector))
        End Function
        '
        ' Summary:
        '     Computes the Covariance of a sequence of System.Double values that are obtained
        '     by invoking a transform function on each element of the input sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Covariance of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Covariance of the sequence of values.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source or selector is null.
        '
        '   System.InvalidOperationException:
        '     source contains no elements.
        <Extension>
        Public Function Covariance(Of TSource)(source As IEnumerable(Of TSource), other As IEnumerable(Of TSource), selector As Func(Of TSource, Double)) As Double
            Return source.[Select](selector).Covariance(other.[Select](selector))
        End Function
        '
        ' Summary:
        '     Computes the Covariance of a sequence of nullable System.Single values that
        '     are obtained by invoking a transform function on each element of the input
        '     sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Covariance of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Covariance of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source or selector is null.
        <Extension>
        Public Function Covariance(Of TSource)(source As IEnumerable(Of TSource), other As IEnumerable(Of TSource), selector As Func(Of TSource, System.Nullable(Of Single))) As System.Nullable(Of Single)
            Return source.[Select](selector).Covariance(other.[Select](selector))
        End Function
        '
        ' Summary:
        '     Computes the Covariance of a sequence of System.Single values that are obtained
        '     by invoking a transform function on each element of the input sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Covariance of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Covariance of the sequence of values.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source or selector is null.
        '
        '   System.InvalidOperationException:
        '     source contains no elements.
        <Extension>
        Public Function Covariance(Of TSource)(source As IEnumerable(Of TSource), other As IEnumerable(Of TSource), selector As Func(Of TSource, Single)) As Single
            Return source.[Select](selector).Covariance(other.[Select](selector))
        End Function
        '
        ' Summary:
        '     Computes the Covariance of a sequence of nullable System.Int32 values that are
        '     obtained by invoking a transform function on each element of the input sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Covariance of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Covariance of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source or selector is null.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Int64.MaxValue.
        <Extension>
        Public Function Covariance(Of TSource)(source As IEnumerable(Of TSource), other As IEnumerable(Of TSource), selector As Func(Of TSource, System.Nullable(Of Integer))) As System.Nullable(Of Double)
            Return source.[Select](selector).Covariance(other.[Select](selector))
        End Function
        '
        ' Summary:
        '     Computes the Covariance of a sequence of System.Int32 values that are obtained
        '     by invoking a transform function on each element of the input sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Covariance of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Covariance of the sequence of values.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source or selector is null.
        '
        '   System.InvalidOperationException:
        '     source contains no elements.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Int64.MaxValue.
        <Extension>
        Public Function Covariance(Of TSource)(source As IEnumerable(Of TSource), other As IEnumerable(Of TSource), selector As Func(Of TSource, Integer)) As Double
            Return source.[Select](selector).Covariance(other.[Select](selector))
        End Function
        '
        ' Summary:
        '     Computes the Covariance of a sequence of nullable System.Int64 values that are
        '     obtained by invoking a transform function on each element of the input sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Covariance of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Covariance of the sequence of values, or null if the source sequence is
        '     empty or contains only values that are null.
        <Extension>
        Public Function Covariance(Of TSource)(source As IEnumerable(Of TSource), other As IEnumerable(Of TSource), selector As Func(Of TSource, System.Nullable(Of Long))) As System.Nullable(Of Double)
            Return source.[Select](selector).Covariance(other.[Select](selector))
        End Function
        '
        ' Summary:
        '     Computes the Covariance of a sequence of System.Int64 values that are obtained
        '     by invoking a transform function on each element of the input sequence.
        '
        ' Parameters:
        '   source:
        '     A sequence of values to calculate the Covariance of.
        '
        '   selector:
        '     A transform function to apply to each element.
        '
        ' Type parameters:
        '   TSource:
        '     The type of the elements of source.
        '
        ' Returns:
        '     The Covariance of the sequence of values.
        '
        ' Exceptions:
        '   System.ArgumentNullException:
        '     source or selector is null.
        '
        '   System.InvalidOperationException:
        '     source contains no elements.
        '
        '   System.OverflowException:
        '     The sum of the elements in the sequence is larger than System.Int64.MaxValue.
        <Extension>
        Public Function Covariance(Of TSource)(source As IEnumerable(Of TSource), other As IEnumerable(Of TSource), selector As Func(Of TSource, Long)) As Double
            Return source.[Select](selector).Covariance(other.[Select](selector))
        End Function
    End Module
End Namespace