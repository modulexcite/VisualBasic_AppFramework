
Namespace Kernel.GeneticAlgorithm

    ''' <summary>
    ''' Most cells do math operations... they are all controlled by this class.
    ''' </summary>
    Public NotInheritable Class BinaryNeuron
        Implements INeuron

        Public Shared Function Add(neuron1 As INeuron, neuron2 As INeuron) As BinaryNeuron
            Return New BinaryNeuron(Function(a, b) a + b, " + ", neuron1, neuron2)
        End Function
        Public Shared Function Subtract(neuron1 As INeuron, neuron2 As INeuron) As BinaryNeuron
            Return New BinaryNeuron(Function(a, b) a - b, " - ", neuron1, neuron2)
        End Function
        Public Shared Function Multiply(neuron1 As INeuron, neuron2 As INeuron) As BinaryNeuron
            Return New BinaryNeuron(Function(a, b) a * b, " * ", neuron1, neuron2)
        End Function

        Private Shared ReadOnly _powerDelegate As Func(Of Integer, Integer, System.Nullable(Of Integer)) = AddressOf _Power
        Private Shared Function _Power(a As Integer, b As Integer) As System.Nullable(Of Integer)
            Dim doubleResult As Double = System.Math.Pow(a, b)
            Dim intResult As Integer = CInt(System.Math.Truncate(doubleResult))
            If intResult <> doubleResult Then
                Return Nothing
            End If

            Return intResult
        End Function
        Public Shared Function Power(neuron1 As INeuron, neuron2 As INeuron) As BinaryNeuron
            Return New BinaryNeuron(_powerDelegate, " power ", neuron1, neuron2)
        End Function

        Private Shared ReadOnly _divideDelegate As Func(Of Integer, Integer, System.Nullable(Of Integer)) = AddressOf _Divide
        Private Shared Function _Divide(a As Integer, b As Integer) As System.Nullable(Of Integer)
            If b = 0 Then
                Return Nothing
            End If

            Return a \ b
        End Function
        Public Shared Function Divide(neuron1 As INeuron, neuron2 As INeuron) As BinaryNeuron
            Return New BinaryNeuron(_divideDelegate, " / ", neuron1, neuron2)
        End Function

        Private Shared ReadOnly _modDelegate As Func(Of Integer, Integer, System.Nullable(Of Integer)) = AddressOf _Mod
        Private Shared Function _Mod(a As Integer, b As Integer) As System.Nullable(Of Integer)
            If b = 0 Then
                Return Nothing
            End If

            Return a Mod b
        End Function
        Public Shared Function [Mod](neuron1 As INeuron, neuron2 As INeuron) As BinaryNeuron
            Return New BinaryNeuron(_modDelegate, " mod ", neuron1, neuron2)
        End Function

        Private ReadOnly _function As Func(Of Integer, Integer, System.Nullable(Of Integer))
        Private ReadOnly _operatorName As String
        Private ReadOnly _neuron1 As INeuron
        Private ReadOnly _neuron2 As INeuron
        Public Sub New([function] As Func(Of Integer, Integer, System.Nullable(Of Integer)), operatorName As String, neuron1 As INeuron, neuron2 As INeuron)
            _function = [function]
            _operatorName = operatorName
            _neuron1 = neuron1
            _neuron2 = neuron2
            _Complexity = neuron1.Complexity + neuron2.Complexity
        End Sub

        Private _Complexity As Integer

        Public Function Execute(input As Integer) As System.Nullable(Of Integer) Implements INeuron.Execute
            Dim value1 As Integer? = _neuron1.Execute(input)
            If Not value1.HasValue Then
                Return Nothing
            End If

            Dim value2 As Integer? = _neuron2.Execute(input)
            If Not value2.HasValue Then
                Return Nothing
            End If

            Try
                Return _function(value1.Value, value2.Value)
            Catch
                Return Nothing
            End Try
        End Function

        Public Function Reproduce() As INeuron Implements INeuron.Reproduce
            Dim neuron1 As INeuron = _neuron1.Reproduce()
            Dim neuron2 As INeuron = _neuron2.Reproduce()

            If GetRandom(100) <> 0 Then
                If neuron1 Is _neuron1 AndAlso neuron2 Is _neuron2 Then
                    Return Me
                End If

                Return New BinaryNeuron(_function, _operatorName, neuron1, neuron2)
            End If

            Return _ReproduceNeuron_Random(Me, neuron1, neuron2)
        End Function

        Public Overrides Function ToString() As String
            Return "(" & _neuron1.ToString() & _operatorName & _neuron2.ToString() & ")"
        End Function

        Public ReadOnly Property Complexity As Integer Implements INeuron.Complexity
            Get
                Return _Complexity
            End Get
        End Property
    End Class
End Namespace