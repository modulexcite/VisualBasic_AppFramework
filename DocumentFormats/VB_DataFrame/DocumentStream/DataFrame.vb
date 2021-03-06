﻿Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection
Imports System.Text
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.ComponentModels
Imports Microsoft.VisualBasic.Terminal
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.Language

Namespace DocumentStream

    ''' <summary>
    ''' The dynamics data frame object which its first line is not contains the but using for the title property.
    ''' (第一行总是没有的，即本对象类型适用于第一行为列标题行的数据)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class DataFrame : Inherits File
        Implements ISchema
        Implements System.Data.IDataReader
        Implements System.IDisposable
        Implements IEnumerable(Of DynamicObjectLoader)

        ''' <summary>
        ''' <see cref="__currentLine"></see>在<see cref="_innerTable"></see>之中的位置
        ''' </summary>
        ''' <remarks></remarks>
        Dim __currentPointer As Integer = -1
        Dim __currentLine As RowObject

        ''' <summary>
        ''' Using the first line of the csv row as the column headers in this csv file.
        ''' </summary>
        ''' <remarks></remarks>
        Protected __columnList As List(Of String)
        Public ReadOnly Property SchemaOridinal As Dictionary(Of String, Integer) Implements ISchema.SchemaOridinal

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="MappingData">{oldFieldName, newFieldName}</param>
        ''' <remarks></remarks>
        Public Sub ChangeMapping(MappingData As Dictionary(Of String, String))
            For Each ColumnName As KeyValuePair(Of String, String) In MappingData
                Dim p As Integer = __columnList.IndexOf(ColumnName.Key)
                If Not p = -1 Then '由于只是改变映射的名称，并不要添加新的列，所以在这里忽略掉不存在的列
                    __columnList(p) = ColumnName.Value
                    _SchemaOridinal.Remove(ColumnName.Key)
                    _SchemaOridinal.Add(ColumnName.Value, p)
                End If
            Next
        End Sub

        Public Function AddAttribute(Name As String) As Integer
            If SchemaOridinal.ContainsKey(Name) Then
                Return SchemaOridinal(Name)
            Else
                Dim p As Integer = __columnList.Count
                Call __columnList.Add(Name)
                Call _SchemaOridinal.Add(Name, p)
                Return p
            End If
        End Function

        Private Shared Function __createSchemaOridinal(df As DataFrame) As Dictionary(Of String, Integer)
            Dim arrayCache As String() = df.__columnList.ToArray

            Try
                Return arrayCache.Sequence _
                    .ToDictionary(Function(oridinal) arrayCache(oridinal),
                                  Function(oridinal) oridinal)
            Catch ex As Exception
                Dim sb As New StringBuilder("There is an duplicated key exists in your csv table, please delete the duplicated key and try load again!")
                Call sb.AppendLine("Here is the column header keys in you data: ")
                Call sb.AppendLine()
                Call sb.AppendLine("   " & String.Join(vbTab, arrayCache.ToArray(Of String)(Function(s) "[" & s & "]").ToArray))

                Throw New DataException(sb.ToString, ex)
            End Try
        End Function

        ''' <summary>
        ''' Get the lines data for the convinent data operation.(为了保持一致的顺序，这个函数是非并行化的)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CreateDataSource() As DynamicObjectLoader()
            Dim LQuery As DynamicObjectLoader() =
                LinqAPI.Exec(Of DynamicObjectLoader) <= From i As Integer
                                                        In Me.RowNumbers.Sequence.AsParallel
                                                        Let Line As DocumentStream.RowObject =
                                                            Me._innerTable(i)  '已经去掉了首行标题行了的
                                                        Select row = New DynamicObjectLoader With {
                                                            .LineNumber = i,
                                                            .RowData = Line,
                                                            .Schema = Me.SchemaOridinal,
                                                            ._innerDataFrame = Me
                                                        }
                                                        Order By row.LineNumber Ascending
            Return LQuery
        End Function

        Public ReadOnly Property HeadTitles As String()
            Get
                Return __columnList.ToArray
            End Get
        End Property

        Public ReadOnly Property Depth As Integer Implements IDataReader.Depth
            Get
                Return 0
            End Get
        End Property

        Public ReadOnly Property IsClosed As Boolean Implements IDataReader.IsClosed
            Get
                Return False
            End Get
        End Property

        Public ReadOnly Property RecordsAffected As Integer Implements IDataReader.RecordsAffected
            Get
                Return 0
            End Get
        End Property

        Public ReadOnly Property FieldCount As Integer Implements IDataRecord.FieldCount
            Get
                Return __columnList.Count
            End Get
        End Property

        Private ReadOnly Property IDataRecord_Item(i As Integer) As Object Implements IDataRecord.Item
            Get
                Return IDataRecord_GetValue(i)
            End Get
        End Property

        Public Overloads ReadOnly Property Item(name As String) As Object Implements IDataRecord.Item
            Get
                Return IDataRecord_GetValue(GetOrdinal(name))
            End Get
        End Property

        Public Function CreateDocument() As Csv.DocumentStream.File
            Dim File As New Csv.DocumentStream.File
            Call File.AppendLine(Me.__columnList.ToCsvRow)
            Call File.AppendRange(Me._innerTable)
            Return File
        End Function

        Protected Friend Sub New()
        End Sub

        ''' <summary>
        ''' Try loading a excel csv data file as a dynamics data frame object.(尝试加载一个Csv文件为数据框对象，请注意，第一行必须要为标题行)
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Shared Function Load(path As String, encoding As Encoding, Optional fast As Boolean = False) As DataFrame
            Dim File As File = If(fast, File.FastLoad(path, True, encoding), File.Load(path, encoding))
            Return CreateObject(File)
        End Function

        Private Shared Function __getColumnList(table As Generic.IEnumerable(Of Csv.DocumentStream.RowObject)) As List(Of String)
            Return (From strValue As String In table.First Select __reviewColumnHeader(strValue)).ToList
        End Function

        ''' <summary>
        ''' 这里不能够使用Trim函数，因为Column也可能是故意定义了空格在其实或者结束的位置的，使用Trim函数之后，反而会导致GetOrder函数执行失败。故而在这里只给出警告信息即可
        ''' </summary>
        ''' <param name="strValue"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function __reviewColumnHeader(strValue As String) As String
            If String.IsNullOrEmpty(strValue) Then
                Call "[CSV::Reflector::Warnning] There are empty column header in your data!".__DEBUG_ECHO
                Return ""
            End If

            Dim ch As Char = strValue.First

            If ch = " "c OrElse ch = vbTab Then
                Call xConsole.WriteLine($"^y{String.Format(FailureWarning, strValue)}^!")
            End If
            ch = strValue.Last
            If ch = " "c OrElse ch = vbTab Then
                Call xConsole.WriteLine($"^y{String.Format(FailureWarning, strValue)}^!")
            End If

            Return strValue '这里不能够使用Trim函数，因为Column也可能是故意定义了空格在其实或者结束的位置的，使用Trim函数之后，反而会导致GetOrder函数执行失败。故而在这里只给出警告信息即可
        End Function

        Const FailureWarning As String =
            "[CSV::Reflector::Warning] The Column header ""{0}"" end with the space character value, this may caused the GetOrder() function execute failure!"

        Public Overloads Shared Function CreateObject(CsvDf As Csv.DocumentStream.File) As DataFrame
            Try
                Return __createObject(CsvDf)
            Catch ex As Exception
                Call $"Error during read file from handle {CsvDf.FileName.ToFileURL}".__DEBUG_ECHO
                Call ex.PrintException
                Throw
            End Try
        End Function

        Private Shared Function __createObject(CsvDf As Csv.DocumentStream.File) As DataFrame
            Dim df As DataFrame = New DataFrame With {
                  ._innerTable = CsvDf._innerTable.Skip(1).ToList,
                  .FilePath = CsvDf.FileName
            }
            df.__columnList = __getColumnList(CsvDf._innerTable)
            df._SchemaOridinal = __createSchemaOridinal(df)

            Return df
        End Function

        Protected Friend Overrides Function __createTableVector() As RowObject()
            Dim readBuffer As New List(Of RowObject)({CType(Me.__columnList, RowObject)})
            Call readBuffer.AddRange(_innerTable)
            Return readBuffer.ToArray
        End Function

        Public Overrides Function Generate() As String
            Dim sb As StringBuilder = New StringBuilder(1024)
            Dim head As String =
                New RowObject(__columnList).AsLine

            Call sb.AppendLine(head)

            For Each row As RowObject In _innerTable
                Call sb.AppendLine(row.AsLine)
            Next

            Return sb.ToString
        End Function

        ''' <summary>
        ''' Function return -1 when column not found. 
        ''' </summary>
        ''' <param name="Column"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function GetOrdinal(Column As String) As Integer Implements IDataRecord.GetOrdinal, ISchema.GetOrdinal
            Return __columnList.IndexOf(Column)
        End Function

        ''' <summary>
        ''' Gets the order list of the specific column list, -1 value will be returned when it is not exists in the table.
        ''' (获取列集合的位置列表，不存在的列则返回-1)
        ''' </summary>
        ''' <param name="ColumnList"></param>
        ''' <returns></returns>
        ''' <remarks>由于存在一一对应关系，这里不会再使用并行拓展</remarks>
        Public Function GetOrdinalSchema(ColumnList As String()) As Integer()
            Dim LQuery As Integer() = (From column As String
                                       In ColumnList
                                       Select Me.__columnList.IndexOf(column)).ToArray
            Return LQuery
        End Function

        Public Function GetValue(Ordinal As Integer) As String
#If DEBUG Then
            If Ordinal > Me.__currentLine.Count - 1 Then
                Return ""
            End If
#End If
            Return __currentLine.Column(Ordinal)
        End Function

        ''' <summary>
        ''' The data frame object start to reading the data in this table, if the current pointer is reach 
        ''' the top of the lines then this function will returns FALSE to stop the reading loop.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overloads Function Read() As Boolean Implements IDataReader.Read, IDataReader.NextResult
            If __currentPointer = _innerTable.Count - 1 Then
                Return False
            Else
                __currentPointer += 1
                __currentLine = _innerTable(__currentPointer)

                Return True
            End If
        End Function

        ''' <summary>
        ''' Reset the reading position in the data frame object.
        ''' </summary>
        ''' <remarks></remarks>
        Public Overrides Sub Reset()
            __currentPointer = -1
        End Sub

        ''' <summary>
        ''' 这个方法会清除当前对象之中的原有数据
        ''' </summary>
        ''' <param name="source"></param>
        ''' <remarks></remarks>
        Public Sub CopyFrom(source As File)
            _innerTable = source._innerTable.Skip(1).ToList
            FilePath = source.FileName
            __columnList = source._innerTable.First.ToList
        End Sub

        Public Overrides Function ToString() As String
            Return FilePath.ToFileURL & "  // " & _innerTable(__currentPointer).ToString
        End Function

        'Public Sub ShowDialog(Optional Title As String = "")
        '    Dim Dialog = New CsvChartDevice

        '    If String.IsNullOrEmpty(Title) Then
        '        Dialog.Text = FilePath
        '    Else
        '        Dialog.Text = Title
        '        Dialog._chart.Titles.Add(Title)
        '        Dim T = Dialog._chart.Titles("Title1")
        '        T.Font = New Font("Microsoft YaHei", 16, FontStyle.Bold)
        '    End If

        '    Dialog.Draw(Me)

        '    Call Dialog.ShowDialog()
        'End Sub

        Public Function Take(ColumnList As String()) As DataFrame
            Dim PQuery As IEnumerable(Of Integer) =
                From Column As String
                In ColumnList
                Select Me.__columnList.IndexOf(Column) '
            Dim pList As List(Of Integer) = PQuery.ToList   'Location pointer to the column
            Dim NewTable As New List(Of RowObject)

            Call Me.Reset()

            Do While Me.Read
                Dim Query As IEnumerable(Of String) = From p In pList Select __currentLine.Column(p) '
                NewTable.Add(New RowObject(Query))
            Loop

            Return New DataFrame With {
                .__columnList = ColumnList.ToList,
                .FilePath = FileName,
                ._innerTable = NewTable
            }
        End Function

        Public Iterator Function GetEnumerator2() As IEnumerator(Of DynamicObjectLoader) Implements IEnumerable(Of DynamicObjectLoader).GetEnumerator
            Dim ColumnSchema As Dictionary(Of String, Integer) =
                (From i As Integer
                 In Me.__columnList.Sequence
                 Select New KeyValuePair(Of String, Integer)(Me.__columnList(i), i)) _
                       .ToDictionary(Function(itm) itm.Key,
                                     Function(itm) itm.Value)
            For Each Item As DynamicObjectLoader In From i As Integer In Me.RowNumbers.Sequence
                                                    Let Line As RowObject = Me(i)
                                                    Let loader = New DynamicObjectLoader With {
                                                        .LineNumber = i,
                                                        .RowData = Line,
                                                        .Schema = ColumnSchema
                                                    }
                                                    Select loader
                Yield Item
            Next
        End Function

        ''' <summary>
        ''' Closes the <see cref="System.Data.IDataReader"/>:<see cref="DataFrame"/> Object.  
        ''' </summary>
        Public Sub Close() Implements IDataReader.Close
            Throw New NotImplementedException()
        End Sub

        ''' <summary>
        ''' Returns a System.Data.DataTable that describes the column metadata of the System.Data.IDataReader.
        ''' </summary>
        ''' <returns>A System.Data.DataTable that describes the column metadata.</returns>
        Public Function GetSchemaTable() As DataTable Implements IDataReader.GetSchemaTable
            Throw New NotImplementedException()
        End Function

        Public Function GetName(i As Integer) As String Implements IDataRecord.GetName
            Return __columnList(i)
        End Function

        Public Function GetDataTypeName(i As Integer) As String Implements IDataRecord.GetDataTypeName
            Dim value As String = GetValue(i)
            If IsNumeric(value) Then
                Return "System.Double"
            ElseIf InStr(value, ", ") > 0 OrElse InStr(value, "; ") > 0 Then
                Return "System.String()"
            Else
                Return "System.String"
            End If
        End Function

        Public Function GetFieldType(i As Integer) As Type Implements IDataRecord.GetFieldType
            Dim typeName As String = GetDataTypeName(i)
            Return Scripting.InputHandler.GetType(typeName, True)
        End Function

        Private Function IDataRecord_GetValue(i As Integer) As Object Implements IDataRecord.GetValue
            Return __currentLine.Column(i)
        End Function

        Public Function GetValues(values() As Object) As Integer Implements IDataRecord.GetValues
            If values.IsNullOrEmpty Then
                Return 0
            Else
                For i As Integer = 0 To values.Length - 1
                    values(i) = __currentLine.Column(i)
                Next

                Return values.Length
            End If
        End Function

        Public Function GetBoolean(i As Integer) As Boolean Implements IDataRecord.GetBoolean
            Dim value As String = __currentLine.Column(i)
            Return Scripting.CTypeDynamic(Of Boolean)(value)
        End Function

        Public Function GetByte(i As Integer) As Byte Implements IDataRecord.GetByte
            Dim value As String = __currentLine.Column(i)
            Return Scripting.CTypeDynamic(Of Byte)(value)
        End Function

        Public Function GetBytes(i As Integer, fieldOffset As Long, buffer() As Byte, bufferoffset As Integer, length As Integer) As Long Implements IDataRecord.GetBytes
            Throw New NotImplementedException()
        End Function

        Public Function GetChar(i As Integer) As Char Implements IDataRecord.GetChar
            Dim value As String = __currentLine.Column(i)
            Return Scripting.CTypeDynamic(Of Char)(value)
        End Function

        Public Function GetChars(i As Integer, fieldoffset As Long, buffer() As Char, bufferoffset As Integer, length As Integer) As Long Implements IDataRecord.GetChars
            Throw New NotImplementedException()
        End Function

        Public Function GetGuid(i As Integer) As Guid Implements IDataRecord.GetGuid
            Dim value As String = __currentLine.Column(i)
            Return Scripting.CTypeDynamic(Of Guid)(value)
        End Function

        Public Function GetInt16(i As Integer) As Short Implements IDataRecord.GetInt16
            Dim value As String = __currentLine.Column(i)
            Return Scripting.CTypeDynamic(Of Short)(value)
        End Function

        Public Function GetInt32(i As Integer) As Integer Implements IDataRecord.GetInt32
            Dim value As String = __currentLine.Column(i)
            Return Scripting.CTypeDynamic(Of Integer)(value)
        End Function

        Public Function GetInt64(i As Integer) As Long Implements IDataRecord.GetInt64
            Dim value As String = __currentLine.Column(i)
            Return Scripting.CTypeDynamic(Of Long)(value)
        End Function

        Public Function GetFloat(i As Integer) As Single Implements IDataRecord.GetFloat
            Dim value As String = __currentLine.Column(i)
            Return Scripting.CTypeDynamic(Of Single)(value)
        End Function

        Public Function GetDouble(i As Integer) As Double Implements IDataRecord.GetDouble
            Dim value As String = __currentLine.Column(i)
            Return Scripting.CTypeDynamic(Of Double)(value)
        End Function

        Public Function GetString(i As Integer) As String Implements IDataRecord.GetString
            Dim value As String = __currentLine.Column(i)
            Return value
        End Function

        Public Function GetDecimal(i As Integer) As Decimal Implements IDataRecord.GetDecimal
            Dim value As String = __currentLine.Column(i)
            Return Scripting.CTypeDynamic(Of Decimal)(value)
        End Function

        Public Function GetDateTime(i As Integer) As Date Implements IDataRecord.GetDateTime
            Dim value As String = __currentLine.Column(i)
            Return Scripting.CTypeDynamic(Of Date)(value)
        End Function

        Public Function GetData(i As Integer) As IDataReader Implements IDataRecord.GetData
            Return Me
        End Function

        Public Function IsDBNull(i As Integer) As Boolean Implements IDataRecord.IsDBNull
            Return String.IsNullOrEmpty(__currentLine.Column(i))
        End Function
    End Class
End Namespace