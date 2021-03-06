' AForge Neural Net Library
' AForge.NET framework
'
' Copyright © Andrew Kirillov, 2005-2008
' andrew.kirillov@gmail.com
'

Namespace NeuralNetwork.IFuncs

    ''' <summary>
    ''' Activation function interface.
    ''' </summary>
    ''' 
    ''' <remarks>All activation functions, which are supposed to be used with
    ''' neurons, which calculate their output as a function of weighted sum of
    ''' their inputs, should implement this interfaces.
    ''' </remarks>
    ''' 
    Public Interface IActivationFunction

        ''' <summary>
        ''' Calculates function value.
        ''' </summary>
        '''
        ''' <param name="x">Function input value.</param>
        ''' 
        ''' <returns>Function output value, <i>f(x)</i>.</returns>
        '''
        ''' <remarks>The method calculates function value at point <paramref name="x"/>.</remarks>
        '''
        Function [Function](x As Double) As Double

        ''' <summary>
        ''' Calculates function derivative.
        ''' </summary>
        ''' 
        ''' <param name="x">Function input value.</param>
        ''' 
        ''' <returns>Function derivative, <i>f'(x)</i>.</returns>
        ''' 
        ''' <remarks>The method calculates function derivative at point <paramref name="x"/>.</remarks>
        '''
        Function Derivative(x As Double) As Double

        ''' <summary>
        ''' Calculates function derivative.
        ''' </summary>
        ''' 
        ''' <param name="y">Function output value - the value, which was obtained
        ''' with the help of "Function" method.</param>
        ''' 
        ''' <returns>Function derivative, <i>f'(x)</i>.</returns>
        ''' 
        ''' <remarks><para>The method calculates the same derivative value as the
        ''' <see cref="Derivative"/> method, but it takes not the input <b>x</b> value
        ''' itself, but the function value, which was calculated previously with
        ''' the help of "Function" method.</para>
        ''' 
        ''' <para><note>Some applications require as function value, as derivative value,
        ''' so they can save the amount of calculations using this method to calculate derivative.</note></para>
        ''' </remarks>
        ''' 
        Function Derivative2(y As Double) As Double
    End Interface
End Namespace
