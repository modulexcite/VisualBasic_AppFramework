#Region "GNU Lesser General Public License"
'
'This file is part of DotFuzzy.
'
'DotFuzzy is free software: you can redistribute it and/or modify
'it under the terms of the GNU Lesser General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.
'
'DotFuzzy is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU Lesser General Public License for more details.
'
'You should have received a copy of the GNU Lesser General Public License
'along with DotFuzzy.  If not, see <http://www.gnu.org/licenses/>.
'

#End Region

Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Text

Namespace Logical.FuzzyLogic

    ''' <summary>
    ''' Represents a collection of membership functions.
    ''' </summary>
    Public Class MembershipFunctionCollection
        Inherits List(Of MembershipFunction)

        Sub New()
            Call MyBase.New
        End Sub

#Region "Public Methods"

        ''' <summary>
        ''' Finds a membership function in a collection.
        ''' </summary>
        ''' <param name="membershipFunctionName">Membership function name.</param>
        ''' <returns>The membership function, if founded.</returns>
        Public Overloads Function Find(membershipFunctionName As String) As MembershipFunction
            Dim membershipFunction As MembershipFunction = Nothing

            For Each [function] As MembershipFunction In Me
                If [function].Name = membershipFunctionName Then
                    membershipFunction = [function]
                    Exit For
                End If
            Next

            If membershipFunction Is Nothing Then
                Throw New Exception("MembershipFunction not found: " & membershipFunctionName)
            Else
                Return membershipFunction
            End If
        End Function

#End Region
    End Class
End Namespace