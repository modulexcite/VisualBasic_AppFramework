﻿Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports System.Collections
Imports System.Windows.Forms
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices
Imports System.Runtime.InteropServices.Marshal
Imports System.IO

Namespace Imaging

    Public Class TiffWriter : Implements IEnumerable(Of Image)

        Dim _imageLayers As List(Of Image)

        Sub New(ParamArray Image As Image())
            _imageLayers = Image.ToList
        End Sub

#Region "Implements Generic.IEnumerable(Of Image)"

        Public Sub Add(ParamArray images As Image())
            Call _imageLayers.AddRange(images)
        End Sub

        Public Iterator Function GetEnumerator() As IEnumerator(Of Image) Implements IEnumerable(Of Image).GetEnumerator
            For Each image As Image In _imageLayers
                Yield image
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
#End Region

        Public Function MultipageTiffSave(path As String) As Boolean
            If _imageLayers.Count = 0 Then
                Return False
            End If

            Dim Res = SaveMultipage(_imageLayers, path, "TIFF")
            Return Res
        End Function

        Public Function ExistingFileSave(path As String) As Boolean
            If _imageLayers.IsNullOrEmpty Then
                Return False
            End If

            Dim Res = SaveToExistingFile(path, __bitmaps(_imageLayers), "TIFF")
            Return Res
        End Function

        Public Shared Function SaveMultipage(bmp As List(Of Image), location As String, type As String) As Boolean
            If bmp Is Nothing Then Return False

            Try
                Call __saveMultipage(__bitmaps(bmp), location, type)
                Return True
            Catch ex As Exception
                ex = New Exception(location.ToFileURL & " ===> " & type, ex)
                Call App.LogException(ex)
            End Try

            Return False
        End Function

        Private Shared Function __bitmaps(bmp As Generic.IEnumerable(Of Image)) As Image()
            Return (From image As Image
                In bmp
                    Where Not image Is Nothing
                    Select DirectCast(ConvertToBitonal(DirectCast(image, Bitmap)), Image)).ToArray
        End Function

        Private Shared Sub __saveMultipage(bmp As Image(), location As String, type As String)
            Dim codecInfo As ImageCodecInfo = GetCodec(type)

            If bmp.Count = 1 Then

                Dim iparams As New EncoderParameters(1)
                Dim iparam As Encoder = Encoder.Compression
                Dim iparamPara As New EncoderParameter(iparam, CLng(EncoderValue.CompressionCCITT4))
                iparams.Param(0) = iparamPara

                Call bmp(0).Save(location, codecInfo, iparams)

            ElseIf bmp.Count > 1 Then

                Dim saveEncoder As Encoder = Encoder.SaveFlag
                Dim compressionEncoder As Encoder = Encoder.Compression
                Dim SaveEncodeParam As EncoderParameter
                Dim CompressionEncodeParam As EncoderParameter
                Dim EncoderParams As New EncoderParameters(2)

                ' Save the first page (frame).
                SaveEncodeParam = New EncoderParameter(saveEncoder, CLng(EncoderValue.MultiFrame))
                CompressionEncodeParam = New EncoderParameter(compressionEncoder, CLng(EncoderValue.CompressionCCITT4))
                EncoderParams.Param(0) = CompressionEncodeParam
                EncoderParams.Param(1) = SaveEncodeParam

                Call IO.File.Delete(location)
                Call bmp(0).Save(location, codecInfo, EncoderParams)

                For i As Integer = 1 To bmp.Count - 1
                    SaveEncodeParam = New EncoderParameter(saveEncoder, CLng(EncoderValue.FrameDimensionPage))
                    CompressionEncodeParam = New EncoderParameter(compressionEncoder, CLng(EncoderValue.CompressionCCITT4))
                    EncoderParams.Param(0) = CompressionEncodeParam
                    EncoderParams.Param(1) = SaveEncodeParam

                    Call bmp(0).SaveAdd(bmp(i), EncoderParams)
                Next

                SaveEncodeParam = New EncoderParameter(saveEncoder, CLng(EncoderValue.Flush))
                EncoderParams.Param(0) = SaveEncodeParam
                Call bmp(0).SaveAdd(EncoderParams)
            End If
        End Sub

        Public Shared Function GetCodec(type As String) As ImageCodecInfo
            Dim info As ImageCodecInfo() = ImageCodecInfo.GetImageEncoders()

            For i As Integer = 0 To info.Length - 1
                Dim EnumName As String = type.ToString()
                If info(i).FormatDescription.Equals(EnumName) Then
                    Return info(i)
                End If
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' This function can save newly scanned images on existing single page or multipage file
        ''' </summary>
        ''' <param name="fileName"></param>
        ''' <param name="bmp"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Shared Function SaveToExistingFile(fileName As String, bmp As Image(), type As String) As Boolean
            Try
                Call __saveToExistingFile(fileName, bmp, type)
                Return True
            Catch ex As Exception
                Call App.LogException(ex)
                Return False
            End Try
        End Function

        Private Shared Sub __saveToExistingFile(fileName As String, bmp As Image(), type As String)
            'bmp[0] is containing Image from Existing file on which we will append newly scanned Images
            'SO first we will dicide wheter existing file is single page or multipage
            Dim fr As FileStream = IO.File.Open(fileName, FileMode.Open, FileAccess.ReadWrite)
            Dim origionalFile As Image = Image.FromStream(fr)
            Dim PageNumber As Integer = __getPageNumber(origionalFile)

            If PageNumber > 1 Then        'Existing File is multi page tiff file
                __saveImageExistingMultiplePage(bmp, origionalFile, type, PageNumber, "shreeTemp.tif")
            ElseIf PageNumber = 1 Then                    'Existing file is single page file
                __saveImageExistingSinglePage(bmp, origionalFile, type, "shreeTemp.tif")
            End If

            Call fr.Flush()
            Call fr.Close()

            Call IO.File.Replace("shreeTemp.tif", fileName, "Backup.tif", True)
        End Sub

        Private Shared Sub __saveImageExistingSinglePage(bmp As Image(), origionalFile As Image, type As String, location As String)
            Dim codecInfo As ImageCodecInfo = GetCodec(type)
            Dim saveEncoder As Encoder
            Dim compressionEncoder As Encoder
            Dim SaveEncodeParam As EncoderParameter
            Dim CompressionEncodeParam As EncoderParameter
            Dim EncoderParams As New EncoderParameters(2)

            saveEncoder = Encoder.SaveFlag
            compressionEncoder = Encoder.Compression
            SaveEncodeParam = New EncoderParameter(saveEncoder, CLng(EncoderValue.MultiFrame))
            CompressionEncodeParam = New EncoderParameter(compressionEncoder, CLng(EncoderValue.CompressionCCITT4))
            EncoderParams.Param(0) = CompressionEncodeParam
            EncoderParams.Param(1) = SaveEncodeParam
            origionalFile = ConvertToBitonal(DirectCast(origionalFile, Bitmap))
            origionalFile.Save(location, codecInfo, EncoderParams)

            For i As Integer = 0 To bmp.Count - 1
                SaveEncodeParam = New EncoderParameter(saveEncoder, CLng(EncoderValue.FrameDimensionPage))
                CompressionEncodeParam = New EncoderParameter(compressionEncoder, CLng(EncoderValue.CompressionCCITT4))
                EncoderParams.Param(0) = CompressionEncodeParam
                EncoderParams.Param(1) = SaveEncodeParam

                origionalFile.SaveAdd(bmp(i), EncoderParams)
            Next

            SaveEncodeParam = New EncoderParameter(saveEncoder, CLng(EncoderValue.Flush))
            EncoderParams.Param(0) = SaveEncodeParam
            origionalFile.SaveAdd(EncoderParams)
        End Sub

        Private Shared Sub __saveImageExistingMultiplePage(bmp As Image(), origionalFile As Image, type As String, PageNumber As Integer, location As String)
            Dim codecInfo As ImageCodecInfo = GetCodec(type)
            Dim saveEncoder As Encoder
            Dim compressionEncoder As Encoder
            Dim SaveEncodeParam As EncoderParameter
            Dim CompressionEncodeParam As EncoderParameter
            Dim EncoderParams As New EncoderParameters(2)
            Dim pages As Bitmap
            Dim NextPage As Bitmap

            saveEncoder = Encoder.SaveFlag
            compressionEncoder = Encoder.Compression

            origionalFile.SelectActiveFrame(FrameDimension.Page, 0)
            pages = New Bitmap(origionalFile)
            pages = ConvertToBitonal(pages)

            ' Save the first page (frame).
            SaveEncodeParam = New EncoderParameter(saveEncoder, CLng(EncoderValue.MultiFrame))
            CompressionEncodeParam = New EncoderParameter(compressionEncoder, CLng(EncoderValue.CompressionCCITT4))
            EncoderParams.Param(0) = CompressionEncodeParam
            EncoderParams.Param(1) = SaveEncodeParam

            pages.Save(location, codecInfo, EncoderParams)


            For i As Integer = 1 To PageNumber - 1
                SaveEncodeParam = New EncoderParameter(saveEncoder, CLng(EncoderValue.FrameDimensionPage))
                CompressionEncodeParam = New EncoderParameter(compressionEncoder, CLng(EncoderValue.CompressionCCITT4))
                EncoderParams.Param(0) = CompressionEncodeParam
                EncoderParams.Param(1) = SaveEncodeParam

                origionalFile.SelectActiveFrame(FrameDimension.Page, i)
                NextPage = New Bitmap(origionalFile)
                NextPage = ConvertToBitonal(NextPage)

                pages.SaveAdd(NextPage, EncoderParams)
            Next

            For i As Integer = 0 To bmp.Count - 1
                SaveEncodeParam = New EncoderParameter(saveEncoder, CLng(EncoderValue.FrameDimensionPage))
                CompressionEncodeParam = New EncoderParameter(compressionEncoder, CLng(EncoderValue.CompressionCCITT4))
                EncoderParams.Param(0) = CompressionEncodeParam
                EncoderParams.Param(1) = SaveEncodeParam
                bmp(i) = DirectCast(ConvertToBitonal(DirectCast(bmp(i), Bitmap)), Bitmap)

                pages.SaveAdd(bmp(i), EncoderParams)
            Next

            SaveEncodeParam = New EncoderParameter(saveEncoder, CLng(EncoderValue.Flush))
            EncoderParams.Param(0) = SaveEncodeParam
            pages.SaveAdd(EncoderParams)
        End Sub

        Private Shared Function __getPageNumber(img As Image) As Integer
            Dim objGuid As Guid = img.FrameDimensionsList(0)
            Dim objDimension As New FrameDimension(objGuid)
            'Gets the total number of frames in the .tiff file
            Dim PageNumber As Integer = img.GetFrameCount(objDimension)
            Return PageNumber
        End Function

        Public Shared Function ConvertToBitonal(original As Bitmap) As Bitmap
            Dim source As Bitmap = Nothing

            ' If original bitmap is not already in 32 BPP, ARGB format, then convert
            If original.PixelFormat <> PixelFormat.Format32bppArgb Then
                source = New Bitmap(original.Width, original.Height, PixelFormat.Format32bppArgb)
                source.SetResolution(original.HorizontalResolution, original.VerticalResolution)
                Using g As Graphics = Graphics.FromImage(source)
                    g.DrawImageUnscaled(original, 0, 0)
                End Using
            Else
                source = original
            End If

            ' Lock source bitmap in memory
            Dim sourceData As BitmapData = source.LockBits(New Rectangle(0, 0, source.Width, source.Height), ImageLockMode.[ReadOnly], PixelFormat.Format32bppArgb)

            ' Copy image data to binary array
            Dim imageSize As Integer = sourceData.Stride * sourceData.Height
            Dim sourceBuffer As Byte() = New Byte(imageSize - 1) {}
            Copy(sourceData.Scan0, sourceBuffer, 0, imageSize)

            ' Unlock source bitmap
            source.UnlockBits(sourceData)

            ' Create destination bitmap
            Dim destination As New Bitmap(source.Width, source.Height, PixelFormat.Format1bppIndexed)

            ' Lock destination bitmap in memory
            Dim destinationData As BitmapData = destination.LockBits(New Rectangle(0, 0, destination.Width, destination.Height), ImageLockMode.[WriteOnly], PixelFormat.Format1bppIndexed)

            ' Create destination buffer
            imageSize = destinationData.Stride * destinationData.Height
            Dim destinationBuffer As Byte() = New Byte(imageSize - 1) {}

            Dim sourceIndex As Integer = 0
            Dim destinationIndex As Integer = 0
            Dim pixelTotal As Integer = 0
            Dim destinationValue As Byte = 0
            Dim pixelValue As Integer = 128
            Dim height As Integer = source.Height
            Dim width As Integer = source.Width
            Dim threshold As Integer = 500

            ' Iterate lines
            For y As Integer = 0 To height - 1
                sourceIndex = y * sourceData.Stride
                destinationIndex = y * destinationData.Stride
                destinationValue = 0
                pixelValue = 128

                ' Iterate pixels
                For x As Integer = 0 To width - 1
                    ' Compute pixel brightness (i.e. total of Red, Green, and Blue values)
                    pixelTotal = CInt(sourceBuffer(sourceIndex + 1)) + CInt(sourceBuffer(sourceIndex + 2)) + CInt(sourceBuffer(sourceIndex + 3))
                    If pixelTotal > threshold Then
                        destinationValue += CByte(pixelValue)
                    End If
                    If pixelValue = 1 Then
                        destinationBuffer(destinationIndex) = destinationValue
                        destinationIndex += 1
                        destinationValue = 0
                        pixelValue = 128
                    Else
                        pixelValue >>= 1
                    End If
                    sourceIndex += 4
                Next
                If pixelValue <> 128 Then
                    destinationBuffer(destinationIndex) = destinationValue
                End If
            Next

            ' Copy binary image data to destination bitmap
            Copy(destinationBuffer, 0, destinationData.Scan0, imageSize)

            ' Unlock destination bitmap
            destination.UnlockBits(destinationData)

            Return destination
        End Function
    End Class
End Namespace