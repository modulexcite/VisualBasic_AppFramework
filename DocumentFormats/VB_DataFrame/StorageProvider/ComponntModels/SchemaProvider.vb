﻿Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream
Imports Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream.Linq
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection.Reflector
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace StorageProvider.ComponentModels

    ''' <summary>
    ''' 从目标对象解析出来的Csv文件的结构组织数据
    ''' </summary>
    ''' <remarks></remarks>
    Public Class SchemaProvider

        ''' <summary>
        ''' 基本数据类型的列
        ''' </summary>
        ''' <remarks></remarks>
        Public Property Columns As Column()
            Get
                Return _columns
            End Get
            Set(value As Column())
                _columns = value

                If value.IsNullOrEmpty Then
                    _dictColumns = New Dictionary(Of String, Column)
                Else
                    _dictColumns = value.ToDictionary(Function(x) x.BindProperty.Name)
                End If
            End Set
        End Property
        ''' <summary>
        ''' 基本数据类型的数组形式的列
        ''' </summary>
        ''' <remarks></remarks>
        Public Property CollectionColumns As CollectionColumn()
            Get
                Return _collectionColumns
            End Get
            Set(value As CollectionColumn())
                _collectionColumns = value

                If _collectionColumns.IsNullOrEmpty Then
                    _dictCollectionColumns = New Dictionary(Of String, CollectionColumn)
                Else
                    _dictCollectionColumns = value.ToDictionary(Function(x) x.BindProperty.Name)
                End If
            End Set
        End Property
        Public Property EnumColumns As [Enum]()
            Get
                Return _enumColumns
            End Get
            Set(value As [Enum]())
                _enumColumns = value
                If _enumColumns.IsNullOrEmpty Then
                    _dictEnumColumns = New Dictionary(Of String, [Enum])
                Else
                    _dictEnumColumns = value.ToDictionary(Function(x) x.BindProperty.Name)
                End If
            End Set
        End Property
        Public Property KeyValuePairColumns As KeyValuePair()
            Get
                Return _keyMeta
            End Get
            Set(value As KeyValuePair())
                _keyMeta = value
                If value.IsNullOrEmpty Then
                    _dictKeyMeta = New Dictionary(Of String, KeyValuePair)
                Else
                    _dictKeyMeta = value.ToDictionary(Function(x) x.BindProperty.Name)
                End If
            End Set
        End Property

        ''' <summary>
        ''' 一个类型之中只可以定义一个元数据存储对象
        ''' </summary>
        ''' <returns></returns>
        Public Property MetaAttributes As MetaAttribute

        Dim _columns As Column()
        Dim _collectionColumns As CollectionColumn()
        Dim _enumColumns As [Enum]()
        Dim _keyMeta As KeyValuePair()

#Region "按照实际的属性名称进行映射"

        Dim _dictColumns As Dictionary(Of String, Column)
        Dim _dictCollectionColumns As Dictionary(Of String, CollectionColumn)
        Dim _dictEnumColumns As Dictionary(Of String, [Enum])
        Dim _dictKeyMeta As Dictionary(Of String, KeyValuePair)
#End Region

        Public ReadOnly Property HasMetaAttributes As Boolean
            Get
                Return Not MetaAttributes Is Nothing
            End Get
        End Property

        Public ReadOnly Property DeclaringType As Type

        Public ReadOnly Iterator Property Properties As IEnumerable(Of StorageProvider)
            Get
                For Each p As CollectionColumn In Me.CollectionColumns.SafeQuery
                    Yield p
                Next
                For Each p As Column In Me.Columns.SafeQuery
                    Yield p
                Next
                For Each p As [Enum] In Me.EnumColumns
                    Yield p
                Next
                For Each p As KeyValuePair In Me.KeyValuePairColumns
                    Yield p
                Next
                If Not Me.MetaAttributes Is Nothing Then
                    Yield MetaAttributes
                End If
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return DeclaringType.FullName
        End Function

        Public Function GetField(Name As String) As ComponentModels.StorageProvider
            If _dictColumns.ContainsKey(Name) Then
                Return _dictColumns(Name)
            End If
            If _dictCollectionColumns.ContainsKey(Name) Then
                Return _dictCollectionColumns(Name)
            End If
            If _dictEnumColumns.ContainsKey(Name) Then
                Return _dictEnumColumns(Name)
            End If
            If _dictKeyMeta.ContainsKey(Name) Then
                Return _dictKeyMeta(Name)
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' For write csv data file.(从目标类型对象之中可以读取这个属性的值将数据写入到文件之中)
        ''' </summary>
        ''' <returns></returns>
        Public Function CopyReadDataFromObject() As SchemaProvider
            Return New SchemaProvider With {
                .CollectionColumns = (From p In CollectionColumns Where p.CanReadDataFromObject Select p).ToArray,
                .Columns = (From p In Columns Where p.CanReadDataFromObject Select p).ToArray,
                .EnumColumns = (From p In EnumColumns Where p.CanReadDataFromObject Select p).ToArray,
                .KeyValuePairColumns = (From p In KeyValuePairColumns Where p.CanReadDataFromObject Select p).ToArray,
                .MetaAttributes = If(MetaAttributes IsNot Nothing AndAlso MetaAttributes.CanReadDataFromObject, MetaAttributes, Nothing)
            }
        End Function

        ''' <summary>
        ''' For create object instance.(可以在读取Csv文件之中的数据之后将数据写入到这个属性之中从而将数据加载进入内存之中)
        ''' </summary>
        ''' <returns></returns>
        Public Function CopyWriteDataToObject() As SchemaProvider
            Return New SchemaProvider With {
                .CollectionColumns = (From p In CollectionColumns Where p.CanWriteDataToObject Select p).ToArray,
                .Columns = (From p In Columns Where p.CanWriteDataToObject Select p).ToArray,
                .EnumColumns = (From p In EnumColumns Where p.CanWriteDataToObject Select p).ToArray,
                .KeyValuePairColumns = (From p In KeyValuePairColumns Where p.CanWriteDataToObject Select p).ToArray,
                .MetaAttributes = If(MetaAttributes IsNot Nothing AndAlso MetaAttributes.CanWriteDataToObject, MetaAttributes, Nothing)
            }
        End Function

        Public Function CacheOrdinal(Df As DataFrame) As SchemaProvider
            Return CacheOrdinal(AddressOf Df.GetOrdinal)
        End Function

        Public Function CacheOrdinal(GetOrdinal As GetOrdinal) As SchemaProvider
            For Each Column As StorageProvider In Columns
                Column.Ordinal = GetOrdinal(Column.Name)
            Next
            For Each Column As StorageProvider In CollectionColumns
                Column.Ordinal = GetOrdinal(Column.Name)
            Next
            For Each Column As StorageProvider In EnumColumns
                Column.Ordinal = GetOrdinal(Column.Name)
            Next
            For Each Column As StorageProvider In KeyValuePairColumns
                Column.Ordinal = GetOrdinal(Column.Name)
            Next

            Return Me
        End Function

        ''' <summary>
        ''' 从域名称来判断
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <returns></returns>
        Public Function ContainsField(Name As String) As Boolean
            Dim LQuery As StorageProvider =
                LinqAPI.DefaultFirst(Of Column) <= From p As Column
                                                   In Columns
                                                   Where String.Equals(Name, p.Name)
                                                   Select p
            If Not LQuery Is Nothing Then
                Return True
            End If
            LQuery =
                LinqAPI.DefaultFirst(Of CollectionColumn) <= From p As CollectionColumn
                                                             In Me.CollectionColumns
                                                             Where String.Equals(Name, p.Name)
                                                             Select p
            Return Not LQuery Is Nothing
        End Function

        ''' <summary>
        ''' 从所绑定的属性来判断
        ''' </summary>
        ''' <param name="[Property]"></param>
        ''' <returns></returns>
        ''' <remarks>这个函数还需要进行一些绑定的映射</remarks>
        Public Function ContainsProperty([Property] As PropertyInfo) As Boolean
            Dim LQuery As StorageProvider =
                LinqAPI.DefaultFirst(Of Column) <= From p As Column
                                                   In Columns
                                                   Where [Property].Equals(p.BindProperty)
                                                   Select p
            If Not LQuery Is Nothing Then
                Return True
            End If
            LQuery =
                LinqAPI.DefaultFirst(Of CollectionColumn) <= From p As CollectionColumn
                                                             In Me.CollectionColumns
                                                             Where [Property].Equals(p.BindProperty)
                                                             Select p
            Return Not LQuery Is Nothing
        End Function

        ''' <summary>
        ''' Creates the data frame schema for the specific object type.
        ''' </summary>
        ''' <param name="type"></param>
        ''' <param name="Explicit"></param>
        ''' <returns></returns>
        Public Shared Function CreateObject(type As Type, Optional Explicit As Boolean = False) As SchemaProvider
            Dim Properties As Dictionary(Of PropertyInfo, StorageProvider) =
                TypeSchemaProvider.GetProperties(type, Explicit)

            Dim Schema As New SchemaProvider With {
                .Columns = GetColumns(Properties),
                .CollectionColumns = GetCollectionColumns(Properties),
                .EnumColumns = GetEnumColumns(Properties),
                .MetaAttributes = GetMetaAttributeColumn(Properties),
                .KeyValuePairColumns = GetKeyValuePairColumn(Properties),
                ._DeclaringType = type
            }

            Return Schema
        End Function

        Public Shared Function CreateObject(Of T As Class)(Explicit As Boolean) As SchemaProvider
            Dim Type As Type = GetType(T)
            Return CreateObject(Type, Explicit)
        End Function

        Private Shared Function GetKeyValuePairColumn(Properties As Dictionary(Of PropertyInfo, StorageProvider)) As KeyValuePair()
            Return __gets(Of KeyValuePair)(Properties, Function(type) type = ProviderIds.KeyValuePair)
        End Function

        Private Shared Function __gets(Of T As StorageProvider)(
                                 Properties As Dictionary(Of PropertyInfo, StorageProvider),
                                 ProviderId As Func(Of Reflection.ProviderIds, Boolean)) As T()
            Dim LQuery As T() =
                LinqAPI.Exec(Of T) <= From [Property] As StorageProvider
                                      In Properties.Values.AsParallel
                                      Where ProviderId([Property].ProviderId) = True
                                      Select DirectCast([Property], T)
            Return LQuery
        End Function

        Const DynamicsNotFound As String = "Explicit option is set TRUE, but could not found Meta attribute for the dynamics property!"

        Public Sub New()
        End Sub

        ''' <summary>
        ''' 对于<see cref="DynamicPropertyBase"/>的继承对象类型，也会自动解析出来的，假若<see cref="MetaAttribute"/>没有被定义的话
        ''' </summary>
        ''' <param name="Properties"></param>
        ''' <returns></returns>
        Private Shared Function GetMetaAttributeColumn(Properties As Dictionary(Of PropertyInfo, StorageProvider)) As MetaAttribute
            Dim MetaAttributes As MetaAttribute =
                __gets(Of MetaAttribute)(Properties, Function(type) type = ProviderIds.MetaAttribute).FirstOrDefault

            If MetaAttributes Is Nothing Then
                Dim prop As PropertyInfo = Properties.Keys.FirstOrDefault

                If prop Is Nothing Then
                    Throw New Exception(DynamicsNotFound)
                End If

                Dim type As Type = prop.DeclaringType

                If type.IsInheritsFrom(GetType(DynamicPropertyBase(Of ))) Then
                    type = type.BaseType
                    Dim metaProp As PropertyInfo =
                        type.GetProperty(NameOf(DynamicPropertyBase(Of Double).Properties),
                                         BindingFlags.Public Or BindingFlags.Instance)
                    type = type.GetGenericArguments.First
                    MetaAttributes = New MetaAttribute(New Reflection.MetaAttribute(type), metaProp)
                End If
            End If

            Return MetaAttributes
        End Function

        Private Shared Function GetEnumColumns(Properties As Dictionary(Of PropertyInfo, ComponentModels.StorageProvider)) As [Enum]()
            Return __gets(Of [Enum])(Properties, Function(type) type = ProviderIds.Enum)
        End Function

        Private Shared Function GetCollectionColumns(Properties As Dictionary(Of PropertyInfo, StorageProvider)) As CollectionColumn()
            Return __gets(Of CollectionColumn)(Properties, Function(type) type = ProviderIds.CollectionColumn)
        End Function

        Private Shared Function GetColumns(Properties As Dictionary(Of PropertyInfo, StorageProvider)) As Column()
            Return __gets(Of Column)(Properties,
                                     Function(type) type = Reflection.ProviderIds.Column OrElse type = Reflection.ProviderIds.NullMask)
        End Function
    End Class
End Namespace