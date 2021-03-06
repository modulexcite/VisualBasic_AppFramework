﻿Imports System.Runtime.CompilerServices

Namespace CodeDOM_VBC

    Public Module CodeDOMExpressions

        ''' <summary>
        ''' Public Shared Function Main(Argvs As String()) As Integer
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property EntryPoint As CodeDom.CodeMemberMethod
            Get
                Dim Func = New System.CodeDom.CodeMemberMethod With {
                    .Name = "Main",
                    .ReturnType = Type(Of Integer)(),
                    .Attributes = System.CodeDom.MemberAttributes.Public Or System.CodeDom.MemberAttributes.Static
                }

                Func.Parameters.Add(Argument(Of String())("Argvs"))

                Return Func
            End Get
        End Property

        ''' <summary>
        ''' 声明一个函数
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="args"></param>
        ''' <param name="returns"></param>
        ''' <param name="control"></param>
        ''' <returns></returns>
        Public Function DeclareFunc(name As String, args As Dictionary(Of String, Type), returns As Type,
                                    Optional control As CodeDom.MemberAttributes =
                                    CodeDom.MemberAttributes.Public Or
                                    CodeDom.MemberAttributes.Static) As CodeDom.CodeMemberMethod
            Dim Func As New CodeDom.CodeMemberMethod With {
                .Name = name,
                .ReturnType = returns.TypeRef,
                .Attributes = control
            }

            If Not args.IsNullOrEmpty Then
                For Each x In args
                    Call Func.Parameters.Add(Argument(x.Key, x.Value))
                Next
            End If

            Return Func
        End Function

        Public Function Field(Name As String, Type As Type) As CodeDom.CodeMemberField
            Return New CodeDom.CodeMemberField(name:=Name, type:=New CodeDom.CodeTypeReference(Type))
        End Function

        ''' <summary>
        ''' Reference of Me.Field
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <returns></returns>
        Public Function FieldRef(Name As String) As CodeDom.CodeFieldReferenceExpression
            Return New CodeDom.CodeFieldReferenceExpression(New CodeDom.CodeThisReferenceExpression, Name)
        End Function

        Public Function Field(Name As String, type As String) As CodeDom.CodeFieldReferenceExpression
            Return New CodeDom.CodeFieldReferenceExpression(New CodeDom.CodeTypeReferenceExpression(type), Name)
        End Function

        Public Function Comments(text As String) As CodeDom.CodeCommentStatement
            Return New CodeDom.CodeCommentStatement(text)
        End Function

        ''' <summary>
        ''' Class object instance constructor
        ''' </summary>
        ''' <param name="Type"></param>
        ''' <param name="parameters"></param>
        ''' <returns></returns>
        Public Function [New](Type As Type, parameters As CodeDom.CodeExpression()) As CodeDom.CodeObjectCreateExpression
            Return New CodeDom.CodeObjectCreateExpression(New CodeDom.CodeTypeReference(Type), parameters)
        End Function

        Public Function [New](type As String) As CodeDom.CodeObjectCreateExpression
            Dim typeRef As New CodeDom.CodeTypeReference(type)
            Return New CodeDom.CodeObjectCreateExpression(typeRef, {})
        End Function

        ''' <summary>
        ''' Class object instance constructor.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="parameters"></param>
        ''' <returns></returns>
        Public Function [New](Of T As Class)(parameters As Object()) As CodeDom.CodeObjectCreateExpression
            If parameters.IsNullOrEmpty Then
                Return [New](GetType(T), {})
            Else
                Return [New](GetType(T), (From obj In parameters Select New CodeDom.CodePrimitiveExpression(obj)).ToArray)
            End If
        End Function

        Public Function [New](typeRef As String, parameters As CodeDom.CodeExpression()) As CodeDom.CodeObjectCreateExpression
            Dim objectType = New CodeDom.CodeTypeReference(typeRef)
            If parameters Is Nothing Then
                parameters = New CodeDom.CodeExpression() {}
            End If

            Return New CodeDom.CodeObjectCreateExpression(objectType, parameters)
        End Function

        ''' <summary>
        ''' 声明一个局部变量
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <param name="Type"></param>
        ''' <param name="initExpression"></param>
        ''' <returns></returns>
        Public Function LocalsInit(Name As String, Type As System.Type, Optional initExpression As CodeDom.CodeExpression = Nothing) As CodeDom.CodeVariableDeclarationStatement
            Dim Expr = New CodeDom.CodeVariableDeclarationStatement(New CodeDom.CodeTypeReference(Type), Name)
            If Not initExpression Is Nothing Then
                Expr.InitExpression = initExpression
            End If
            Return Expr
        End Function

        Public Function LocalsInit(Name As String, Type As String, Optional initExpression As CodeDom.CodeExpression = Nothing) As CodeDom.CodeVariableDeclarationStatement
            Dim typeRef As New CodeDom.CodeTypeReference(Type)
            Dim Expr = New CodeDom.CodeVariableDeclarationStatement(typeRef, Name)
            If Not initExpression Is Nothing Then
                Expr.InitExpression = initExpression
            End If
            Return Expr
        End Function

        ''' <summary>
        ''' Declare a local variable.
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <param name="Type"></param>
        ''' <param name="init"></param>
        ''' <returns></returns>
        Public Function LocalsInit(Name As String, Type As System.Type, Optional init As Object = Nothing) As CodeDom.CodeVariableDeclarationStatement
            If Not init Is Nothing Then
                Return LocalsInit(Name, Type, New CodeDom.CodePrimitiveExpression(init))
            Else
                Return LocalsInit(Name, Type, initExpression:=Nothing)
            End If
        End Function

        Public Function Cast(obj As CodeDom.CodeExpression, type As Type) As CodeDom.CodeCastExpression
            Return New CodeDom.CodeCastExpression(New CodeDom.CodeTypeReference(type), obj)
        End Function

        Public Function [Call](Method As System.Reflection.MethodInfo,
                               Parameters As CodeDom.CodeExpression(),
                               Optional obj As CodeDom.CodeExpression = Nothing) As CodeDom.CodeMethodInvokeExpression
            Return [Call](If(obj Is Nothing, New CodeDom.CodeTypeReferenceExpression(Method.DeclaringType), obj), Method.Name, Parameters)
        End Function

        Public Function [Call](obj As CodeDom.CodeExpression, Name As String, Parameters As CodeDom.CodeExpression()) As CodeDom.CodeMethodInvokeExpression
            Dim MethodRef As New CodeDom.CodeMethodReferenceExpression(obj, Name)
            Dim Expression As New CodeDom.CodeMethodInvokeExpression(MethodRef, Parameters)
            Return Expression
        End Function

        Public Function [Call](type As Type, Name As String, Parameters As CodeDom.CodeExpression()) As CodeDom.CodeMethodInvokeExpression
            Return [Call](New CodeDom.CodeTypeReferenceExpression(type), Name, Parameters)
        End Function

        ''' <summary>
        ''' Call a statics function from a specific type with a known function name
        ''' </summary>
        ''' <param name="type"></param>
        ''' <param name="Name"></param>
        ''' <param name="parametersValue"></param>
        ''' <returns></returns>
        Public Function [Call](type As Type, Name As String, parametersValue As Object()) As CodeDom.CodeMethodInvokeExpression
            If parametersValue.IsNullOrEmpty Then
                Return [Call](type, Name, Parameters:={})
            Else
                Return [Call](type, Name, (From obj In parametersValue Select New CodeDom.CodePrimitiveExpression(obj)).ToArray)
            End If
        End Function

        Public Function [Return](variable As String) As CodeDom.CodeMethodReturnStatement
            Return New CodeDom.CodeMethodReturnStatement(New CodeDom.CodeVariableReferenceExpression(variable))
        End Function

        ''' <summary>
        ''' Returns value in a function body
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <returns></returns>
        Public Function [Return](expression As CodeDom.CodeExpression) As CodeDom.CodeMethodReturnStatement
            Return New CodeDom.CodeMethodReturnStatement(expression)
        End Function

        ''' <summary>
        ''' Reference to a statics field in the specific target type
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="Name"></param>
        ''' <returns></returns>
        Public Function Reference(obj As Type, Name As String) As CodeDom.CodeFieldReferenceExpression
            Return New CodeDom.CodeFieldReferenceExpression(New CodeDom.CodeTypeReferenceExpression(obj), Name)
        End Function

        ''' <summary>
        ''' Reference to a instance field in the specific object instance. 
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <param name="Name"></param>
        ''' <returns></returns>
        Public Function Reference(obj As CodeDom.CodeExpression, Name As String) As CodeDom.CodeFieldReferenceExpression
            Return New CodeDom.CodeFieldReferenceExpression(obj, Name)
        End Function

        Public Function ValueAssign(LeftAssigned As CodeDom.CodeExpression, value As CodeDom.CodeExpression) As CodeDom.CodeAssignStatement
            Return New CodeDom.CodeAssignStatement(LeftAssigned, value)
        End Function

        ''' <summary>
        ''' Variable value initializer
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Public Function Value(obj As Object) As CodeDom.CodePrimitiveExpression
            Return New CodeDom.CodePrimitiveExpression(obj)
        End Function

        ''' <summary>
        ''' Reference to a local variable in a function body.(引用局部变量)
        ''' </summary>
        ''' <param name="Name"></param>
        ''' <returns></returns>
        Public Function LocalVariable(Name As String) As CodeDom.CodeVariableReferenceExpression
            Return New CodeDom.CodeVariableReferenceExpression(Name)
        End Function

        ''' <summary>
        ''' Gets the element value in a array object.
        ''' </summary>
        ''' <param name="Array"></param>
        ''' <param name="index"></param>
        ''' <returns></returns>
        Public Function GetValue(Array As CodeDom.CodeExpression, index As Integer) As CodeDom.CodeArrayIndexerExpression
            Dim idx = New CodeDom.CodePrimitiveExpression(index)
            Return New CodeDom.CodeArrayIndexerExpression(Array, idx)
        End Function

        Public Function Type(Of T)() As CodeDom.CodeTypeReference
            Dim refType As Type = GetType(T)
            Return New CodeDom.CodeTypeReference(refType)
        End Function

        <Extension> Public Function TypeRef(type As Type) As CodeDom.CodeTypeReference
            Return New CodeDom.CodeTypeReference(type)
        End Function

        ''' <summary>
        ''' System.Type.GetType(TypeName)
        ''' </summary>
        ''' <param name="Type"></param>
        ''' <returns></returns>
        Public Function [GetType](Type As Type) As CodeDom.CodeMethodInvokeExpression
            Return [Call](GetType(System.Type), NameOf(System.Type.GetType), parametersValue:={Type.FullName, True, False})
        End Function

        Public Function Argument(Name As String, Type As Type) As CodeDom.CodeParameterDeclarationExpression
            Return New CodeDom.CodeParameterDeclarationExpression(New CodeDom.CodeTypeReference(Type), Name)
        End Function

        Public Function Argument(Of T)(Name As String) As CodeDom.CodeParameterDeclarationExpression
            Return Argument(Name, GetType(T))
        End Function

    End Module
End Namespace