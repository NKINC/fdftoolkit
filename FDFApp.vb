Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Web
Imports System.Diagnostics.Process
Imports System.Data.OleDb
Imports System.Data
Imports System.Xml
Imports System.Text.Encoder
Imports System.Security
Imports System.Security.Permissions
Namespace FDFApp
    Public Class FDFApp_Class
        Implements IDisposable
        Private FDFDox As New FDFDoc_Class
        Private _FDFErrors As New FDFErrors
        Private _FDFMIME As String = "application/vnd.fdf"
        Private _PDFMIME As String = "application/pdf"
        Private _HTMMIME As String = "text/html"
        Private _TXTMIME As String = "text/plain"
        Private _XMLMIME As String = "text/xml"
        Private _XFDFMIME As String = "application/vnd.adobe.xfdf"
        Private _XDPMIME As String = "application/vnd.adobe.xdp+xml"
        Private _defaultEncoding As Encoding = Encoding.UTF8
        Public Property ThrowErrors() As Boolean
            Get
                Return _FDFErrors.ThrowErrors
            End Get
            Set(ByVal value As Boolean)
                _FDFErrors.ThrowErrors = value
            End Set
        End Property
        Public Enum FieldType
            FldTextual = 1
            FldMultiSelect = 3
            FldOption = 5
        End Enum
        Public Enum FDFType
            FDF = 1
            xFDF = 2
            XML = 3
            PDF = 4
            XDP = 5
            XPDF = 6
            XFA = 10
        End Enum
        Private Property FDFErrors() As FDFErrors
            Get
                Return _FDFErrors
            End Get
            Set(ByVal Value As FDFErrors)
                _FDFErrors = Value
            End Set
        End Property
        Public Function FDFHasErrors() As Boolean
            Return _FDFErrors.FDFHasErrors
        End Function
        ''' <summary>
        ''' MimeTxt is the string variable representing the TEXT MIME Type for Response.ContentType objects
        ''' </summary>
        ''' <value></value>
        ''' <returns>text/plain</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property MimeTXT() As String
            Get
                Return _TXTMIME
            End Get
        End Property
        ''' <summary>
        ''' MimeXDP is the string variable representing the XDP Mime Type for Response.ContentType objects
        ''' </summary>
        ''' <value></value>
        ''' <returns>application/vnd.adobe.xdp+xml</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property MimeXDP() As String
            Get
                Return _XDPMIME
            End Get
        End Property
        ''' <summary>
        ''' MimeHTML is the string variable representing the HTML MIME Type for Response.ContentType objects
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property MimeHTML() As String
            Get
                Return _HTMMIME
            End Get
        End Property
        ''' <summary>
        ''' MimeXML is the string variable representing the XML MIME Type for Response.ContentType objects
        ''' </summary>
        ''' <value></value>
        ''' <returns>text/xml</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property MimeXML() As String
            Get
                Return _XMLMIME
            End Get
        End Property
        ''' <summary>
        ''' MimeFDF is the string variable representing the FDF MIME Type for Response.ContentType objects
        ''' </summary>
        ''' <value></value>
        ''' <returns>application/vnd.fdf</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property MimeFDF() As String
            Get
                Return _FDFMIME
            End Get
        End Property
        ''' <summary>
        ''' MimeXFDF is the string variable representing the XFDF MIME Type for Response.ContentType objects
        ''' </summary>
        ''' <value></value>
        ''' <returns>application/vnd.adobe.xfdf</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property MimeXFDF() As String
            Get
                Return _XFDFMIME
            End Get
        End Property
        ''' <summary>
        ''' MimePDF is the string variable representing the PDF MIME Type for Response.ContentType objects
        ''' </summary>
        ''' <value></value>
        ''' <returns>application/pdf</returns>
        ''' <remarks></remarks>
        Public ReadOnly Property MimePDF() As String
            Get
                Return _PDFMIME
            End Get
        End Property
        ''' <summary>
        ''' ResetErrors resets all errors in FDFApp.FDFApp_Class object
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub ResetErrors()
            _FDFErrors.ResetErrors()
        End Sub
        ''' <summary>
        ''' FDFAppErrors() Returns an array of FDFError objects
        ''' </summary>
        ''' <value></value>
        ''' <returns>FDFAppError Object Array</returns>
        ''' <remarks></remarks>
        Public Property FDFAppErrors() As FDFErrors
            Get
                Return _FDFErrors
            End Get
            Set(ByVal Value As FDFErrors)
                _FDFErrors = Value
            End Set
        End Property
        ''' <summary>
        ''' FDFAppErrorStr returns FDFAppErrors in string format, Optional HTML Format
        ''' </summary>
        ''' <param name="HTMLFormat">Set to true to return with HTML line breaks</param>
        ''' <returns>String with FDFAppError Array content</returns>
        ''' <remarks></remarks>
        Public Function FDFAppErrorsStr(Optional ByVal HTMLFormat As Boolean = False) As String
            Dim FDFErrors As FDFErrors
            Dim FDFError As FDFErrors.FDFError
            FDFErrors = _FDFErrors
            Dim retString As String
            retString = CStr(IIf(HTMLFormat, "<br>", vbNewLine)) & "FDFApp Errors:"
            If FDFErrors.FDFErrors Is Nothing Then Return ""
            If FDFErrors.FDFErrors.Length <= 0 Then Return ""
            For Each FDFError In FDFErrors.FDFErrors
                retString = retString & CStr(IIf(HTMLFormat, "<br>", vbNewLine)) & vbTab & "Error: " & FDFError.FDFError_Code & " - " & FDFError.FDFError & CStr(IIf(HTMLFormat, "<br>", vbNewLine)) & vbTab & "#: " & FDFError.FDFError_Number & CStr(IIf(HTMLFormat, "<br>", vbNewLine)) & vbTab & "Module: " & FDFError.FDFError_Module & CStr(IIf(HTMLFormat, "<br>", vbNewLine)) & vbTab & "Message: " & FDFError.FDFError_Msg & CStr(IIf(HTMLFormat, "<br>", vbNewLine))
            Next
            Return retString
        End Function
        ''' <summary>
        ''' Determines what format the data is in.
        ''' </summary>
        ''' <param name="PDFData">PDF Data (String) or PDF URL ir PDF Local File Path</param>
        ''' <returns>FDFType (XML/xFDF/XDP/PDF=Acrobat/XPDF=LiveCycle)</returns>
        ''' <remarks>EDITED: 2011-06-27</remarks>
        Public Function Determine_Type(ByVal PDFData As String) As FDFType
            FDFDox.DefaultEncoding = _defaultEncoding
            Dim PDFData2 As String = PDFData
            Dim PDFFileName As String = ""
            Dim bytes() As Byte = Nothing
            Try
                If IsValidUrl(PDFData) Then
                    Dim client As New WebClient
                    PDFFileName = PDFData
                    Dim wClient As New Net.WebClient
                    Dim strPDF As New MemoryStream
                    bytes = wClient.DownloadData(PDFData)
                    PDFData = _defaultEncoding.GetString(bytes)
                ElseIf File.Exists(PDFData) Then
                    PDFFileName = PDFData
                    Dim FS As New FileStream(PDFData, FileMode.Open, FileAccess.Read, FileShare.Read)
                    Dim reader As StreamReader = New StreamReader(FS)
                    ReDim bytes(CInt(FS.Length))
                    reader.BaseStream.Read(bytes, 0, CInt(reader.BaseStream.Length))
                    FS.Close()
                    PDFData = _defaultEncoding.GetString(bytes)
                End If
            Catch ex As Exception
                PDFData = PDFData2
            End Try
            If PDFData.StartsWith("%FDF") Then
                Return FDFType.FDF
            ElseIf PDFData.StartsWith("%PDF") Then
                Try
                    Dim reader As iTextSharp.text.pdf.PdfReader
                    If Not String_IsNullOrEmpty(PDFFileName) Then
                        reader = New iTextSharp.text.pdf.PdfReader(PDFFileName)
                    Else
                        reader = New iTextSharp.text.pdf.PdfReader(bytes)
                    End If
                    Dim xfaFrm As New iTextSharp.text.pdf.XfaForm(reader)
                    Dim isXFA As Boolean = False
                    isXFA = xfaFrm.XfaPresent
                    reader.Close()
                    reader = Nothing
                    xfaFrm = Nothing
                    If isXFA Then
                        If FDFDox.PDFisXFADynamic(bytes) Then
                            Return FDFType.XFA
                        Else
                            Return FDFType.XPDF
                        End If
                    Else
                        Return FDFType.PDF
                    End If
                Catch ex As Exception
                    Return FDFType.PDF
                End Try
                Return FDFType.PDF
            ElseIf InStr(PDFData, "<xdp:xdp xmlns:xdp=""http://ns.adobe.com/xdp/""") > 0 Then
                Return FDFType.XDP
            ElseIf PDFData.StartsWith("<?xml version=""1.0""") Then
                If InStrRev(PDFData, "<xfdf", -1, CompareMethod.Text) > 0 Then
                    Return FDFType.xFDF
                Else
                    Return FDFType.XML
                End If
            Else
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcBadFDF, "Error: Bad FDF/PDF or Unknown Data Type", "FDFApp.FDFType", 1)
            End If
            Return Nothing
            Exit Function
        End Function
        ''' <summary>
        ''' Determines what format the data is in.
        ''' </summary>
        ''' <param name="PDFData">PDF Data</param>
        ''' <returns>FDFType (XML/xFDF/XDP/PDF=Acrobat/XPDF=LiveCycle)</returns>
        ''' <remarks></remarks>
        Public Function Determine_Type(ByVal PDFData As Byte()) As FDFType
            Dim data As String = _defaultEncoding.GetString(PDFData)
            If data.ToString.StartsWith("%FDF") Then
                Return FDFType.FDF
            ElseIf data.ToString.StartsWith("%PDF") Then
                Try
                    Dim reader As New iTextSharp.text.pdf.PdfReader(PDFData)
                    Dim xfaFrm As New iTextSharp.text.pdf.XfaForm(reader)
                    Dim isXFA As Boolean = False
                    isXFA = xfaFrm.XfaPresent
                    reader.Close()
                    reader = Nothing
                    xfaFrm = Nothing
                    If isXFA Then
                        If FDFDox.PDFisXFADynamic(PDFData) Then
                            Return FDFType.XFA
                        Else
                            Return FDFType.XPDF
                        End If
                    Else
                        Return FDFType.PDF
                    End If
                Catch ex As Exception
                    Return FDFType.PDF
                End Try
                Return FDFType.PDF
            ElseIf InStr(data.ToString, "<xdp:xdp xmlns:xdp=""http://ns.adobe.com/xdp/""") > 0 Then
                Return FDFType.XDP
            ElseIf data.ToString.StartsWith("<?xml version=""1.0""") Then
                If InStrRev(data.ToString, "<xfdf", -1, CompareMethod.Text) > 0 Then
                    Return FDFType.xFDF
                Else
                    Return FDFType.XML
                End If
            Else
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcBadFDF, "Error: Bad FDF/PDF or Unknown Data Type", "FDFApp.FDFType", 1)
                Return FDFType.FDF
            End If
        End Function
        ''' <summary>
        ''' Determines what format the data is in.
        ''' </summary>
        ''' <param name="PDFData">PDF Data</param>
        ''' <param name="ownerPassword">OwnerPassword</param>
        ''' <returns>FDFType (XML/xFDF/XDP/PDF=Acrobat/XPDF=LiveCycle)</returns>
        ''' <remarks></remarks>
        Public Function Determine_Type(ByVal PDFData As Byte(), ByVal ownerPassword As String) As FDFType
            Dim data As String = _defaultEncoding.GetString(PDFData)
            If data.ToString.StartsWith("%FDF") Then
                Return FDFType.FDF
            ElseIf data.ToString.StartsWith("%PDF") Then
                Try
                    Dim reader As iTextSharp.text.pdf.PdfReader
                    If Not String_IsNullOrEmpty(ownerPassword) Then
                        reader = New iTextSharp.text.pdf.PdfReader(PDFData, _defaultEncoding.GetBytes(ownerPassword))
                    Else
                        reader = New iTextSharp.text.pdf.PdfReader(PDFData)
                    End If
                    Dim xfaFrm As New iTextSharp.text.pdf.XfaForm(reader)
                    Dim isXFA As Boolean = False
                    isXFA = xfaFrm.XfaPresent
                    reader.Close()
                    reader = Nothing
                    xfaFrm = Nothing
                    If isXFA Then
                        If FDFDox.PDFisXFADynamic(PDFData) Then
                            Return FDFType.XFA
                        Else
                            Return FDFType.XPDF
                        End If
                    Else
                        Return FDFType.PDF
                    End If
                Catch ex As Exception
                    Return FDFType.PDF
                End Try
                Return FDFType.PDF
            ElseIf InStr(data.ToString, "<xdp:xdp xmlns:xdp=""http://ns.adobe.com/xdp/""") > 0 Then
                Return FDFType.XDP
            ElseIf data.ToString.StartsWith("<?xml version=""1.0""") Then
                If InStrRev(data.ToString, "<xfdf", -1, CompareMethod.Text) > 0 Then
                    Return FDFType.xFDF
                Else
                    Return FDFType.XML
                End If
            Else
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcBadFDF, "Error: Bad FDF/PDF or Unknown Data Type", "FDFApp.FDFType", 1)
                Return FDFType.FDF
            End If
        End Function
        ''' <summary>
        ''' Determines what format the data is in.
        ''' </summary>
        ''' <param name="PDFData">PDF Data</param>
        ''' <returns>FDFType (XML/xFDF/XDP/PDF=Acrobat/XPDF=LiveCycle)</returns>
        ''' <remarks></remarks>
        Public Function Determine_Type(ByVal PDFData As Stream) As FDFType
            Dim Data As String = ReadStream_New(PDFData, False)
            If Data.StartsWith("%FDF") Then
                Return FDFType.FDF
            ElseIf Data.StartsWith("%PDF") Then
                Try
                    Dim reader As New iTextSharp.text.pdf.PdfReader(PDFData)
                    Dim xfaFrm As New iTextSharp.text.pdf.XfaForm(reader)
                    Dim isXFA As Boolean = False
                    isXFA = xfaFrm.XfaPresent
                    reader.Close()
                    reader = Nothing
                    xfaFrm = Nothing
                    If isXFA Then
                        If FDFDox.PDFisXFADynamic(PDFData) Then
                            Return FDFType.XFA
                        Else
                            Return FDFType.XPDF
                        End If
                    Else
                        Return FDFType.PDF
                    End If
                Catch ex As Exception
                    Return FDFType.PDF
                End Try
                Return FDFType.PDF
            ElseIf InStr(Data, "<xdp:xdp xmlns:xdp=""http://ns.adobe.com/xdp/""") > 0 Then
                Return FDFType.XDP
            ElseIf Data.StartsWith("<?xml version=""1.0""") Then
                If InStrRev(Data, "<xfdf", -1, CompareMethod.Text) > 0 Then
                    Return FDFType.xFDF
                Else
                    Return FDFType.XML
                End If
            Else
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcBadFDF, "Error: Bad FDF/PDF or Unknown Data Type", "FDFApp.FDFType", 1)
                Return FDFType.FDF
            End If
        End Function
        Public Property DefaultEncoding() As Encoding
            Get
                Return _defaultEncoding
            End Get
            Set(ByVal value As Encoding)
                _defaultEncoding = value
            End Set
        End Property
#Region "OPEN DOCUMENT"
        ''' <summary>
        ''' FDFOpenFromStream opens an FDF Document from a Stream Object
        ''' </summary>
        ''' <param name="varFDFData">FDF Data Stream to parse</param>
        ''' <param name="FDFInitialize">Initialize FDFDoc Object</param>
        ''' <param name="AppendSaves">Appends Saves</param>
        ''' <returns>FDFDoc_Class</returns>
        ''' <remarks></remarks>
        Public Function FDFOpenFromStream(ByVal varFDFData As Stream, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal AppendSaves As Boolean = True) As FDFDoc_Class
            Try
                FDFDox.DefaultEncoding = _defaultEncoding
                If varFDFData.CanSeek Then
                    varFDFData.Position = 0
                End If
                Dim bytesPDF(CInt(varFDFData.Length)) As Byte
                varFDFData.Read(bytesPDF, 0, bytesPDF.Length)
                Dim rawString As String = _defaultEncoding.GetString(bytesPDF)
                Dim eFDFType As FDFType
                eFDFType = Determine_Type(bytesPDF)
                FDFDox.PDFData = bytesPDF
                Select Case eFDFType
                    Case FDFType.FDF
                        Return FDFOpenWithiText(bytesPDF, FDFInitialize, AppendSaves)
                    Case FDFType.xFDF
                        FDFDox.FDFData = rawString '_defaultEncoding.GetString(FDFDox.PDFData)
                        Return parseXFDF(rawString, FDFInitialize)
                    Case FDFType.XML
                        FDFDox.FDFData = rawString
                        Return parseXML(rawString, "", FDFInitialize)
                    Case FDFType.PDF
                        Return parsePDF(bytesPDF, FDFInitialize)
                    Case FDFType.XPDF
                        Return parseXFA(bytesPDF, FDFInitialize)
                    Case FDFType.XDP
                        FDFDox.XDPData = rawString '_defaultEncoding.GetString(FDFDox.PDFData)
                        Return parseXDP(rawString, FDFInitialize)
                End Select
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.FDFOpenFromBuf", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' FDFOpenFromBuf opens an FDF Document from a Byte Array or Buffer
        ''' </summary>
        ''' <param name="varFDFData">FDF Data Byte Array to parse</param>
        ''' <param name="FDFInitialize">Initialize FDFDoc Object</param>
        ''' <param name="AppendSaves">Appends Saves</param>
        ''' <returns>FDFDoc_Class</returns>
        ''' <remarks></remarks>
        Public Function FDFOpenFromBuf(ByVal varFDFData() As Byte, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal AppendSaves As Boolean = True) As FDFDoc_Class
            Try
                FDFDox.DefaultEncoding = _defaultEncoding
                Dim FDFData As String = ReadBytes(varFDFData, False)
                Dim eFDFType As FDFType
                eFDFType = Determine_Type(FDFData)
                Select Case eFDFType
                    Case FDFType.FDF
                        Return parseFDFi(varFDFData, FDFInitialize, AppendSaves)
                    Case FDFType.xFDF
                        Return parseXFDF(FDFData, FDFInitialize)
                    Case FDFType.XML
                        Return parseXML(FDFData, "", FDFInitialize)
                    Case FDFType.PDF
                        Return parsePDF(varFDFData, FDFInitialize)
                    Case FDFType.XPDF
                        Return parseXFA(varFDFData, FDFInitialize)
                    Case FDFType.XDP
                        FDFDox.XDPData = FDFData
                        Return parseXDP(FDFData, FDFInitialize)
                End Select
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.FDFOpenFromBuf", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' PDFOpenFromBuf opens an PDF Document from a Byte Array or Buffer
        ''' </summary>
        ''' <param name="varPDFData">PDF Data Byte Array to parse</param>
        ''' <param name="FDFInitialize">Initialize FDFDoc Object</param>
        ''' <param name="AppendSaves">Appends Saves</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>FDFDoc_Class</returns>
        ''' <remarks></remarks>
        Public Function PDFOpenFromBuf(ByVal varPDFData() As Byte, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal AppendSaves As Boolean = True, Optional ByVal ownerPassword As String = "") As FDFDoc_Class
            Try
                FDFDox.DefaultEncoding = _defaultEncoding
                Dim FDFData As String = ReadBytes(varPDFData, False)
                Dim eFDFType As FDFType
                eFDFType = Determine_Type(varPDFData, ownerPassword)
                Select Case eFDFType
                    Case FDFType.FDF
                        Return FDFOpenWithiText(FDFData, FDFInitialize, AppendSaves)
                    Case FDFType.xFDF
                        Return parseXFDF(FDFData, FDFInitialize)
                    Case FDFType.XML
                        Return parseXML(FDFData, "", FDFInitialize)
                    Case FDFType.PDF
                        Return parsePDF(varPDFData, FDFInitialize, ownerPassword)
                    Case FDFType.XPDF
                        Return parseXFA(varPDFData, FDFInitialize, ownerPassword)
                    Case FDFType.XDP
                        FDFDox.XDPData = FDFData
                        Return parseXDP(FDFData, FDFInitialize)
                End Select
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.FDFOpenFromBuf", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' PDFOpenFromStream opens an PDF Document from a Stream
        ''' </summary>
        ''' <param name="varPDFData">PDF Stream to parse</param>
        ''' <param name="FDFInitialize">Initialize FDFDoc Object</param>
        ''' <param name="AppendSaves">Appends Saves</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>FDFDoc_Class</returns>
        ''' <remarks></remarks>
        Public Function PDFOpenFromStream(ByVal varPDFData As Stream, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal AppendSaves As Boolean = True, Optional ByVal ownerPassword As String = "") As FDFDoc_Class
            Try
                FDFDox.DefaultEncoding = _defaultEncoding
                If varPDFData.CanSeek Then
                    varPDFData.Position = 0
                End If
                ReDim FDFDox.PDFData(CInt(varPDFData.Length))
                varPDFData.Read(FDFDox.PDFData, 0, CInt(varPDFData.Length))
                Dim eFDFType As FDFType
                eFDFType = Determine_Type(FDFDox.PDFData, ownerPassword)
                Select Case eFDFType
                    Case FDFType.FDF
                        Return FDFOpenWithiText(FDFDox.PDFData, FDFInitialize, AppendSaves)
                    Case FDFType.xFDF
                        FDFDox.FDFData = _defaultEncoding.GetString(FDFDox.PDFData)
                        Return parseXFDF(FDFDox.FDFData, FDFInitialize)
                    Case FDFType.XML
                        FDFDox.FDFData = _defaultEncoding.GetString(FDFDox.PDFData)
                        Return parseXML(FDFDox.FDFData, "", FDFInitialize)
                    Case FDFType.PDF
                        Return parsePDF(FDFDox.PDFData, FDFInitialize, ownerPassword)
                    Case FDFType.XPDF
                        Return parseXFA(FDFDox.PDFData, FDFInitialize, ownerPassword)
                    Case FDFType.XDP
                        FDFDox.XDPData = _defaultEncoding.GetString(FDFDox.PDFData)
                        Return parseXDP(FDFDox.XDPData, FDFInitialize)
                End Select
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.FDFOpenFromBuf", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' FDFOpenFromStr opens an FDF Document from a String variable
        ''' </summary>
        ''' <param name="bstrFDFData">FDF Data String to parse</param>
        ''' <param name="FDFInitialize">Initialize FDFDoc Object</param>
        ''' <param name="AppendSaves">Appends Saves</param>
        ''' <returns>FDFDoc_Class</returns>
        ''' <remarks></remarks>
        Public Function FDFOpenFromStr(ByVal bstrFDFData As String, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal AppendSaves As Boolean = True) As FDFDoc_Class
            Try
                Dim FDFData As String = bstrFDFData
                Dim eFDFType As FDFType
                FDFDox.DefaultEncoding = _defaultEncoding
                eFDFType = Determine_Type(FDFData)
                Select Case eFDFType
                    Case FDFType.FDF
                        Return parseFDFi(_defaultEncoding.GetBytes(FDFData), FDFInitialize, AppendSaves)
                    Case FDFType.xFDF
                        Return parseXFDF(FDFData, FDFInitialize)
                    Case FDFType.XML
                        Return parseXML(FDFData, "", FDFInitialize)
                    Case FDFType.PDF
                        Return parsePDF(_defaultEncoding.GetBytes(FDFData), FDFInitialize)
                    Case FDFType.XPDF
                        Return parseXFA(_defaultEncoding.GetBytes(FDFData), FDFInitialize)
                    Case FDFType.XDP
                        FDFDox.XDPData = FDFData
                        Return parseXDP(FDFData, FDFInitialize)
                End Select
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.FDFOpenFromStr", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' FDFOpenFromFile opens an FDF Document from a File Location or URL
        ''' </summary>
        ''' <param name="bstrFileName">FDF Data File or URL to parse</param>
        ''' <param name="FDFInitialize">Initialize FDFDoc Object</param>
        ''' <param name="AppendSaves">Appends Saves</param>
        ''' <returns>FDFDoc_Class</returns>
        ''' <remarks></remarks>
        Public Function FDFOpenFromFile(ByVal bstrFileName As String, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal AppendSaves As Boolean = True) As FDFDoc_Class
            Try
                FDFDox.DefaultEncoding = _defaultEncoding
                Dim FDFData As String
                Dim bytes() As Byte = Nothing
                If IsValidUrl(bstrFileName) Then
                    Dim client As New WebClient
                    bytes = GetUsedBytesOnly(client.DownloadData(bstrFileName))
                    FDFData = _defaultEncoding.GetString(bytes)
                ElseIf File.Exists(bstrFileName) Then
                    Dim input As New StreamReader(bstrFileName, _defaultEncoding)
                    FDFData = input.ReadToEnd
                    bytes = _defaultEncoding.GetBytes(FDFData)
                Else
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: File Does Not Exist", "FDFApp.FDFOpenFromFile", 1)
                    Return Nothing
                    Exit Function
                End If
                FDFDox.DefaultEncoding = _defaultEncoding
                Dim eFDFType As FDFType
                eFDFType = Determine_Type(FDFData)
                Select Case eFDFType
                    Case FDFType.FDF
                        Return parseFDFi(bytes, FDFInitialize, AppendSaves)
                    Case FDFType.xFDF
                        Return parseXFDF(FDFData, FDFInitialize)
                    Case FDFType.XML
                        Return parseXML(FDFData, bstrFileName, FDFInitialize)
                    Case FDFType.PDF
                        Return parsePDF(bytes, FDFInitialize)
                    Case FDFType.XPDF
                        Return parseXFA(bytes, FDFInitialize)
                    Case FDFType.XDP
                        FDFDox.XDPData = FDFData
                        Return parseXDP(FDFData, FDFInitialize)
                End Select
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.FDFOpenFromFile", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
#Region "Edited 2010-09-28"
        ''' <summary>
        ''' FDFOpenFromStream opens an FDF Document from a Stream Object
        ''' </summary>
        ''' <param name="varFDFData">FDF Data Stream to parse</param>
        ''' <param name="FDFInitialize">Initialize FDFDoc Object</param>
        ''' <param name="AppendSaves">Appends Saves</param>
        ''' <returns>FDFDoc_Class</returns>
        ''' <remarks></remarks>
        Public Function FDFOpen(ByVal varFDFData As Stream, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal AppendSaves As Boolean = True) As FDFDoc_Class
            Try
                FDFDox.DefaultEncoding = _defaultEncoding
                If varFDFData.CanSeek Then
                    varFDFData.Position = 0
                End If
                ReDim FDFDox.PDFData(CInt(varFDFData.Length))
                varFDFData.Read(FDFDox.PDFData, 0, CInt(varFDFData.Length))
                Dim eFDFType As FDFType
                eFDFType = Determine_Type(FDFDox.PDFData)
                Select Case eFDFType
                    Case FDFType.FDF
                        Return FDFOpenWithiText(FDFDox.PDFData, FDFInitialize, AppendSaves)
                    Case FDFType.xFDF
                        FDFDox.FDFData = _defaultEncoding.GetString(FDFDox.PDFData)
                        Return parseXFDF(FDFDox.FDFData, FDFInitialize)
                    Case FDFType.XML
                        FDFDox.FDFData = _defaultEncoding.GetString(FDFDox.PDFData)
                        Return parseXML(FDFDox.FDFData, "", FDFInitialize)
                    Case FDFType.PDF
                        Return parsePDF(FDFDox.PDFData, FDFInitialize)
                    Case FDFType.XPDF
                        Return parseXFA(FDFDox.PDFData, FDFInitialize)
                    Case FDFType.XDP
                        FDFDox.XDPData = _defaultEncoding.GetString(FDFDox.PDFData)
                        Return parseXDP(FDFDox.XDPData, FDFInitialize)
                End Select
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.FDFOpenFromBuf", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' FDFOpenFromBuf opens an FDF Document from a Byte Array or Buffer
        ''' </summary>
        ''' <param name="varFDFData">FDF Data Byte Array to parse</param>
        ''' <param name="FDFInitialize">Initialize FDFDoc Object</param>
        ''' <param name="AppendSaves">Appends Saves</param>
        ''' <returns>FDFDoc_Class</returns>
        ''' <remarks></remarks>
        Public Function FDFOpen(ByVal varFDFData() As Byte, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal AppendSaves As Boolean = True) As FDFDoc_Class
            Try
                FDFDox.DefaultEncoding = _defaultEncoding
                Dim FDFData As String = ReadBytes(varFDFData, False)
                Dim eFDFType As FDFType
                eFDFType = Determine_Type(FDFData)
                Select Case eFDFType
                    Case FDFType.FDF
                        Return FDFOpenWithiText(FDFData, FDFInitialize, AppendSaves)
                    Case FDFType.xFDF
                        Return parseXFDF(FDFData, FDFInitialize)
                    Case FDFType.XML
                        Return parseXML(FDFData, "", FDFInitialize)
                    Case FDFType.PDF
                        Return parsePDF(varFDFData, FDFInitialize)
                    Case FDFType.XPDF
                        Return parseXFA(varFDFData, FDFInitialize)
                    Case FDFType.XDP
                        FDFDox.XDPData = FDFData
                        Return parseXDP(FDFData, FDFInitialize)
                End Select
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.FDFOpenFromBuf", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' FDFOpenFromFile opens an FDF Document from a File Location or URL
        ''' </summary>
        ''' <param name="bstrFileName">FDF Data File or URL to parse</param>
        ''' <param name="FDFInitialize">Initialize FDFDoc Object</param>
        ''' <param name="AppendSaves">Appends Saves</param>
        ''' <returns>FDFDoc_Class</returns>
        ''' <remarks></remarks>
        Public Function FDFOpen(ByVal bstrFileName As String, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal AppendSaves As Boolean = True) As FDFDoc_Class
            Try
                FDFDox.DefaultEncoding = _defaultEncoding
                Dim FDFData As String
                Dim bytes() As Byte = Nothing
                If IsValidUrl(bstrFileName) Then
                    Dim client As New WebClient
                    bytes = GetUsedBytesOnly(client.DownloadData(bstrFileName))
                    FDFData = _defaultEncoding.GetString(bytes)
                ElseIf File.Exists(bstrFileName) Then
                    Dim input As New StreamReader(bstrFileName, _defaultEncoding)
                    FDFData = input.ReadToEnd
                    bytes = _defaultEncoding.GetBytes(FDFData)
                Else
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: File Does Not Exist", "FDFApp.FDFOpenFromFile", 1)
                    Return Nothing
                    Exit Function
                End If
                FDFDox.DefaultEncoding = _defaultEncoding
                Dim eFDFType As FDFType
                eFDFType = Determine_Type(FDFData)
                Select Case eFDFType
                    Case FDFType.FDF
                        Return FDFOpenWithiText(FDFData, FDFInitialize, AppendSaves)
                    Case FDFType.xFDF
                        Return parseXFDF(FDFData, FDFInitialize)
                    Case FDFType.XML
                        Return parseXML(FDFData, bstrFileName, FDFInitialize)
                    Case FDFType.PDF
                        Return parsePDF(bytes, FDFInitialize)
                    Case FDFType.XPDF
                        Return parseXFA(bytes, FDFInitialize)
                    Case FDFType.XDP
                        FDFDox.XDPData = FDFData
                        Return parseXDP(FDFData, FDFInitialize)
                End Select
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.FDFOpenFromFile", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
#End Region
        ''' <summary>
        ''' OPEN FILE WITH iText
        ''' </summary>
        ''' <param name="strFilePathOrURL">FDF URL, or Path String</param>
        ''' <param name="FDFInitialize">Intialize FDFDoc</param>
        ''' <param name="AppendSaves"></param>
        ''' <returns>FDFDoc</returns>
        ''' <remarks></remarks>
        Public Function FDFOpenFromFileiText(ByVal strFilePathOrURL As String, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal AppendSaves As Boolean = True) As FDFDoc_Class
            Try
                Try
                    Select Case Determine_Type(strFilePathOrURL)
                        Case FDFType.FDF
                            Return parseFDFiB(_defaultEncoding.GetString(GetUsedBytesOnly(strFilePathOrURL)), True, True)
                        Case FDFType.XDP
                            Return parseXDP(_defaultEncoding.GetString(GetUsedBytesOnly(strFilePathOrURL)), True)
                        Case FDFType.XML
                            Return parseXML(_defaultEncoding.GetString(GetUsedBytesOnly(strFilePathOrURL)), True)
                        Case FDFType.xFDF
                            Return parseXFDF(_defaultEncoding.GetString(GetUsedBytesOnly(strFilePathOrURL)), True)
                        Case FDFType.PDF, FDFType.XPDF
                            Return parsePDF((GetUsedBytesOnly(strFilePathOrURL)), True, "")
                        Case FDFType.XFA
                            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: File (XFA-Dynamic) not supported.", "FDFApp.FDFOpenFromFileiText", 1)
                            Return Nothing
                            Exit Function
                        Case Else
                            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: File not supported.", "FDFApp.FDFOpenFromFileiText", 1)
                            Return Nothing
                            Exit Function
                    End Select
                    Return Nothing
                Catch ex As Exception
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.FDFOpenFromFileiText", 1)
                    Return Nothing
                    Exit Function
                End Try
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.FDFOpenFromFileiText", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' Parse FDF with iText
        ''' </summary>
        ''' <param name="strUrl">String URL or Path of FDF</param>
        ''' <param name="FDFInitialize">Initialize FDFDoc</param>
        ''' <param name="AppendSaves"></param>
        ''' <returns>FDFDoc</returns>
        ''' <remarks></remarks>
        Public Function FDFOpenWithiText(ByVal strUrl As String, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal AppendSaves As Boolean = True) As FDFDoc_Class
            Try
                Select Case Determine_Type(strUrl)
                    Case FDFType.FDF
                        Return parseFDFiB(_defaultEncoding.GetString(GetUsedBytesOnly(strUrl)), True, True)
                    Case FDFType.XDP
                        Return parseXDP(_defaultEncoding.GetString(GetUsedBytesOnly(strUrl)), True)
                    Case FDFType.XML
                        Return parseXML(_defaultEncoding.GetString(GetUsedBytesOnly(strUrl)), True)
                    Case FDFType.xFDF
                        Return parseXFDF(_defaultEncoding.GetString(GetUsedBytesOnly(strUrl)), True)
                    Case FDFType.PDF, FDFType.XPDF
                        Return parsePDF((GetUsedBytesOnly(strUrl)), True, "")
                    Case FDFType.XFA
                        _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: File (XFA-Dynamic) not supported.", "FDFApp.FDFOpenWithiText", 1)
                        Return Nothing
                        Exit Function
                    Case Else
                        _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: File not supported.", "FDFApp.FDFOpenWithiText", 1)
                        Return Nothing
                        Exit Function
                End Select
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.FDFOpenFromFile", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        Public Function FDFOpenWithiText(ByVal strData() As Byte, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal AppendSaves As Boolean = True) As FDFDoc_Class
            Try
                If strData.Length > 0 Then
                    Select Case Determine_Type(strData)
                        Case FDFType.FDF
                            Return parseFDFiB(_defaultEncoding.GetString(strData), True, True)
                        Case FDFType.XDP
                            Return parseXDP(_defaultEncoding.GetString(strData), True)
                        Case FDFType.XML
                            Return parseXML(_defaultEncoding.GetString(strData), True)
                        Case FDFType.xFDF
                            Return parseXFDF(_defaultEncoding.GetString(strData), True)
                        Case FDFType.PDF, FDFType.XPDF
                            Return parsePDF(strData, True, "")
                        Case FDFType.XFA
                            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: File (XFA-Dynamic) not supported.", "FDFApp.FDFOpenWithiText", 1)
                            Return Nothing
                            Exit Function
                        Case Else
                            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: File not supported.", "FDFApp.FDFOpenWithiText", 1)
                            Return Nothing
                            Exit Function
                    End Select
                Else
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: File Does Not Exist", "FDFApp.FDFOpenFromFile", 1)
                    Return Nothing
                    Exit Function
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.FDFOpenFromFile", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        Public Function FDFOpenWithiText(ByVal strData As Stream, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal AppendSaves As Boolean = True) As FDFDoc_Class
            Try
                If strData.Length > 0 Then
                    If strData.CanSeek Then
                        strData.Position = 0
                    End If
                    Select Case Determine_Type(strData)
                        Case FDFType.FDF
                            Return parseFDFiB(_defaultEncoding.GetString(GetUsedBytesOnly(strData)), True, True)
                        Case FDFType.XDP
                            Return parseXDP(_defaultEncoding.GetString(GetUsedBytesOnly(strData)), True)
                        Case FDFType.XML
                            Return parseXML(_defaultEncoding.GetString(GetUsedBytesOnly(strData)), True)
                        Case FDFType.xFDF
                            Return parseXFDF(_defaultEncoding.GetString(GetUsedBytesOnly(strData)), True)
                        Case FDFType.PDF, FDFType.XPDF
                            Return parsePDF(strData, True, "")
                        Case FDFType.XFA
                            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: File (XFA-Dynamic) not supported.", "FDFApp.FDFOpenWithiText", 1)
                            Return Nothing
                            Exit Function
                        Case Else
                            _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: File not supported.", "FDFApp.FDFOpenWithiText", 1)
                            Return Nothing
                            Exit Function
                    End Select
                    Return Nothing
                Else
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: File Does Not Exist", "FDFApp.FDFOpenFromFile", 1)
                    Return Nothing
                    Exit Function
                End If
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.FDFOpenFromFile", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' FDFOpenFromURL opens an FDF Document from a URL
        ''' </summary>
        ''' <param name="FDFURL">FDF Document URL to parse</param>
        ''' <param name="FDFInitialize">Initialize FDFDoc Object</param>
        ''' <param name="AppendSaves">Appends Saves</param>
        ''' <returns>FDFDoc_Class</returns>
        ''' <remarks></remarks>
        Public Function FDFOpenFromURL(ByVal FDFURL As String, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal AppendSaves As Boolean = True) As FDFDoc_Class
            Try
                FDFDox.DefaultEncoding = _defaultEncoding
                Dim client As New WebClient
                Dim bytes() As Byte = client.DownloadData(FDFURL)
                Dim FDFData As String = _defaultEncoding.GetString(bytes)
                Dim eFDFType As FDFType
                eFDFType = Determine_Type(bytes)
                Select Case eFDFType
                    Case FDFType.FDF
                        Return parseFDFi(bytes, FDFInitialize, AppendSaves)
                    Case FDFType.xFDF
                        Return parseXFDF(FDFData, FDFInitialize)
                    Case FDFType.XML
                        Return parseXML(FDFData, "", FDFInitialize)
                    Case FDFType.PDF
                        Return parsePDF(bytes, FDFInitialize)
                    Case FDFType.XPDF
                        Return parseXFA(bytes, FDFInitialize)
                    Case FDFType.XDP
                        FDFDox.XDPData = FDFData
                        Return parseXDP(FDFData, FDFInitialize)
                End Select
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.FDFOpenFromURL", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
#Region "Edited 2010-09-28"
        ''' <summary>
        ''' PDFOpenFromBuf opens an PDF Document from a Byte Array or Buffer
        ''' </summary>
        ''' <param name="varPDFData">PDF Data Byte Array to parse</param>
        ''' <param name="FDFInitialize">Initialize FDFDoc Object</param>
        ''' <param name="AppendSaves">Appends Saves</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>FDFDoc_Class</returns>
        ''' <remarks></remarks>
        Public Function PDFOpen(ByVal varPDFData() As Byte, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal AppendSaves As Boolean = True, Optional ByVal ownerPassword As String = "") As FDFDoc_Class
            Try
                FDFDox.DefaultEncoding = _defaultEncoding
                Dim FDFData As String = ReadBytes(varPDFData, False)
                Dim eFDFType As FDFType
                eFDFType = Determine_Type(varPDFData, ownerPassword)
                Select Case eFDFType
                    Case FDFType.FDF
                        Return FDFOpenWithiText(FDFData, FDFInitialize, AppendSaves)
                    Case FDFType.xFDF
                        Return parseXFDF(FDFData, FDFInitialize)
                    Case FDFType.XML
                        Return parseXML(FDFData, "", FDFInitialize)
                    Case FDFType.PDF
                        Return parsePDF(varPDFData, FDFInitialize, ownerPassword)
                    Case FDFType.XPDF
                        Return parseXFA(varPDFData, FDFInitialize, ownerPassword)
                    Case FDFType.XDP
                        FDFDox.XDPData = FDFData
                        Return parseXDP(FDFData, FDFInitialize)
                End Select
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.FDFOpenFromBuf", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' PDFOpenFromStream opens an PDF Document from a Stream
        ''' </summary>
        ''' <param name="varPDFData">PDF Stream to parse</param>
        ''' <param name="FDFInitialize">Initialize FDFDoc Object</param>
        ''' <param name="AppendSaves">Appends Saves</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>FDFDoc_Class</returns>
        ''' <remarks></remarks>
        Public Function PDFOpen(ByVal varPDFData As Stream, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal AppendSaves As Boolean = True, Optional ByVal ownerPassword As String = "") As FDFDoc_Class
            Try
                FDFDox.DefaultEncoding = _defaultEncoding
                If varPDFData.CanSeek Then
                    varPDFData.Position = 0
                End If
                ReDim FDFDox.PDFData(CInt(varPDFData.Length))
                varPDFData.Read(FDFDox.PDFData, 0, CInt(varPDFData.Length))
                Dim eFDFType As FDFType
                eFDFType = Determine_Type(FDFDox.PDFData, ownerPassword)
                Select Case eFDFType
                    Case FDFType.FDF
                        Return FDFOpenWithiText(FDFDox.PDFData, FDFInitialize, AppendSaves)
                    Case FDFType.xFDF
                        FDFDox.FDFData = _defaultEncoding.GetString(FDFDox.PDFData)
                        Return parseXFDF(FDFDox.FDFData, FDFInitialize)
                    Case FDFType.XML
                        FDFDox.FDFData = _defaultEncoding.GetString(FDFDox.PDFData)
                        Return parseXML(FDFDox.FDFData, "", FDFInitialize)
                    Case FDFType.PDF
                        Return parsePDF(FDFDox.PDFData, FDFInitialize, ownerPassword)
                    Case FDFType.XPDF
                        Return parseXFA(FDFDox.PDFData, FDFInitialize, ownerPassword)
                    Case FDFType.XDP
                        FDFDox.XDPData = _defaultEncoding.GetString(FDFDox.PDFData)
                        Return parseXDP(FDFDox.XDPData, FDFInitialize)
                End Select
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.FDFOpenFromBuf", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' PDFOpenFromFile opens an PDF Document from a File Location or URL
        ''' </summary>
        ''' <param name="bstrFileName">PDF Data File or URL to parse</param>
        ''' <param name="FDFInitialize">Initialize FDFDoc Object</param>
        ''' <param name="AppendSaves">Appends Saves</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>FDFDoc_Class</returns>
        ''' <remarks></remarks>
        Public Function PDFOpen(ByVal bstrFileName As String, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal AppendSaves As Boolean = True, Optional ByVal ownerPassword As String = "") As FDFDoc_Class
            Try
                FDFDox.DefaultEncoding = _defaultEncoding
                Dim PDFFile As String
                Dim PDFData As String
                Dim bytes() As Byte = Nothing
                If IsValidUrl(bstrFileName) Then
                    Dim client As New WebClient
                    bytes = GetUsedBytesOnly(client.DownloadData(bstrFileName))
                    PDFData = _defaultEncoding.GetString(bytes)
                    FDFDox.PDFData = bytes
                ElseIf File.Exists(bstrFileName) Then
                    PDFFile = Me.OpenFile(bstrFileName)
                    Dim fInfo As New FileInfo(bstrFileName)
                    Dim numBytes As Long = fInfo.Length
                    Dim FS As New FileStream(bstrFileName, FileMode.Open, FileAccess.Read, FileShare.Read)
                    If FS.CanRead Then
                        FS.Position = 0
                    End If
                    Dim br As New BinaryReader(FS)
                    ReDim bytes(CInt(br.BaseStream.Length))
                    bytes = br.ReadBytes(CInt(br.BaseStream.Length))
                    PDFData = _defaultEncoding.GetString(bytes)
                    FDFDox.PDFData = bytes
                    FS.Close()
                Else
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: File Does Not Exist", "FDFApp.PDFOpenFromFile", 1)
                    Return Nothing
                    Exit Function
                End If
                Dim eFDFType As FDFType
                eFDFType = Me.Determine_Type(bytes)
                Select Case eFDFType
                    Case FDFType.FDF
                        Return FDFOpenWithiText(PDFData, FDFInitialize, AppendSaves)
                    Case FDFType.xFDF
                        Return parseXFDF(PDFData, FDFInitialize)
                    Case FDFType.XML
                        Return parseXML(PDFData, "", FDFInitialize)
                    Case FDFType.PDF
                        Return parsePDF(bstrFileName, FDFInitialize, ownerPassword)
                    Case FDFType.XPDF
                        Return parseXFA(bstrFileName, FDFInitialize, ownerPassword)
                    Case FDFType.XDP
                        FDFDox.XDPData = PDFData
                        Return parseXDP(PDFData, FDFInitialize)
                End Select
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.FDFOpenFromFile", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' PDFOpenFromURL opens an PDF Document from a File Location or URL
        ''' </summary>
        ''' <param name="PDFURL">PDF Data URL to parse</param>
        ''' <param name="FDFInitialize">Initialize FDFDoc Object</param>
        ''' <param name="AppendSaves">Appends Saves</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>FDFDoc_Class</returns>
        ''' <remarks></remarks>
        Public Function PDFOpen(ByVal PDFURL As System.Uri, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal AppendSaves As Boolean = True, Optional ByVal ownerPassword As String = "") As FDFDoc_Class
            Try
                FDFDox.DefaultEncoding = _defaultEncoding
                Dim PDFData As String
                Dim bytes() As Byte = Nothing
                If IsValidUrl(PDFURL.ToString) Then
                    Dim client As New WebClient
                    bytes = client.DownloadData(PDFURL.ToString)
                    PDFData = _defaultEncoding.GetString(bytes)
                    FDFDox.PDFData = bytes
                Else
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: File Does Not Exist", "FDFApp.PDFOpenFromFile", 1)
                    Return Nothing
                    Exit Function
                End If
                Dim eFDFType As FDFType
                eFDFType = Me.Determine_Type(bytes)
                Select Case eFDFType
                    Case FDFType.FDF
                        Return FDFOpenWithiText(PDFData, FDFInitialize, AppendSaves)
                    Case FDFType.xFDF
                        Return parseXFDF(PDFData, FDFInitialize)
                    Case FDFType.XML
                        Return parseXML(PDFData, "", FDFInitialize)
                    Case FDFType.PDF
                        Return parsePDF(bytes, FDFInitialize, ownerPassword)
                    Case FDFType.XPDF
                        Return parseXFA(bytes, FDFInitialize, ownerPassword)
                    Case FDFType.XDP
                        FDFDox.XDPData = PDFData
                        Return parseXDP(PDFData, FDFInitialize)
                End Select
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.FDFOpenFromStr", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
#End Region
        ''' <summary>
        ''' PDFOpenFromFile opens an PDF Document from a File Location or URL
        ''' </summary>
        ''' <param name="bstrFileName">PDF Data File or URL to parse</param>
        ''' <param name="FDFInitialize">Initialize FDFDoc Object</param>
        ''' <param name="AppendSaves">Appends Saves</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>FDFDoc_Class</returns>
        ''' <remarks></remarks>
        Public Function PDFOpenFromFile(ByVal bstrFileName As String, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal AppendSaves As Boolean = True, Optional ByVal ownerPassword As String = "") As FDFDoc_Class
            Try
                FDFDox.DefaultEncoding = _defaultEncoding
                Dim PDFFile As String
                Dim PDFData As String
                Dim bytes() As Byte = Nothing
                If IsValidUrl(bstrFileName) Then
                    FDFDox.DefaultEncoding = _defaultEncoding
                    Dim client As New WebClient
                    bytes = GetUsedBytesOnly(client.DownloadData(bstrFileName))
                ElseIf File.Exists(bstrFileName) Then
                    PDFFile = Me.OpenFile(bstrFileName)
                    Dim fInfo As New FileInfo(bstrFileName)
                    Dim numBytes As Long = fInfo.Length
                    Dim FS As New FileStream(bstrFileName, FileMode.Open, FileAccess.Read, FileShare.Read)
                    If FS.CanRead Then
                        FS.Position = 0
                    End If
                    Dim br As New BinaryReader(FS)
                    ReDim bytes(CInt(numBytes))
                    bytes = br.ReadBytes(CInt(numBytes))
                    PDFData = _defaultEncoding.GetString(bytes)
                    FS.Close()
                Else
                    Throw New Exception("Error: File Does Not Exist")
                    Return Nothing
                    Exit Function
                End If
                Dim eFDFType As FDFType
                eFDFType = Me.Determine_Type(bytes)
                Select Case eFDFType
                    Case FDFType.FDF
                        Return FDFOpenWithiText(_defaultEncoding.GetString(bytes), FDFInitialize, AppendSaves)
                    Case FDFType.xFDF
                        Return parseXFDF(_defaultEncoding.GetString(bytes), FDFInitialize)
                    Case FDFType.XML
                        Return parseXML(_defaultEncoding.GetString(bytes), "", FDFInitialize)
                    Case FDFType.PDF
                        Return parsePDF(bstrFileName, FDFInitialize, ownerPassword)
                    Case FDFType.XPDF
                        Return parseXFA(bstrFileName, FDFInitialize, ownerPassword)
                    Case FDFType.XDP
                        FDFDox.XDPData = _defaultEncoding.GetString(bytes)
                        Return parseXDP(_defaultEncoding.GetString(bytes), FDFInitialize)
                End Select
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, ex)
                Return Nothing
                Exit Function
            End Try
        End Function
        ''' <summary>
        ''' PDFOpenFromURL opens an PDF Document from a File Location or URL
        ''' </summary>
        ''' <param name="PDFURL">PDF Data URL to parse</param>
        ''' <param name="FDFInitialize">Initialize FDFDoc Object</param>
        ''' <param name="AppendSaves">Appends Saves</param>
        ''' <param name="ownerPassword">Owner password for Original password protected documents</param>
        ''' <returns>FDFDoc_Class</returns>
        ''' <remarks></remarks>
        Public Function PDFOpenFromURL(ByVal PDFURL As String, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal AppendSaves As Boolean = True, Optional ByVal ownerPassword As String = "") As FDFDoc_Class
            Try
                FDFDox.DefaultEncoding = _defaultEncoding
                Dim client As New WebClient
                Dim bytes() As Byte = GetUsedBytesOnly(client.DownloadData(PDFURL))
                Dim eFDFType As FDFType
                eFDFType = Determine_Type(bytes, ownerPassword)
                Select Case eFDFType
                    Case FDFType.FDF
                        Return FDFOpenWithiText(bytes, FDFInitialize, AppendSaves)
                    Case FDFType.xFDF
                        Return parseXFDF(_defaultEncoding.GetString(bytes), FDFInitialize)
                    Case FDFType.XML
                        Return parseXML(_defaultEncoding.GetString(bytes), "", FDFInitialize)
                    Case FDFType.PDF
                        Return parsePDF(PDFURL, FDFInitialize, ownerPassword)
                    Case FDFType.XPDF
                        Return parseXFA(PDFURL, FDFInitialize, ownerPassword)
                    Case FDFType.XDP
                        FDFDox.XDPData = _defaultEncoding.GetString(bytes)
                        Return parseXDP(_defaultEncoding.GetString(bytes), FDFInitialize)
                End Select
                Return Nothing
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.FDFOpenFromStr", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        Public Function GetUsedBytesOnly(ByRef b() As Byte) As Byte()
            Dim bytes As Byte() = b
            Dim i As Integer = 0
            For i = bytes.Length - 1 To 1 Step -1
                If bytes(i) <> 0 Then
                    Exit For
                End If
            Next
            Dim newBytes As Byte() = New Byte(i) {}
            Array.Copy(bytes, newBytes, i + 1)
            ReDim bytes(0)
            bytes = Nothing
            Return newBytes
        End Function
        Public Function GetUsedBytesOnly(ByRef s As Stream) As Byte()
            Dim bytes(CInt(s.Length)) As Byte
            If s.CanSeek Then
                s.Seek(0, SeekOrigin.Begin)
            End If
            s.Read(bytes, 0, bytes.Length)
            Return GetUsedBytesOnly(bytes)
        End Function
        Public Function GetUsedBytesOnly(ByRef s As String) As Byte()
            If IsValidUrl(s.ToString & "") Then
                Dim w As New System.Net.WebClient()
                Dim bytes() As Byte = w.DownloadData(s.ToString)
                Return GetUsedBytesOnly(bytes)
            ElseIf File.Exists(s & "") Then
                Dim bytes() As Byte = File.ReadAllBytes(s)
                Return GetUsedBytesOnly(bytes)
            ElseIf Not String.IsNullOrEmpty(s & "") Then
                Dim bytes() As Byte = _defaultEncoding.GetBytes(s & "")
                Return GetUsedBytesOnly(bytes)
            Else
                Return Nothing
            End If
        End Function
        Private Function GetUsedBytesOnly(ByRef u As Uri) As Byte()
            If IsValidUrl(u.ToString) Then
                Dim w As New System.Net.WebClient()
                Dim bytes() As Byte = w.DownloadData(u.ToString)
                Return GetUsedBytesOnly(bytes)
            Else
                Return Nothing
            End If
        End Function
#End Region
#Region "PARSING"
        Private Function parseXDP(ByVal FDF As String, Optional ByVal FDFInitialize As Boolean = False) As FDFDoc_Class
            If FDFInitialize Then FDFDox.Initialize(_defaultEncoding)
            Dim cCntr As Integer = -1
            Dim strFDF As String = ByteArrayToString(_defaultEncoding.GetBytes(FDF))
            Dim str As String = ""
            Dim PDFFileName As String = ""
            If Not InStr(strFDF, "<root>", CompareMethod.Text) > 0 Then
                Dim sb As StringBuilder = New StringBuilder
                strFDF = strFDF.Replace(Chr(10) & ">", ">")
                strFDF = strFDF.Replace(Chr(10) & "/>", "/>")
                strFDF = strFDF.Replace(Chr(13) & ">", ">")
                strFDF = strFDF.Replace(Chr(13) & "/>", "/>")
                strFDF = strFDF.Replace(Environment.NewLine & ">", ">")
                strFDF = strFDF.Replace(Environment.NewLine & "/>", "/>")
                Dim XMLMeta As String = strFDF.Substring(0, strFDF.IndexOf("<xfa:data>", 0) + 10)
                Dim XMLMetaEnd As String = strFDF.Substring(strFDF.IndexOf("</xfa:data>", 0), strFDF.Length - strFDF.IndexOf("</xfa:data>", 0))
                Dim XMLMetaFix As String = "<?xml version=""1.0"" encoding=""UTF-8""?>"
                Try
                    Dim XMLMeteEndPDF1 As Integer = strFDF.IndexOf("<pdf ", 0)
                    If XMLMeteEndPDF1 >= 0 Then
                        Dim XMLMeteEndPDF2 As Integer = strFDF.IndexOf("/>", XMLMeteEndPDF1 + 5) + 2
                        Dim XMLMetaFixEnd As String = strFDF.Substring(XMLMeteEndPDF1, XMLMeteEndPDF2 - XMLMeteEndPDF1)
                        Dim STARTINDEXOF As Integer = XMLMetaFixEnd.IndexOf("href=""", 0) + 6
                        Dim ENDINDEXOF As Integer = XMLMetaFixEnd.IndexOf("""", STARTINDEXOF + 7) + 1
                        PDFFileName = XMLMetaFixEnd.Substring(STARTINDEXOF, ENDINDEXOF - STARTINDEXOF)
                        PDFFileName = PDFFileName.TrimStart(""""c)
                        PDFFileName = PDFFileName.TrimEnd(""""c)
                        If Not PDFFileName.Length = 0 Then
                            FDFDox.FDFSetFile(PDFFileName)
                        End If
                    End If
                Catch ex As Exception
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, ex)
                    Err.Clear()
                End Try
                strFDF = strFDF.Replace(XMLMeta, "")
                strFDF = strFDF.Replace(XMLMetaEnd, "")
                sb.Append(XMLMetaFix)
                sb.Append("<root>")
                strFDF = strFDF.Replace("xfa:contentType", "contenttype")
                strFDF = strFDF.Replace("xfa:", "")
                sb.Append(strFDF)
                sb.Append("</root>")
                strFDF = sb.ToString
            End If
            Return parseXML(strFDF, False)
            Return FDFDox
        End Function
        Private Function parseXFA(ByVal FileNameorURL As String, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal ownerPassword As String = "") As FDFDoc_Class
            If FDFInitialize Then FDFDox.Initialize(_defaultEncoding)
            FDFDox.DefaultEncoding = _defaultEncoding
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(FileNameorURL)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(FileNameorURL, _defaultEncoding.GetBytes(ownerPassword))
            End If
            Dim xfaFrm As New iTextSharp.text.pdf.XfaForm(reader)
            Dim isXFA As Boolean = False
            isXFA = xfaFrm.XfaPresent
            If Not isXFA Then
                reader.Close()
                reader = Nothing
                xfaFrm = Nothing
                Return parsePDF(FileNameorURL, FDFInitialize)
                Exit Function
            End If
            Dim xmlData() As Byte = GetXFAXML(FileNameorURL)
            FDFDox = parseXDP(_defaultEncoding.GetString(xmlData), True)
            FDFDox.FDFSetFile(FileNameorURL)
            For Each fld As FDFApp.FDFDoc_Class.FDFField In FDFDox.XDPGetAllFields()
                Select Case reader.AcroFields.GetFieldType(fld.FieldName)
                    Case iTextSharp.text.pdf.AcroFields.FIELD_TYPE_TEXT
                        FDFDox.XDPGetField(fld.FieldLevelLong).FieldType = FDFDoc_Class.FieldType.FldTextual
                    Case iTextSharp.text.pdf.AcroFields.FIELD_TYPE_LIST
                        FDFDox.XDPGetField(fld.FieldLevelLong).FieldType = FDFDoc_Class.FieldType.FldMultiSelect
                    Case iTextSharp.text.pdf.AcroFields.FIELD_TYPE_CHECKBOX
                        FDFDox.XDPGetField(fld.FieldLevelLong).FieldType = FDFDoc_Class.FieldType.FldTextual
                    Case iTextSharp.text.pdf.AcroFields.FIELD_TYPE_COMBO
                        FDFDox.XDPGetField(fld.FieldLevelLong).FieldType = FDFDoc_Class.FieldType.FldTextual
                    Case iTextSharp.text.pdf.AcroFields.FIELD_TYPE_RADIOBUTTON
                        FDFDox.XDPGetField(fld.FieldLevelLong).FieldType = FDFDoc_Class.FieldType.FldOption
                    Case iTextSharp.text.pdf.AcroFields.FIELD_TYPE_PUSHBUTTON
                        FDFDox.XDPGetField(fld.FieldLevelLong).FieldType = FDFDoc_Class.FieldType.FldButton
                    Case iTextSharp.text.pdf.AcroFields.FIELD_TYPE_SIGNATURE

                    Case iTextSharp.text.pdf.AcroFields.FIELD_TYPE_NONE

                    Case Else

                End Select
            Next
ContinueProcess:
            FDFDox.FDFData = FDFDox.FDFSavetoStr(FDFDoc_Class.FDFType.FDF, True)
            FDFDox.XDPData = FDFDox.FDFSavetoStr(FDFDoc_Class.FDFType.XDP, True)
            FDFDox.FDFSetFile(FileNameorURL)
            reader.Close()
            reader = Nothing
            xfaFrm = Nothing
            Return FDFDox
        End Function
        Public Function GetXFAXML(ByVal PDFBuffer As Byte(), Optional ByVal ownerPassword As String = "") As Byte()
            Dim outputStream As New System.IO.MemoryStream()
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(PDFBuffer)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(PDFBuffer, _defaultEncoding.GetBytes(ownerPassword))
            End If
            Dim settings As System.Xml.XmlWriterSettings = New System.Xml.XmlWriterSettings
            settings.Indent = True
            Using writer As System.Xml.XmlWriter = System.Xml.XmlWriter.Create(outputStream, settings)
                reader.AcroFields.Xfa.DatasetsNode.WriteTo(writer)
            End Using
            If outputStream.CanSeek Then
                outputStream.Seek(0, SeekOrigin.Begin)
            End If
            Return outputStream.ToArray()
        End Function
        Public Function GetXFAXML(ByVal PDFPath As String, Optional ByVal ownerPassword As String = "") As Byte()
            Dim outputStream As New System.IO.MemoryStream()
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(PDFPath)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(PDFPath, _defaultEncoding.GetBytes(ownerPassword))
            End If
            Dim settings As System.Xml.XmlWriterSettings = New System.Xml.XmlWriterSettings
            settings.Indent = True
            Using writer As System.Xml.XmlWriter = System.Xml.XmlWriter.Create(outputStream, settings)
                reader.AcroFields.Xfa.DatasetsNode.WriteTo(writer)
            End Using
            If outputStream.CanSeek Then
                outputStream.Seek(0, SeekOrigin.Begin)
            End If
            Return outputStream.ToArray()
        End Function
        Public Function GetXFAXML(ByVal PDFStream As Stream, Optional ByVal ownerPassword As String = "") As Byte()
            Dim outputStream As New System.IO.MemoryStream()
            If PDFStream.CanSeek Then
                PDFStream.Seek(0, SeekOrigin.Begin)
            End If
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(PDFStream)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(PDFStream, _defaultEncoding.GetBytes(ownerPassword))
            End If
            Dim settings As System.Xml.XmlWriterSettings = New System.Xml.XmlWriterSettings
            settings.Indent = True
            Using writer As System.Xml.XmlWriter = System.Xml.XmlWriter.Create(outputStream, settings)
                reader.AcroFields.Xfa.DatasetsNode.WriteTo(writer)
            End Using
            If outputStream.CanSeek Then
                outputStream.Seek(0, SeekOrigin.Begin)
            End If
            Return outputStream.ToArray()
        End Function
        Private Function parseXFA(ByVal PDFBuffer As Byte(), Optional ByVal FDFInitialize As Boolean = False, Optional ByVal ownerPassword As String = "") As FDFDoc_Class
            If FDFInitialize Then FDFDox.Initialize(_defaultEncoding)
            FDFDox.DefaultEncoding = _defaultEncoding
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(PDFBuffer)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(PDFBuffer, _defaultEncoding.GetBytes(ownerPassword))
            End If
            Dim xfaFrm As New iTextSharp.text.pdf.XfaForm(reader)
            Dim isXFA As Boolean = False
            isXFA = xfaFrm.XfaPresent
            If Not isXFA Then
                reader.Close()
                reader = Nothing
                xfaFrm = Nothing
                Return parsePDF(PDFBuffer, FDFInitialize)
                Exit Function
            End If
            Dim xmlData() As Byte = GetXFAXML(PDFBuffer)
            FDFDox = parseXDP(_defaultEncoding.GetString(xmlData), True)
            For Each fld As FDFApp.FDFDoc_Class.FDFField In FDFDox.XDPGetAllFields()
                Select Case reader.AcroFields.GetFieldType(fld.FieldName)
                    Case iTextSharp.text.pdf.AcroFields.FIELD_TYPE_TEXT
                        FDFDox.XDPGetField(fld.FieldLevelLong).FieldType = FDFDoc_Class.FieldType.FldTextual
                    Case iTextSharp.text.pdf.AcroFields.FIELD_TYPE_LIST
                        FDFDox.XDPGetField(fld.FieldLevelLong).FieldType = FDFDoc_Class.FieldType.FldMultiSelect
                    Case iTextSharp.text.pdf.AcroFields.FIELD_TYPE_CHECKBOX
                        FDFDox.XDPGetField(fld.FieldLevelLong).FieldType = FDFDoc_Class.FieldType.FldTextual
                    Case iTextSharp.text.pdf.AcroFields.FIELD_TYPE_COMBO
                        FDFDox.XDPGetField(fld.FieldLevelLong).FieldType = FDFDoc_Class.FieldType.FldTextual
                    Case iTextSharp.text.pdf.AcroFields.FIELD_TYPE_PUSHBUTTON
                        FDFDox.XDPGetField(fld.FieldLevelLong).FieldType = FDFDoc_Class.FieldType.FldButton
                    Case iTextSharp.text.pdf.AcroFields.FIELD_TYPE_SIGNATURE

                    Case iTextSharp.text.pdf.AcroFields.FIELD_TYPE_NONE

                    Case Else

                End Select
            Next
ContinueProcess:
            FDFDox.FDFData = FDFDox.FDFSavetoStr(FDFDoc_Class.FDFType.FDF, True)
            FDFDox.XDPData = FDFDox.FDFSavetoStr(FDFDoc_Class.FDFType.XDP, True)
            reader.Close()
            reader = Nothing
            xfaFrm = Nothing
            Return FDFDox
        End Function
        Private Function parseXFA(ByVal PDFStream As Stream, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal ownerPassword As String = "") As FDFDoc_Class
            If FDFInitialize Then FDFDox.Initialize(_defaultEncoding)
            FDFDox.DefaultEncoding = _defaultEncoding
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(PDFStream)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(PDFStream, _defaultEncoding.GetBytes(ownerPassword))
            End If
            Dim xfaFrm As New iTextSharp.text.pdf.XfaForm(reader)
            Dim isXFA As Boolean = False
            isXFA = xfaFrm.XfaPresent
            If Not isXFA Then
                reader.Close()
                reader = Nothing
                xfaFrm = Nothing
                Return parsePDF(PDFStream, FDFInitialize)
                Exit Function
            End If
            Dim xmlData() As Byte = GetXFAXML(PDFStream)
            FDFDox = parseXDP(_defaultEncoding.GetString(xmlData), True)
            For Each fld As FDFApp.FDFDoc_Class.FDFField In FDFDox.XDPGetAllFields()
                Select Case reader.AcroFields.GetFieldType(fld.FieldName)
                    Case iTextSharp.text.pdf.AcroFields.FIELD_TYPE_TEXT
                        FDFDox.XDPGetField(fld.FieldLevelLong).FieldType = FDFDoc_Class.FieldType.FldTextual
                    Case iTextSharp.text.pdf.AcroFields.FIELD_TYPE_LIST
                        FDFDox.XDPGetField(fld.FieldLevelLong).FieldType = FDFDoc_Class.FieldType.FldMultiSelect
                    Case iTextSharp.text.pdf.AcroFields.FIELD_TYPE_CHECKBOX
                        FDFDox.XDPGetField(fld.FieldLevelLong).FieldType = FDFDoc_Class.FieldType.FldTextual
                    Case iTextSharp.text.pdf.AcroFields.FIELD_TYPE_COMBO
                        FDFDox.XDPGetField(fld.FieldLevelLong).FieldType = FDFDoc_Class.FieldType.FldTextual
                    Case iTextSharp.text.pdf.AcroFields.FIELD_TYPE_PUSHBUTTON
                        FDFDox.XDPGetField(fld.FieldLevelLong).FieldType = FDFDoc_Class.FieldType.FldButton
                    Case iTextSharp.text.pdf.AcroFields.FIELD_TYPE_SIGNATURE

                    Case iTextSharp.text.pdf.AcroFields.FIELD_TYPE_NONE

                    Case Else

                End Select
            Next
ContinueProcess:
            FDFDox.FDFData = FDFDox.FDFSavetoStr(FDFDoc_Class.FDFType.FDF, True)
            FDFDox.XDPData = FDFDox.FDFSavetoStr(FDFDoc_Class.FDFType.XDP, True)
            reader.Close()
            reader = Nothing
            xfaFrm = Nothing
            Return FDFDox
        End Function
        Private Function parseFDFiB(ByVal FDF As String, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal AppendSaves As Boolean = True) As FDFDoc_Class
            If FDFInitialize Then FDFDox.Initialize(_defaultEncoding)
            FDFDox.DefaultEncoding = _defaultEncoding
            FDFDox.FDFData = FDF
            Dim reader As iTextSharp.text.pdf.FdfReader
            reader = New iTextSharp.text.pdf.FdfReader(Me.StringToByteArray(FDF))
            Dim fld As String
            Dim vals As New iTextSharp.text.pdf.PdfDictionary
            Dim form As iTextSharp.text.pdf.AcroFields
            form = reader.AcroFields
            Dim fspec As String = reader.FileSpec
            Try
                For Each fld In reader.Fields.Keys
                    Dim fieldName As String
                    fieldName = fld
                    Dim val As String = reader.GetFieldValue(fld.ToString)
                    Dim FldType As String = ""
                    val = reader.GetFieldValue(fieldName) & ""
                    Try
                        Dim arrVals As New System.Collections.Generic.List(Of String)
                        Dim arrDisplay As New System.Collections.Generic.List(Of String)
                        Dim arrExports As New System.Collections.Generic.List(Of String)
                        If DirectCast(reader.GetField(fld), iTextSharp.text.pdf.PdfDictionary).Get(iTextSharp.text.pdf.PdfName.V).IsArray Then
                            Try
                                If Not DirectCast(reader.GetField(fld), iTextSharp.text.pdf.PdfDictionary).GetAsArray(iTextSharp.text.pdf.PdfName.V) Is Nothing Then
                                    Dim arV As iTextSharp.text.pdf.PdfArray = DirectCast(reader.GetField(fld), iTextSharp.text.pdf.PdfDictionary).GetAsArray(iTextSharp.text.pdf.PdfName.V)
                                    For x123 As Integer = 0 To arV.Size - 1
                                        Try
                                            Dim strTemp As String = arV.GetAsString(x123).ToUnicodeString() & ""
                                            If Not arrVals.Contains(strTemp) Then
                                                arrVals.Add(strTemp)
                                            End If
                                        Catch ex2 As Exception
                                            Err.Clear()
                                        End Try
                                    Next
                                End If
                            Catch ex As Exception
                                Err.Clear()
                            End Try
                            Try
                                If DirectCast(reader.GetField(fld), iTextSharp.text.pdf.PdfDictionary).Get(iTextSharp.text.pdf.PdfName.OPT).IsArray Then
                                    Dim arOpt As iTextSharp.text.pdf.PdfArray = DirectCast(reader.GetField(fld), iTextSharp.text.pdf.PdfDictionary).GetAsArray(iTextSharp.text.pdf.PdfName.OPT)
                                    For x123 As Integer = 0 To arOpt.Size - 1
                                        Try
                                            If Not arOpt.GetAsArray(x123) Is Nothing Then
                                                If arOpt.GetAsArray(x123).Size >= 2 Then
                                                    Dim strTempExport As String = arOpt.GetAsArray(x123).GetAsString(0).ToUnicodeString() & ""
                                                    If Not arrExports.Contains(strTempExport) Then
                                                        arrExports.Add(strTempExport)
                                                    End If
                                                    Dim strTempDisplay As String = arOpt.GetAsArray(x123).GetAsString(1).ToUnicodeString() & ""
                                                    If Not arrDisplay.Contains(strTempDisplay) Then
                                                        arrDisplay.Add(strTempDisplay)
                                                    End If
                                                Else
                                                    Dim strTempExport As String = arOpt.GetAsArray(x123).GetAsString(0).ToUnicodeString() & ""
                                                    If Not arrExports.Contains(strTempExport) Then
                                                        arrExports.Add(strTempExport)
                                                    End If
                                                    Dim strTempDisplay As String = arOpt.GetAsArray(x123).GetAsString(0).ToUnicodeString() & ""
                                                    If Not arrDisplay.Contains(strTempDisplay) Then
                                                        arrDisplay.Add(strTempDisplay)
                                                    End If
                                                End If
                                            End If
                                        Catch ex2 As Exception
                                            Err.Clear()
                                        End Try
                                    Next
                                    FDFDox.FDFAddField(fieldName & "", arrVals.ToArray, arrDisplay.ToArray, arrExports.ToArray, FDFDoc_Class.FieldType.FldMultiSelect, True, True)
                                Else
                                    If arrVals.Count > 0 Then
                                        FDFDox.FDFAddField(fieldName & "", arrVals.ToArray, FDFDoc_Class.FieldType.FldMultiSelect, True, True)
                                    Else
                                        FDFDox.FDFAddField(fieldName & "", val & "", FDFDoc_Class.FieldType.FldTextual, True, True)
                                    End If
                                End If
                            Catch ex As Exception
                                Err.Clear()
                            End Try
                        Else
                            FDFDox.FDFAddField(fieldName & "", val & "", FDFDoc_Class.FieldType.FldTextual, True, True)
                        End If
                    Catch ex As Exception
                        FDFDox.FDFAddField(fieldName & "", val & "", FDFDoc_Class.FieldType.FldTextual, True, True)
                        Err.Clear()
                    End Try
                Next
                FDFDox.FDFSetFile(reader.FileSpec.ToString)
                FDFDox.FDFData = FDFDox.FDFSavetoStr(FDFDoc_Class.FDFType.FDF, True)
                reader.Close()
                Return FDFDox
            Catch ex As Exception
                Try
                    reader.Close()
                    Return FDFDox
                Catch ex2 As Exception
                    Return FDFDox
                End Try
            End Try
            reader = Nothing
            FDFDox.XDPAdjustSubforms()
            Return FDFDox
        End Function
        Private Function parseFDFi(ByVal FDF As Byte(), Optional ByVal FDFInitialize As Boolean = False, Optional ByVal AppendSaves As Boolean = True, Optional ByVal ownerPassword As String = "") As FDFDoc_Class
            If FDFInitialize Then FDFDox.Initialize(_defaultEncoding)
            FDFDox.DefaultEncoding = _defaultEncoding
            FDFDox.FDFData = ByteArrayToString(FDF)
            Dim reader As iTextSharp.text.pdf.FdfReader
            reader = New iTextSharp.text.pdf.FdfReader(FDF)
            Dim fld As DictionaryEntry
            Dim vals As New iTextSharp.text.pdf.PdfDictionary
            Dim form As iTextSharp.text.pdf.AcroFields
            form = reader.AcroFields
            Dim fields As New Hashtable
            fields = reader.Fields
            Try
                For Each fld In fields
                    Dim fieldName As String
                    fieldName = DirectCast(fld.Key, String)
                    Dim val As String = "" 'fld.ToString
                    Dim FldType As String = ""
                    val = reader.GetFieldValue(fieldName) & ""
                    Try
                        Dim arrVals As New System.Collections.Generic.List(Of String)
                        Dim arrDisplay As New System.Collections.Generic.List(Of String)
                        Dim arrExports As New System.Collections.Generic.List(Of String)
                        If Not DirectCast(reader.GetField(fieldName), iTextSharp.text.pdf.PdfDictionary).Get(iTextSharp.text.pdf.PdfName.V) Is Nothing Or Not DirectCast(reader.GetField(fieldName), iTextSharp.text.pdf.PdfDictionary).Get(iTextSharp.text.pdf.PdfName.OPT) Is Nothing Then
                            Try
                                If Not DirectCast(reader.GetField(fieldName), iTextSharp.text.pdf.PdfDictionary).GetAsArray(iTextSharp.text.pdf.PdfName.V) Is Nothing Then
                                    Dim arV As iTextSharp.text.pdf.PdfArray = DirectCast(reader.GetField(fieldName), iTextSharp.text.pdf.PdfDictionary).GetAsArray(iTextSharp.text.pdf.PdfName.V)
                                    For x123 As Integer = 0 To arV.Size - 1
                                        Try
                                            Dim strTemp As String = arV.GetAsString(x123).ToUnicodeString() & ""
                                            If Not arrVals.Contains(strTemp) Then
                                                arrVals.Add(strTemp)
                                            End If
                                        Catch ex2 As Exception
                                            Err.Clear()
                                        End Try
                                    Next
                                End If
                            Catch ex As Exception
                                Err.Clear()
                            End Try
                            Try
                                If Not DirectCast(reader.GetField(fieldName), iTextSharp.text.pdf.PdfDictionary).Get(iTextSharp.text.pdf.PdfName.OPT) Is Nothing Then
                                    If DirectCast(reader.GetField(fieldName), iTextSharp.text.pdf.PdfDictionary).Get(iTextSharp.text.pdf.PdfName.OPT).IsArray Then
                                        Dim arOpt As iTextSharp.text.pdf.PdfArray = DirectCast(reader.GetField(fieldName), iTextSharp.text.pdf.PdfDictionary).GetAsArray(iTextSharp.text.pdf.PdfName.OPT)
                                        For x123 As Integer = 0 To arOpt.Size - 1
                                            Try
                                                If Not arOpt.GetAsArray(x123) Is Nothing Then
                                                    If arOpt.GetAsArray(x123).Size >= 2 Then
                                                        Dim strTempExport As String = arOpt.GetAsArray(x123).GetAsString(0).ToUnicodeString() & ""
                                                        If Not arrExports.Contains(strTempExport) Then
                                                            arrExports.Add(strTempExport)
                                                        End If
                                                        Dim strTempDisplay As String = arOpt.GetAsArray(x123).GetAsString(1).ToUnicodeString() & ""
                                                        If Not arrDisplay.Contains(strTempDisplay) Then
                                                            arrDisplay.Add(strTempDisplay)
                                                        End If
                                                    Else
                                                        Dim strTempExport As String = arOpt.GetAsArray(x123).GetAsString(0).ToUnicodeString() & ""
                                                        If Not arrExports.Contains(strTempExport) Then
                                                            arrExports.Add(strTempExport)
                                                        End If
                                                        Dim strTempDisplay As String = arOpt.GetAsArray(x123).GetAsString(0).ToUnicodeString() & ""
                                                        If Not arrDisplay.Contains(strTempDisplay) Then
                                                            arrDisplay.Add(strTempDisplay)
                                                        End If
                                                    End If
                                                End If
                                            Catch ex2 As Exception
                                                Err.Clear()
                                            End Try
                                        Next
                                        FDFDox.FDFAddField(fieldName & "", arrVals.ToArray, arrDisplay.ToArray, arrExports.ToArray, FDFDoc_Class.FieldType.FldMultiSelect, True, True)
                                    Else
                                        If arrVals.Count > 0 Then
                                            FDFDox.FDFAddField(fieldName & "", arrVals.ToArray, FDFDoc_Class.FieldType.FldMultiSelect, True, True)
                                        Else
                                            FDFDox.FDFAddField(fieldName & "", val & "", FDFDoc_Class.FieldType.FldTextual, True, True)
                                        End If
                                    End If
                                Else
                                    If arrVals.Count > 0 Then
                                        FDFDox.FDFAddField(fieldName & "", arrVals.ToArray, FDFDoc_Class.FieldType.FldMultiSelect, True, True)
                                    Else
                                        FDFDox.FDFAddField(fieldName & "", val & "", FDFDoc_Class.FieldType.FldTextual, True, True)
                                    End If
                                End If
                            Catch ex As Exception
                                Err.Clear()
                            End Try
                        Else
                            FDFDox.FDFAddField(fieldName & "", val & "", FDFDoc_Class.FieldType.FldTextual, True, True)
                        End If
                    Catch ex As Exception
                        FDFDox.FDFAddField(fieldName & "", val & "", FDFDoc_Class.FieldType.FldTextual, True, True)
                        Err.Clear()
                    End Try
                Next
                FDFDox.FDFSetFile(reader.FileSpec.ToString)
                FDFDox.XDPAdjustSubforms()
                FDFDox.FDFData = FDFDox.FDFSavetoStr(FDFDoc_Class.FDFType.FDF, True)
                reader.Close()
                Return FDFDox
            Catch ex As Exception
                Try
                    reader.Close()
                    Return FDFDox
                Catch ex2 As Exception
                    Return FDFDox
                End Try
            End Try
            reader = Nothing
            FDFDox.XDPAdjustSubforms()
            Return FDFDox
        End Function
        Private Function FDFGetFileiText(ByVal FDFData As String) As String
            Dim reader As iTextSharp.text.pdf.FdfReader
            Try
                reader = New iTextSharp.text.pdf.FdfReader(_defaultEncoding.GetBytes(FDFData))
                Return reader.FileSpec.ToString & ""
            Catch ex As Exception
                Return ""
            End Try
        End Function
        Private Function parseFDFi(ByVal FDF As Stream, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal AppendSaves As Boolean = True, Optional ByVal ownerPassword As String = "") As FDFDoc_Class
            If FDFInitialize Then FDFDox.Initialize(_defaultEncoding)
            FDFDox.DefaultEncoding = _defaultEncoding
            Dim reader As iTextSharp.text.pdf.FdfReader
            reader = New iTextSharp.text.pdf.FdfReader(FDF)
            Dim fld As DictionaryEntry
            Dim vals As New iTextSharp.text.pdf.PdfDictionary
            Dim form As iTextSharp.text.pdf.AcroFields
            form = reader.AcroFields
            Dim fields As New Hashtable
            fields = reader.Fields
            Try
                For Each fld In fields
                    Dim fieldName As String
                    fieldName = DirectCast(fld.Key, String)
                    Dim val As String = "" 'fld.ToString
                    Dim FldType As String = ""
                    val = reader.GetFieldValue(fieldName) & ""
                    Try
                        Dim arrVals As New System.Collections.Generic.List(Of String)
                        Dim arrDisplay As New System.Collections.Generic.List(Of String)
                        Dim arrExports As New System.Collections.Generic.List(Of String)
                        If Not DirectCast(reader.GetField(fieldName), iTextSharp.text.pdf.PdfDictionary).Get(iTextSharp.text.pdf.PdfName.V) Is Nothing Or Not DirectCast(reader.GetField(fieldName), iTextSharp.text.pdf.PdfDictionary).Get(iTextSharp.text.pdf.PdfName.OPT) Is Nothing Then
                            Try
                                If Not DirectCast(reader.GetField(fieldName), iTextSharp.text.pdf.PdfDictionary).GetAsArray(iTextSharp.text.pdf.PdfName.V) Is Nothing Then
                                    Dim arV As iTextSharp.text.pdf.PdfArray = DirectCast(reader.GetField(fieldName), iTextSharp.text.pdf.PdfDictionary).GetAsArray(iTextSharp.text.pdf.PdfName.V)
                                    For x123 As Integer = 0 To arV.Size - 1
                                        Try
                                            Dim strTemp As String = arV.GetAsString(x123).ToUnicodeString() & ""
                                            If Not arrVals.Contains(strTemp) Then
                                                arrVals.Add(strTemp)
                                            End If
                                        Catch ex2 As Exception
                                            Err.Clear()
                                        End Try
                                    Next
                                End If
                            Catch ex As Exception
                                Err.Clear()
                            End Try
                            Try
                                If Not DirectCast(reader.GetField(fieldName), iTextSharp.text.pdf.PdfDictionary).GetAsArray(iTextSharp.text.pdf.PdfName.OPT) Is Nothing Then
                                    Dim arOpt As iTextSharp.text.pdf.PdfArray = DirectCast(reader.GetField(fieldName), iTextSharp.text.pdf.PdfDictionary).GetAsArray(iTextSharp.text.pdf.PdfName.OPT)
                                    For x123 As Integer = 0 To arOpt.Size - 1
                                        Try
                                            If Not arOpt.GetAsArray(x123) Is Nothing Then
                                                If arOpt.GetAsArray(x123).Size >= 2 Then
                                                    Dim strTempExport As String = arOpt.GetAsArray(x123).GetAsString(0).ToUnicodeString() & ""
                                                    If Not arrExports.Contains(strTempExport) Then
                                                        arrExports.Add(strTempExport)
                                                    End If
                                                    Dim strTempDisplay As String = arOpt.GetAsArray(x123).GetAsString(1).ToUnicodeString() & ""
                                                    If Not arrDisplay.Contains(strTempDisplay) Then
                                                        arrDisplay.Add(strTempDisplay)
                                                    End If
                                                Else
                                                    Dim strTempExport As String = arOpt.GetAsArray(x123).GetAsString(0).ToUnicodeString() & ""
                                                    If Not arrExports.Contains(strTempExport) Then
                                                        arrExports.Add(strTempExport)
                                                    End If
                                                    Dim strTempDisplay As String = arOpt.GetAsArray(x123).GetAsString(0).ToUnicodeString() & ""
                                                    If Not arrDisplay.Contains(strTempDisplay) Then
                                                        arrDisplay.Add(strTempDisplay)
                                                    End If
                                                End If
                                            End If
                                        Catch ex2 As Exception
                                            Err.Clear()
                                        End Try
                                    Next
                                    FDFDox.FDFAddField(fieldName & "", arrVals.ToArray, arrDisplay.ToArray, arrExports.ToArray, FDFDoc_Class.FieldType.FldMultiSelect, True, True)
                                Else
                                    If arrVals.Count > 1 Then
                                        FDFDox.FDFAddField(fieldName & "", arrVals.ToArray, FDFDoc_Class.FieldType.FldMultiSelect, True, True)
                                    Else
                                        FDFDox.FDFAddField(fieldName & "", val & "", FDFDoc_Class.FieldType.FldTextual, True, True)
                                        'FDFDox.FDFAddField(fieldName & "", arrVals(0) & "", FDFDoc_Class.FieldType.FldTextual, True, True)
                                    End If
                                End If
                            Catch ex As Exception
                                Err.Clear()
                            End Try
                        Else
                            FDFDox.FDFAddField(fieldName & "", val & "", FDFDoc_Class.FieldType.FldTextual, True, True)
                        End If
                    Catch ex As Exception
                        FDFDox.FDFAddField(fieldName & "", val & "", FDFDoc_Class.FieldType.FldTextual, True, True)
                        Err.Clear()
                    End Try
                Next
                FDFDox.FDFSetFile(reader.FileSpec.ToString)
                FDFDox.XDPAdjustSubforms()
                FDFDox.FDFData = FDFDox.FDFSavetoStr(FDFDoc_Class.FDFType.FDF, True)
                reader.Close()
                Return FDFDox
            Catch ex As Exception
                Try
                    reader.Close()
                    Return FDFDox
                Catch ex2 As Exception
                    Return FDFDox
                End Try
            End Try
            reader = Nothing
            FDFDox.XDPAdjustSubforms()
            Return FDFDox
        End Function
        Private Function parseFDFi(ByVal FileNameorURL As String, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal AppendSaves As Boolean = True, Optional ByVal ownerPassword As String = "") As FDFDoc_Class
            If FDFInitialize Then FDFDox.Initialize(_defaultEncoding)
            FDFDox.DefaultEncoding = _defaultEncoding
            Dim reader As iTextSharp.text.pdf.FdfReader
            reader = New iTextSharp.text.pdf.FdfReader(FileNameorURL)
            Dim fld As DictionaryEntry
            Dim vals As New iTextSharp.text.pdf.PdfDictionary
            Dim form As iTextSharp.text.pdf.AcroFields
            form = reader.AcroFields
            Dim fields As New Hashtable
            fields = reader.Fields
            Try
                For Each fld In fields
                    Dim fieldName As String
                    fieldName = DirectCast(fld.Key, String)
                    Dim val As String = "" 'fld.ToString
                    Dim FldType As String = ""
                    val = reader.GetFieldValue(fieldName) & ""
                    Try
                        Dim arrVals As New System.Collections.Generic.List(Of String)
                        Dim arrDisplay As New System.Collections.Generic.List(Of String)
                        Dim arrExports As New System.Collections.Generic.List(Of String)
                        If Not DirectCast(reader.GetField(fieldName), iTextSharp.text.pdf.PdfDictionary).Get(iTextSharp.text.pdf.PdfName.V) Is Nothing Or Not DirectCast(reader.GetField(fieldName), iTextSharp.text.pdf.PdfDictionary).Get(iTextSharp.text.pdf.PdfName.OPT) Is Nothing Then
                            Try
                                If Not DirectCast(reader.GetField(fieldName), iTextSharp.text.pdf.PdfDictionary).Get(iTextSharp.text.pdf.PdfName.V) Is Nothing Then
                                    Dim arV As iTextSharp.text.pdf.PdfArray = DirectCast(reader.GetField(fieldName), iTextSharp.text.pdf.PdfDictionary).GetAsArray(iTextSharp.text.pdf.PdfName.V)
                                    For x123 As Integer = 0 To arV.Size - 1
                                        Try
                                            Dim strTemp As String = arV.GetAsString(x123).ToUnicodeString() & ""
                                            If Not arrVals.Contains(strTemp) Then
                                                arrVals.Add(strTemp)
                                            End If
                                        Catch ex2 As Exception
                                            Err.Clear()
                                        End Try
                                    Next
                                End If
                            Catch ex As Exception
                                Err.Clear()
                            End Try
                            Try
                                If Not DirectCast(reader.GetField(fieldName), iTextSharp.text.pdf.PdfDictionary).Get(iTextSharp.text.pdf.PdfName.OPT) Is Nothing Then
                                    Dim arOpt As iTextSharp.text.pdf.PdfArray = DirectCast(reader.GetField(fieldName), iTextSharp.text.pdf.PdfDictionary).GetAsArray(iTextSharp.text.pdf.PdfName.OPT)
                                    For x123 As Integer = 0 To arOpt.Size - 1
                                        Try
                                            If Not arOpt.GetAsArray(x123) Is Nothing Then
                                                If arOpt.GetAsArray(x123).Size >= 2 Then
                                                    Dim strTempExport As String = arOpt.GetAsArray(x123).GetAsString(0).ToUnicodeString() & ""
                                                    If Not arrExports.Contains(strTempExport) Then
                                                        arrExports.Add(strTempExport)
                                                    End If
                                                    Dim strTempDisplay As String = arOpt.GetAsArray(x123).GetAsString(1).ToUnicodeString() & ""
                                                    If Not arrDisplay.Contains(strTempDisplay) Then
                                                        arrDisplay.Add(strTempDisplay)
                                                    End If
                                                Else
                                                    Dim strTempExport As String = arOpt.GetAsArray(x123).GetAsString(0).ToUnicodeString() & ""
                                                    If Not arrExports.Contains(strTempExport) Then
                                                        arrExports.Add(strTempExport)
                                                    End If
                                                    Dim strTempDisplay As String = arOpt.GetAsArray(x123).GetAsString(0).ToUnicodeString() & ""
                                                    If Not arrDisplay.Contains(strTempDisplay) Then
                                                        arrDisplay.Add(strTempDisplay)
                                                    End If
                                                End If
                                            End If
                                        Catch ex2 As Exception
                                            Err.Clear()
                                        End Try
                                    Next
                                    FDFDox.FDFAddField(fieldName & "", arrVals.ToArray, arrDisplay.ToArray, arrExports.ToArray, FDFDoc_Class.FieldType.FldMultiSelect, True, True)
                                Else
                                    If arrVals.Count > 1 Then
                                        FDFDox.FDFAddField(fieldName & "", arrVals.ToArray, FDFDoc_Class.FieldType.FldMultiSelect, True, True)
                                    Else
                                        FDFDox.FDFAddField(fieldName & "", val & "", FDFDoc_Class.FieldType.FldTextual, True, True)
                                    End If
                                End If
                            Catch ex As Exception
                                Err.Clear()
                            End Try
                        Else
                            FDFDox.FDFAddField(fieldName & "", val & "", FDFDoc_Class.FieldType.FldTextual, True, True)
                        End If
                    Catch ex As Exception
                        FDFDox.FDFAddField(fieldName & "", val & "", FDFDoc_Class.FieldType.FldTextual, True, True)
                        Err.Clear()
                    End Try
                Next
                FDFDox.FDFSetFile(reader.FileSpec.ToString)
                FDFDox.XDPAdjustSubforms()
                FDFDox.FDFData = FDFDox.FDFSavetoStr(FDFDoc_Class.FDFType.FDF, True)
                reader.Close()
                Return FDFDox
            Catch ex As Exception
                Try
                    reader.Close()
                    Return FDFDox
                Catch ex2 As Exception
                    Return FDFDox
                End Try
            End Try
            reader = Nothing
            FDFDox.XDPAdjustSubforms()
            Return FDFDox
        End Function
        Private Function parseFDF(ByVal FDF As String, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal AppendSaves As Boolean = True) As FDFDoc_Class
            Dim tmpFDF As String = FDF
            If FDFInitialize Then FDFDox.Initialize(_defaultEncoding)
            FDFDox.DefaultEncoding = _defaultEncoding
            Try
                If Has_Kids(0, FDF.Length - 1, FDF) Then
                    Return parseFDFiB(FDF, FDFInitialize, AppendSaves)
                    Exit Function
                End If
                FDF = ByteArrayToString(_defaultEncoding.GetBytes(FDF))
                Dim strFields(4) As String
                Dim FldStart As Integer, FldEnd As Integer
                Dim intField(7) As Integer
                FldStart = FDF.ToLower.IndexOf("/fields")
                If FDF.IndexOf("[", FldStart + 7, 2) > 0 Then
                    FldStart = FldStart + 1
                    FldEnd = FDF.ToLower.IndexOf("endobj", FldStart + 8)
                Else
                    Dim strFieldsObject_Start As String
                    strFieldsObject_Start = FDF.Substring(FldStart + 7, FDF.IndexOf(" R", FldStart + 7) - (FldStart + 7))
                    strFieldsObject_Start = strFieldsObject_Start.TrimStart(" "c)
                    strFieldsObject_Start = strFieldsObject_Start.TrimEnd("/"c)
                    strFieldsObject_Start = strFieldsObject_Start.TrimEnd(" "c)
                    strFieldsObject_Start = strFieldsObject_Start.TrimEnd("R"c)
                    strFieldsObject_Start = strFieldsObject_Start & " obj"
                    FldStart = FDF.IndexOf(strFieldsObject_Start, FldStart)
                    FldEnd = FDF.ToLower.IndexOf("endobj", FldStart + 8)
                End If
                intField(0) = FldStart
                Dim strFix As String
                Try
                    Dim FldSubStr As String = ""
                    intField(1) = FDF.ToLower.IndexOf("/doc ", 1) + 5
                    If intField(1) > 5 Then
                        intField(1) = FDF.ToLower.IndexOf("[", intField(1)) + 1
                        intField(2) = FDF.ToLower.IndexOf("]", intField(1))
                        strFields(3) = FDF.Substring(intField(1), intField(2) - intField(1))
                        strFields(3) = strFields(0).TrimStart(" "c)
                        strFields(3) = strFields(0).TrimEnd(" "c)
                        Dim strDocScripts() As String
                        strDocScripts = strFields(3).Split(CStr(")(").ToCharArray)
                        Dim result As Integer
                        Dim intTmpScript As Integer = 0, strTmpScript As String = ""
                        result = CInt(strDocScripts.Length / 2)
                        If result > 0 Then
                            For intTmpScript = 0 To strDocScripts.Length - 1 Step 2
                                strDocScripts(intTmpScript).TrimStart(" "c)
                                strDocScripts(intTmpScript).TrimStart("("c)
                                strDocScripts(intTmpScript).TrimEnd(" "c)
                                strDocScripts(intTmpScript).TrimEnd(")"c)
                                strDocScripts(intTmpScript + 1).TrimStart(" "c)
                                strDocScripts(intTmpScript + 1).TrimStart("("c)
                                strDocScripts(intTmpScript + 1).TrimEnd(" "c)
                                strDocScripts(intTmpScript + 1).TrimEnd(")"c)
                            Next
                            intTmpScript = 0
                            If result > 2 Then
                                For intTmpScript = 0 To strDocScripts.Length - 1 Step 2
                                    FDFDox.FDFAddDocJavaScript(strDocScripts(intTmpScript), strDocScripts(intTmpScript + 1))
                                Next
                            ElseIf result = 2 Then
                                FDFDox.FDFAddDocJavaScript(strDocScripts(intTmpScript), strDocScripts(intTmpScript + 1))
                            End If
                        End If
                    End If
                    intField(0) = FldStart
                    Do While intField(0) < FldEnd
                        intField(0) = FDF.ToLower.IndexOf("<<", intField(0)) + 2
                        intField(5) = FDF.ToLower.IndexOf(">>", intField(0))
                        If intField(5) + 3 >= FldEnd Then
                            Exit Do
                        End If
                        FldSubStr = FDF.Substring(intField(0), intField(5) - intField(0))
                        Dim xit As Boolean = False
                        If (FldSubStr.ToLower.IndexOf("/t", 0) + 2 < FldSubStr.ToLower.IndexOf("/v", 0) + 2) Then
                            intField(1) = FldSubStr.ToLower.IndexOf("/t", 0) + 2
                            intField(1) = FldSubStr.ToLower.IndexOf("(", intField(1)) + 1
                            intField(2) = FldSubStr.ToLower.IndexOf(")", intField(1))
                            If FldSubStr.ToLower.IndexOf("/v", intField(1)) + 2 > 5 And FldSubStr.ToLower.IndexOf("/v", intField(2)) + 2 > FldSubStr.Length Then
                                intField(3) = FldSubStr.ToLower.IndexOf("/v", intField(2)) + 2
                                intField(4) = FldSubStr.Length
                            ElseIf FldSubStr.ToLower.IndexOf("/v/", intField(2), FldSubStr.Length - intField(2)) > 2 Then
                                intField(3) = FldSubStr.ToLower.IndexOf("/v/", intField(2)) + 3
                                intField(4) = FldSubStr.Length
                                If intField(4) > FldSubStr.Length Then
                                    If FldSubStr.ToLower.IndexOf("/", intField(3)) > intField(3) And FldSubStr.ToLower.IndexOf("/", intField(3)) < FldSubStr.Length Then
                                        intField(3) = FldSubStr.ToLower.IndexOf("/", intField(3)) + 1
                                        intField(4) = FldSubStr.ToLower.IndexOf(">>", intField(4)) - 2
                                    End If
                                End If
                            ElseIf FldSubStr.ToLower.IndexOf("/v /", intField(2), FldSubStr.Length - intField(2)) > 2 Then
                                intField(3) = FldSubStr.ToLower.IndexOf("/v/", intField(2)) + 3
                                intField(4) = FldSubStr.Length
                                If intField(4) > FldSubStr.Length Then
                                    If FldSubStr.ToLower.IndexOf("/", intField(3)) > intField(3) And FldSubStr.ToLower.IndexOf("/", intField(3)) < FldSubStr.Length Then
                                        intField(3) = FldSubStr.ToLower.IndexOf("/", intField(3)) + 1
                                        intField(4) = FldSubStr.ToLower.IndexOf(">>", intField(4)) - 2
                                    End If
                                End If
                            ElseIf FldSubStr.ToLower.IndexOf("/v/", intField(2), FldSubStr.Length - intField(2)) > 2 Then
                                intField(3) = FldSubStr.ToLower.IndexOf("/v/", intField(2)) + 3
                                intField(4) = FldSubStr.Length
                                If intField(4) > FldSubStr.Length Then
                                    If FldSubStr.ToLower.IndexOf("/", intField(3)) > intField(3) And FldSubStr.ToLower.IndexOf("/", intField(3)) < FldSubStr.Length Then
                                        intField(3) = FldSubStr.ToLower.IndexOf("/", intField(3)) + 1
                                        intField(4) = FldSubStr.ToLower.IndexOf(">>", intField(4)) - 2
                                    End If
                                End If
                            Else
                                intField(3) = FldSubStr.ToLower.IndexOf("/v", intField(2)) + 2
                                intField(3) = FldSubStr.ToLower.IndexOf("(", intField(3))
                                intField(4) = InStrRev(FldSubStr, ")", FldSubStr.Length, CompareMethod.Text)
                                If intField(4) > FldSubStr.Length Then
                                    If FldSubStr.ToLower.IndexOf("/", intField(3)) > intField(3) And FldSubStr.ToLower.IndexOf("/", intField(3)) < FldSubStr.Length Then
                                        intField(3) = FldSubStr.ToLower.IndexOf("/", intField(3)) + 1
                                        intField(4) = FldSubStr.ToLower.IndexOf(">>", intField(4)) - 2
                                    End If
                                End If
                            End If
                        Else
                            intField(1) = FldSubStr.ToLower.IndexOf("/v", 0) + 2
                            intField(1) = FldSubStr.ToLower.IndexOf("(", intField(1)) + 1
                            intField(2) = FldSubStr.ToLower.IndexOf(")", intField(1))
                            If FldSubStr.ToLower.IndexOf("/v", 0) + 2 > 5 And FldSubStr.ToLower.IndexOf("/v", 0) + 2 > FldSubStr.Length Then
                                intField(3) = FldSubStr.ToLower.IndexOf("/v", 0) + 2
                                intField(4) = FldSubStr.Length
                            ElseIf FldSubStr.ToLower.IndexOf("/v/", 0) > 1 Then
                                intField(3) = FldSubStr.ToLower.IndexOf("/v/", 0) + 3
                                intField(4) = FldSubStr.ToLower.IndexOf("/t", intField(3))
                                If intField(4) > FldSubStr.Length Then
                                    If FldSubStr.ToLower.IndexOf("/", intField(3)) > intField(3) And FldSubStr.ToLower.IndexOf("/", intField(3)) < FldSubStr.Length Then
                                        intField(3) = FldSubStr.ToLower.IndexOf("/", intField(3)) + 1
                                        intField(4) = intField(1) - 2
                                    End If
                                End If
                            ElseIf FldSubStr.ToLower.IndexOf("/v /", 0) > 0 Then
                                intField(3) = FldSubStr.ToLower.IndexOf("/v/", 0) + 4
                                intField(4) = FldSubStr.ToLower.IndexOf("/t", intField(3))
                                If intField(4) > FldSubStr.Length Then
                                    If FldSubStr.ToLower.IndexOf("/", intField(3)) > intField(3) And FldSubStr.ToLower.IndexOf("/", intField(3)) < FldSubStr.Length Then
                                        intField(3) = FldSubStr.ToLower.IndexOf("/", intField(3)) + 1
                                        intField(4) = intField(1) - 2
                                    End If
                                End If
                            ElseIf FldSubStr.ToLower.IndexOf("/v  /", 0) > 0 Then
                                intField(3) = FldSubStr.ToLower.IndexOf("/v/", 0) + 5
                                intField(4) = FldSubStr.ToLower.IndexOf("/t", intField(3))
                                If intField(4) > FldSubStr.Length Then
                                    If FldSubStr.ToLower.IndexOf("/", intField(3)) > intField(3) And FldSubStr.ToLower.IndexOf("/", intField(3)) < FldSubStr.Length Then
                                        intField(3) = FldSubStr.ToLower.IndexOf("/", intField(3)) + 1
                                        intField(4) = intField(1) - 2
                                    End If
                                End If
                            Else
                                intField(3) = FldSubStr.ToLower.IndexOf("/v", intField(2)) + 2
                                intField(3) = FldSubStr.ToLower.IndexOf("(", intField(3))
                                intField(4) = InStrRev(FldSubStr, ")", FldSubStr.Length - 1, CompareMethod.Text)
                                If intField(4) > FldSubStr.Length Then
                                    If FldSubStr.ToLower.IndexOf("/", intField(3)) > intField(3) And FldSubStr.ToLower.IndexOf("/", intField(3)) < intField(1) Then
                                        intField(3) = FldSubStr.ToLower.IndexOf("/", intField(3)) + 1
                                        intField(4) = intField(1) - 2
                                    End If
                                End If
                            End If
                            intField(1) = FldSubStr.ToLower.IndexOf("/t", 0) + 2
                            intField(1) = FldSubStr.ToLower.IndexOf("(", intField(1)) + 1
                            intField(2) = FldSubStr.ToLower.IndexOf(")", intField(1))
                        End If
                        xit = False
                        Dim lngFldFound As Long
                        lngFldFound = FldSubStr.ToLower.IndexOf("\)", intField(3)) + 2
                        Do While xit = False
                            If intField(4) = lngFldFound Then
                                intField(4) = FldSubStr.ToLower.IndexOf(")", CInt(lngFldFound)) + 1
                                lngFldFound = FldSubStr.ToLower.IndexOf("\)", intField(4)) + 2
                                xit = False
                            Else
                                xit = True
                            End If
                        Loop
                        If (Not intField(3) > FldSubStr.Length) And intField(3) > 3 Then
                            strFields(0) = FldSubStr.Substring(intField(1), intField(2) - intField(1))
                            strFields(1) = FldSubStr.Substring(intField(3), intField(4) - intField(3))
                            strFields(0) = strFields(0).TrimStart(" "c)
                            strFields(0) = strFields(0).TrimEnd(" "c)
                            strFields(1) = strFields(1).TrimStart(" "c)
                            strFields(1) = strFields(1).TrimEnd(" "c)
                            If strFields(1).StartsWith("("c) And strFields(1).EndsWith(")"c) Then
                                If InStrRev(FldSubStr, "<body", FldSubStr.Length, CompareMethod.Text) > intField(2) Then
                                    intField(3) = InStrRev(FldSubStr, "<body", FldSubStr.Length, CompareMethod.Text) + 5
                                    intField(3) = FldSubStr.IndexOf(">", intField(3)) + 1
                                    intField(4) = InStrRev(FldSubStr, "</body>", FldSubStr.Length, CompareMethod.Text) - 1
                                    strFields(0) = FldSubStr.Substring(intField(1), intField(2) - intField(1))
                                    strFields(1) = FldSubStr.Substring(intField(3), intField(4) - intField(3))
                                Else
                                    strFields(0) = strFields(0).TrimStart("("c)
                                    strFields(0) = strFields(0).TrimEnd(")"c)
                                    strFields(1) = strFields(1).Remove(0, 1)
                                    strFields(1) = strFields(1).Remove(strFields(1).Length - 1, 1)
                                End If
                                strFix = strFields(1)
                                strFields(1) = strFix
                                intField(4) = intField(5) + 2
                                FDFDox.FDFAddField(CStr(strFields(0)), CStr(FDFCheckCharReverse(strFields(1))), FDFDoc_Class.FieldType.FldTextual, True)
                            ElseIf strFields(1).StartsWith("["c) And strFields(1).EndsWith("]"c) Then
                                strFields(0) = strFields(0).TrimStart("("c)
                                strFields(0) = strFields(0).TrimEnd(")"c)
                                strFields(1) = strFields(1).TrimStart("["c)
                                strFields(1) = strFields(1).TrimEnd("]"c)
                                intField(4) = intField(5) + 2
                                FDFDox.FDFAddField(CStr(strFields(0)), CStr(FDFCheckCharReverse(strFields(1))), FDFDoc_Class.FieldType.FldMultiSelect)
                            ElseIf strFields(1).StartsWith("/"c) Then
                                strFields(0) = strFields(0).TrimStart("("c)
                                strFields(0) = strFields(0).TrimEnd(")"c)
                                strFields(1) = strFields(1).TrimStart("/"c)
                                strFields(1) = strFields(1).Substring(0, strFields(1).IndexOf(">>"))
                                intField(4) = intField(5) + 2
                                FDFDox.FDFAddField(strFields(0), FDFCheckCharReverse(strFields(1) & ""), FDFDoc_Class.FieldType.FldTextual)
                            ElseIf FldSubStr.ToLower.IndexOf("/v/", intField(2), FldSubStr.Length - intField(2)) > 2 Then
                                strFix = strFields(1)
                                strFields(1) = strFix
                                FDFDox.FDFAddField(CStr(strFields(0)), CStr(FDFCheckCharReverse(strFields(1))), FDFDoc_Class.FieldType.FldOption)
                                intField(4) = intField(5) + 2
                            Else
                                intField(4) = intField(5) + 2
                            End If
                        Else
                            If intField(3) > FldSubStr.Length Then
                                strFields(0) = FDF.Substring(intField(1), intField(2) - intField(1))
                                strFields(1) = FDF.Substring(intField(3), intField(4) - intField(3))
                                strFields(0) = strFields(0).TrimStart(" "c)
                                strFields(0) = strFields(0).TrimEnd(" "c)
                                If FDF.ToLower.IndexOf("/a ", intField(2)) > 1 Then
                                    intField(3) = FDF.ToLower.IndexOf("/javascript /js ", intField(1)) + 16
                                    If intField(3) <= 16 Then
                                        intField(3) = FDF.ToUpper.IndexOf("/URI /URI ", intField(1)) + 10
                                    End If
                                    If intField(3) <= 16 Then
                                        GoTo NO_SCRIPT
                                    End If
                                    intField(4) = FDF.ToLower.IndexOf(">>", intField(3))
                                    strFields(1) = FDF.Substring(intField(3), intField(4) - intField(3))
                                    strFields(1) = strFields(1).TrimStart(" "c)
                                    strFields(1) = strFields(1).TrimStart("("c)
                                    strFields(1) = strFields(1).TrimEnd(" "c)
                                    strFields(1) = strFields(1).TrimEnd(")"c)
                                    FDFDox.FDFSetJavaScriptAction(strFields(0), FDFDoc_Class.FDFActionTrigger.FDFUp, strFields(1))
                                ElseIf FDF.ToLower.IndexOf("/aa ", intField(2)) > 1 Then
                                    intField(3) = FDF.ToLower.IndexOf("/aa << /", intField(2)) + 8
                                    intField(4) = FDF.ToLower.IndexOf(" <<", intField(3))
                                    intField(5) = FDF.ToLower.IndexOf("/javascript /js ", intField(1)) + 16
                                    If intField(5) <= 16 Then
                                        intField(5) = FDF.ToUpper.IndexOf("/URI /URI ", intField(1)) + 10
                                    End If
                                    If intField(5) <= 16 Then
                                        GoTo NO_SCRIPT
                                    End If
                                    intField(6) = FDF.ToLower.IndexOf(">>", intField(3))
                                    strFields(1) = FDF.Substring(intField(3), intField(4) - intField(3))
                                    strFields(1) = strFields(1).TrimStart(" "c)
                                    strFields(1) = strFields(1).TrimStart("("c)
                                    strFields(1) = strFields(1).TrimEnd(" "c)
                                    strFields(1) = strFields(1).TrimEnd(")"c)
                                    strFields(2) = FDF.Substring(intField(5), intField(6) - intField(5))
                                    strFields(2) = strFields(2).TrimStart(" "c)
                                    strFields(2) = strFields(2).TrimStart("("c)
                                    strFields(2) = strFields(2).TrimEnd(" "c)
                                    strFields(2) = strFields(2).TrimEnd(")"c)
                                    FDFDox.FDFSetJavaScriptAction(strFields(0), ReturnTriggerString(strFields(1)), strFields(2))
                                Else
                                    intField(4) = FDF.ToLower.IndexOf(">>", intField(2))
                                End If
                            End If
NO_SCRIPT:
                            If FDF.ToLower.IndexOf("/t", intField(0)) < 5 Then
                                intField(3) = FldEnd + 1
                            End If
                        End If
                        intField(0) = intField(5) + 2
                        If intField(0) + 4 >= FldEnd Then
                            Exit Do
                        ElseIf intField(5) + 4 >= FldEnd Then
                            Exit Do
                        End If
                    Loop
                Catch ex As Exception
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, ex.Message, "FDFApp.parseFDF", 1)
                    Return FDFDox
                End Try
                Try
                    intField(1) = FDF.ToLower.IndexOf("/f ") + 3
                    If intField(1) <= 3 Then
                        intField(1) = FDF.ToLower.IndexOf("/f(") + 3
                    End If
                    If intField(1) <= 3 Then
                        intField(1) = FDF.ToLower.IndexOf("/f")
                        If intField(1) = FDF.ToLower.IndexOf("/fields") Then
                            intField(1) = FDF.ToLower.IndexOf("/f", intField(1)) + 2
                        End If
                    End If
                    If intField(1) > 3 Then
                        intField(2) = FDF.IndexOf(")", intField(1))
                        strFields(0) = FDF.Substring(intField(1), intField(2) - intField(1)) & ""
                        strFields(0) = strFields(0).Replace("(", "")
                        strFields(0) = strFields(0).Replace(")", "")
                        strFields(0) = strFields(0).TrimStart(" "c)
                        strFields(0) = strFields(0).TrimEnd(" "c)
                        FDFDox.FDFSetFile(strFields(0) & "")
                    End If
                Catch ex As Exception
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, ex.Message, "FDFApp.parseFDF", 2)
                    Return FDFDox
                End Try
                Try
                    If String_IsNullOrEmpty(FDFDox.FDFGetFile) Then
                        FDFDox.FDFSetFile(FDFGetFileiText(tmpFDF))
                    End If
                Catch ex As Exception
                End Try
                Try
                    If AppendSaves Then
                        FDFDox = FDFImportAppendSaves(FDF, False)
                    End If
                Catch ex As Exception
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.parseFDF", 3)
                    Return FDFDox
                End Try
                FDFDox.FDFData = FDF
                FDFDox.XDPAdjustSubforms()
                Return FDFDox
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, Ex)
                Return Nothing
            End Try
        End Function
        Private Function parsePDF(ByVal PDF As Byte(), Optional ByVal FDFInitialize As Boolean = False, Optional ByVal ownerPassword As String = "") As FDFDoc_Class
            If FDFInitialize Then FDFDox.Initialize(_defaultEncoding)
            FDFDox.DefaultEncoding = _defaultEncoding
            If Not PDF Is Nothing Then
                FDFDox.PDFData = PDF
            End If
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(PDF)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(PDF, _defaultEncoding.GetBytes(ownerPassword))
            End If
            Dim xfaFrm As New iTextSharp.text.pdf.XfaForm(reader)
            Dim isXFA As Boolean = False
            isXFA = xfaFrm.XfaPresent
            If isXFA Then
                reader.Close()
                reader = Nothing
                xfaFrm = Nothing
                Return parseXFA(PDF, FDFInitialize)
                Exit Function
            End If
            Dim af As iTextSharp.text.pdf.PRAcroForm
            af = reader.AcroForm
            Dim fld As iTextSharp.text.pdf.AcroFields.Item ' iTextSharp.text.pdf.PRAcroForm.FieldInformation
            Dim flds As iTextSharp.text.pdf.AcroFields = reader.AcroFields
            On Error Resume Next
            For Each fieldName As String In flds.Fields.Keys
                If InStr(fieldName, "].") > 0 Then
                    Dim fldstart As Integer = fieldName.LastIndexOf("].")
                    fieldName = fieldName.Substring(fldstart + 2, fieldName.Length - fldstart - 2)
                End If
                Dim val As String = ""
                val = flds.GetField(fieldName) & ""
                If flds.GetFieldType(fieldName) = iTextSharp.text.pdf.AcroFields.FIELD_TYPE_TEXT Or flds.GetFieldType(fieldName) = iTextSharp.text.pdf.AcroFields.FIELD_TYPE_CHECKBOX Then
                    If Not String_IsNullOrEmpty(val & "") Then
                        FDFDox.FDFAddField(fieldName, val, FDFDoc_Class.FieldType.FldTextual, True, True)
                    Else
                        FDFDox.FDFAddField(fieldName, "", FDFDoc_Class.FieldType.FldTextual, True, True)
                    End If
                ElseIf flds.GetFieldType(fieldName) = iTextSharp.text.pdf.AcroFields.FIELD_TYPE_COMBO Then
                    Dim disp() As String, vals() As String, exp() As String
                    exp = flds.GetListOptionExport(fieldName)
                    disp = flds.GetListOptionDisplay(fieldName)
                    vals = flds.GetListSelection(fieldName)
                    FDFDox.FDFAddField(fieldName, vals, disp, exp, FDFDoc_Class.FieldType.FldMultiSelect, True, False)
                ElseIf flds.GetFieldType(fieldName) = iTextSharp.text.pdf.AcroFields.FIELD_TYPE_LIST Then
                    Dim disp() As String, vals() As String, exp() As String
                    exp = flds.GetListOptionExport(fieldName)
                    disp = flds.GetListOptionDisplay(fieldName)
                    vals = flds.GetListSelection(fieldName)
                    FDFDox.FDFAddField(fieldName, vals, disp, exp, FDFDoc_Class.FieldType.FldMultiSelect, True, False)
                ElseIf flds.GetFieldType(fieldName) = iTextSharp.text.pdf.AcroFields.FIELD_TYPE_RADIOBUTTON Then
                    If Not String_IsNullOrEmpty(val & "") Then
                        FDFDox.FDFAddField(fieldName, val.TrimStart("/"c), FDFDoc_Class.FieldType.FldOption, True, True)
                    Else
                        FDFDox.FDFAddField(fieldName, "", FDFDoc_Class.FieldType.FldOption, True, True)
                    End If
                ElseIf flds.GetFieldType(fieldName) = iTextSharp.text.pdf.AcroFields.FIELD_TYPE_PUSHBUTTON Then
                    If Not String_IsNullOrEmpty(val & "") Then
                        FDFDox.FDFAddField(fieldName, val.TrimStart("/"c), FDFDoc_Class.FieldType.FldButton, True, True)
                    Else
                        FDFDox.FDFAddField(fieldName, "", FDFDoc_Class.FieldType.FldButton, True, True)
                    End If
                Else
                    If Not String_IsNullOrEmpty(val & "") Then
                        FDFDox.FDFAddField(fieldName, val, FDFDoc_Class.FieldType.FldTextual, True, True)
                    Else
                        FDFDox.FDFAddField(fieldName, "", FDFDoc_Class.FieldType.FldTextual, True, True)
                    End If
                End If
            Next
            Dim streamPDF As New MemoryStream
            Dim stp As New iTextSharp.text.pdf.PdfStamper(reader, streamPDF)
            stp.Close()
            streamPDF.Read(FDFDox.PDFData, 0, CInt(streamPDF.Length))
            FDFDox.XDPAdjustSubforms()
            FDFDox.FDFData = FDFDox.FDFSavetoStr(FDFDoc_Class.FDFType.FDF, True)
            stp = Nothing
            reader = Nothing
            xfaFrm = Nothing
            Return FDFDox
        End Function
        Private Function getAcroFields(ByVal pathDoc As String) As iTextSharp.text.pdf.AcroFields
            Dim pdfReader As New iTextSharp.text.pdf.PdfReader(pathDoc)
            Dim fields As iTextSharp.text.pdf.AcroFields = pdfReader.AcroFields
            pdfReader.Close()
            Return fields
        End Function
        Private Function getAcroFields(ByVal PDF() As Byte) As iTextSharp.text.pdf.AcroFields
            Dim pdfReader As New iTextSharp.text.pdf.PdfReader(PDF)
            Dim fields As iTextSharp.text.pdf.AcroFields = pdfReader.AcroFields
            pdfReader.Close()
            Return fields
        End Function
        Public Function getXFAFieldNames(ByVal path As String) As System.Collections.Generic.List(Of String)
            Dim Keys As New System.Collections.Generic.List(Of String) 'System.Collections.Generic.Dictionary(Of String, String)
            Select Case Determine_Type(path)
                Case FDFType.PDF
                    Dim af As iTextSharp.text.pdf.AcroFields = getAcroFields(path)
                    For Each fld As String In af.Fields.Keys
                        Keys.Add(fld) ', af.GetField(fld))
                    Next
                Case FDFType.XFA
                    Return Keys
                Case FDFType.XPDF
                    Dim af As iTextSharp.text.pdf.AcroFields = getAcroFields(path)
                    Dim xfa As iTextSharp.text.pdf.XfaForm = af.Xfa
                    If xfa.XfaPresent Then
                        Dim n As System.Xml.XmlNode = xfa.DatasetsNode.FirstChild
                        For Each f As String In xfa.DatasetsSom.Order.ToArray
                            If f.Contains("."c) Then
                                f = f.Substring(f.LastIndexOf("."c) + 1, f.Length - f.LastIndexOf("."c) - 1)
                            End If
                            Keys.Add(f)
                        Next
                    End If
                Case Else
                    Return Keys
            End Select
            Return Keys
        End Function
        Public Function getXFAFieldNames(ByVal PDF() As Byte) As System.Collections.Generic.List(Of String)
            Dim Keys As New System.Collections.Generic.List(Of String) 'System.Collections.Generic.Dictionary(Of String, String)
            Select Case Determine_Type(PDF)
                Case FDFType.PDF
                    Dim af As iTextSharp.text.pdf.AcroFields = getAcroFields(PDF)
                    For Each fld As String In af.Fields.Keys
                        Keys.Add(fld) ', af.GetField(fld))
                    Next
                Case FDFType.XFA
                    Return Keys
                Case FDFType.XPDF
                    Dim af As iTextSharp.text.pdf.AcroFields = getAcroFields(PDF)
                    Dim xfa As iTextSharp.text.pdf.XfaForm = af.Xfa
                    If xfa.XfaPresent Then
                        Dim n As System.Xml.XmlNode = xfa.DatasetsNode.FirstChild
                        For Each f As String In xfa.DatasetsSom.Order.ToArray
                            If f.Contains("."c) Then
                                f = f.Substring(f.LastIndexOf("."c) + 1, f.Length - f.LastIndexOf("."c) - 1)
                            End If
                            Keys.Add(f)
                        Next
                    End If
                Case Else
                    Return Keys
            End Select
            Return Keys
        End Function
        Public Function GetPushButtonFieldNames(ByVal PDF As Byte()) As String()
            FDFDox.DefaultEncoding = _defaultEncoding
            If Not PDF Is Nothing Then
                FDFDox.PDFData = PDF
            End If
            Dim reader As iTextSharp.text.pdf.PdfReader
            reader = New iTextSharp.text.pdf.PdfReader(PDF)
            Dim xfaFrm As New iTextSharp.text.pdf.XfaForm(reader)
            Dim isXFA As Boolean = False
            isXFA = xfaFrm.XfaPresent
            Dim af As iTextSharp.text.pdf.PRAcroForm
            af = reader.AcroForm
            Dim fld As iTextSharp.text.pdf.PRAcroForm.FieldInformation
            Dim flds As iTextSharp.text.pdf.AcroFields = reader.AcroFields
            Dim fields As ArrayList = af.Fields
            On Error Resume Next
            Dim FieldNames As New System.Collections.Generic.List(Of String)
            For Each fld In fields
                Dim fieldName As String = fld.Name
                If InStr(fieldName, "].") > 0 Then
                    Dim fldstart As Integer = fieldName.LastIndexOf("].")
                    fieldName = fieldName.Substring(fldstart + 2, fieldName.Length - fldstart - 2)
                End If
                Dim val As String = ""
                val = flds.GetField(fieldName) & ""
                If flds.GetFieldType(fieldName) = iTextSharp.text.pdf.AcroFields.FIELD_TYPE_PUSHBUTTON Then
                    FieldNames.Add(fieldName)
                End If
            Next
            Return FieldNames.ToArray
        End Function
        Private Function parsePDF(ByVal FileNameorURL As String, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal ownerPassword As String = "") As FDFDoc_Class
            If FDFInitialize Then FDFDox.Initialize(_defaultEncoding)
            FDFDox.DefaultEncoding = _defaultEncoding
            If IsValidUrl(FileNameorURL) Then
                Dim wClient As New Net.WebClient
                Dim strPDF As New MemoryStream
                FDFDox.PDFData = wClient.DownloadData(FileNameorURL)
            ElseIf File.Exists(FileNameorURL) Then
                Dim fs As New FileStream(FileNameorURL, FileMode.Open)
                ReDim FDFDox.PDFData(CInt(fs.Length))
                fs.Read(FDFDox.PDFData, 0, CInt(fs.Length))
                fs.Close()
            Else
                Return Nothing
            End If
            Dim reader As iTextSharp.text.pdf.PdfReader
            If Not FDFDox.PDFData Is Nothing Then
                If String_IsNullOrEmpty(ownerPassword) Then
                    reader = New iTextSharp.text.pdf.PdfReader(FDFDox.PDFData)
                Else
                    reader = New iTextSharp.text.pdf.PdfReader(FDFDox.PDFData, _defaultEncoding.GetBytes(ownerPassword))
                End If
            Else
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & "PDF File Empty", "FDFApp.ParsePDF", 1)
                Return Nothing
                Exit Function
            End If
            Dim xfaFrm As New iTextSharp.text.pdf.XfaForm(reader)
            Dim isXFA As Boolean = False
            isXFA = xfaFrm.XfaPresent
            If isXFA Then
                reader.Close()
                reader = Nothing
                xfaFrm = Nothing
                Return parseXFA(FileNameorURL, FDFInitialize)
                Exit Function
            Else
                FDFDox.FDFSetFile(FileNameorURL)
            End If
            Dim af As iTextSharp.text.pdf.PRAcroForm
            af = reader.AcroForm
            Dim fld As iTextSharp.text.pdf.AcroFields.Item ' iTextSharp.text.pdf.PRAcroForm.FieldInformation
            Dim flds As iTextSharp.text.pdf.AcroFields = reader.AcroFields
            On Error Resume Next
            For Each fieldName As String In flds.Fields.Keys
                If InStr(fieldName, "].") > 0 Then
                    Dim fldstart As Integer = fieldName.LastIndexOf("].")
                    fieldName = fieldName.Substring(fldstart + 2, fieldName.Length - fldstart - 2)
                End If
                Dim val As String = ""
                val = flds.GetField(fieldName) & ""
                If flds.GetFieldType(fieldName) = iTextSharp.text.pdf.AcroFields.FIELD_TYPE_TEXT Or flds.GetFieldType(fieldName) = iTextSharp.text.pdf.AcroFields.FIELD_TYPE_CHECKBOX Then
                    If Not String_IsNullOrEmpty(val & "") Then
                        FDFDox.FDFAddField(fieldName, val, FDFDoc_Class.FieldType.FldTextual, True, True)
                    Else
                        FDFDox.FDFAddField(fieldName, "", FDFDoc_Class.FieldType.FldTextual, True, True)
                    End If
                ElseIf flds.GetFieldType(fieldName) = iTextSharp.text.pdf.AcroFields.FIELD_TYPE_COMBO Then
                    Dim disp() As String, vals() As String, exp() As String
                    exp = flds.GetListOptionExport(fieldName)
                    disp = flds.GetListOptionDisplay(fieldName)
                    vals = flds.GetListSelection(fieldName)
                    FDFDox.FDFAddField(fieldName, vals, disp, exp, FDFDoc_Class.FieldType.FldMultiSelect, True, False)
                ElseIf flds.GetFieldType(fieldName) = iTextSharp.text.pdf.AcroFields.FIELD_TYPE_LIST Then
                    Dim disp() As String, vals() As String, exp() As String
                    exp = flds.GetListOptionExport(fieldName)
                    disp = flds.GetListOptionDisplay(fieldName)
                    vals = flds.GetListSelection(fieldName)
                    FDFDox.FDFAddField(fieldName, vals, disp, exp, FDFDoc_Class.FieldType.FldMultiSelect, True, False)
                ElseIf flds.GetFieldType(fieldName) = iTextSharp.text.pdf.AcroFields.FIELD_TYPE_RADIOBUTTON Then
                    If Not String_IsNullOrEmpty(val & "") Then
                        FDFDox.FDFAddField(fieldName, val.TrimStart("/"c), FDFDoc_Class.FieldType.FldOption, True, True)
                    Else
                        FDFDox.FDFAddField(fieldName, "", FDFDoc_Class.FieldType.FldOption, True, True)
                    End If
                ElseIf flds.GetFieldType(fieldName) = iTextSharp.text.pdf.AcroFields.FIELD_TYPE_PUSHBUTTON Then
                    If Not String_IsNullOrEmpty(val & "") Then
                        FDFDox.FDFAddField(fieldName, val.TrimStart("/"c), FDFDoc_Class.FieldType.FldButton, True, True)
                    Else
                        FDFDox.FDFAddField(fieldName, "", FDFDoc_Class.FieldType.FldButton, True, True)
                    End If
                Else
                    If Not String_IsNullOrEmpty(val & "") Then
                        FDFDox.FDFAddField(fieldName, val, FDFDoc_Class.FieldType.FldTextual, True, True)
                    Else
                        FDFDox.FDFAddField(fieldName, "", FDFDoc_Class.FieldType.FldTextual, True, True)
                    End If
                End If
            Next
            Dim streamPDF As New MemoryStream
            Dim stp As New iTextSharp.text.pdf.PdfStamper(reader, streamPDF)
            stp.Close()
            streamPDF.Read(FDFDox.PDFData, 0, CInt(streamPDF.Length))
            FDFDox.XDPAdjustSubforms()
            FDFDox.FDFData = FDFDox.FDFSavetoStr(FDFDoc_Class.FDFType.FDF, True)
            FDFDox.FDFSetFile(FileNameorURL)
            stp = Nothing
            reader = Nothing
            xfaFrm = Nothing
            Return FDFDox
        End Function
        Private Function parsePDF(ByVal PDFStream As Stream, Optional ByVal FDFInitialize As Boolean = False, Optional ByVal ownerPassword As String = "") As FDFDoc_Class
            If FDFInitialize Then FDFDox.Initialize(_defaultEncoding)
            FDFDox.DefaultEncoding = _defaultEncoding
            Dim reader As iTextSharp.text.pdf.PdfReader
            If String_IsNullOrEmpty(ownerPassword) Then
                reader = New iTextSharp.text.pdf.PdfReader(PDFStream)
            Else
                reader = New iTextSharp.text.pdf.PdfReader(PDFStream, _defaultEncoding.GetBytes(ownerPassword))
            End If
            If PDFStream.Length > 0 Then
                If PDFStream.CanSeek Then
                    PDFStream.Position = 0
                End If
                PDFStream.Read(FDFDox.PDFData, 0, FDFDox.PDFData.Length)
            End If
            Dim xfaFrm As New iTextSharp.text.pdf.XfaForm(reader)
            Dim isXFA As Boolean = False
            isXFA = xfaFrm.XfaPresent
            If isXFA Then
                reader.Close()
                reader = Nothing
                xfaFrm = Nothing
                Return parseXFA(PDFStream, FDFInitialize)
                Exit Function
            End If
            Dim af As iTextSharp.text.pdf.PRAcroForm
            af = reader.AcroForm
            Dim fld As iTextSharp.text.pdf.AcroFields.Item ' iTextSharp.text.pdf.PRAcroForm.FieldInformation
            Dim flds As iTextSharp.text.pdf.AcroFields = reader.AcroFields
            On Error Resume Next
            For Each fieldName As String In flds.Fields.Keys
                If InStr(fieldName, "].") > 0 Then
                    Dim fldstart As Integer = fieldName.LastIndexOf("].")
                    fieldName = fieldName.Substring(fldstart + 2, fieldName.Length - fldstart - 2)
                End If
                Dim val As String = ""
                val = flds.GetField(fieldName) & ""
                If flds.GetFieldType(fieldName) = iTextSharp.text.pdf.AcroFields.FIELD_TYPE_TEXT Or flds.GetFieldType(fieldName) = iTextSharp.text.pdf.AcroFields.FIELD_TYPE_CHECKBOX Then
                    If Not String_IsNullOrEmpty(val & "") Then
                        FDFDox.FDFAddField(fieldName, val, FDFDoc_Class.FieldType.FldTextual, True, True)
                    Else
                        FDFDox.FDFAddField(fieldName, "", FDFDoc_Class.FieldType.FldTextual, True, True)
                    End If
                ElseIf flds.GetFieldType(fieldName) = iTextSharp.text.pdf.AcroFields.FIELD_TYPE_COMBO Then
                    Dim disp() As String, vals() As String, exp() As String
                    exp = flds.GetListOptionExport(fieldName)
                    disp = flds.GetListOptionDisplay(fieldName)
                    vals = flds.GetListSelection(fieldName)
                    FDFDox.FDFAddField(fieldName, vals, disp, exp, FDFDoc_Class.FieldType.FldMultiSelect, True, False)
                ElseIf flds.GetFieldType(fieldName) = iTextSharp.text.pdf.AcroFields.FIELD_TYPE_LIST Then
                    Dim disp() As String, vals() As String, exp() As String
                    exp = flds.GetListOptionExport(fieldName)
                    disp = flds.GetListOptionDisplay(fieldName)
                    vals = flds.GetListSelection(fieldName)
                    FDFDox.FDFAddField(fieldName, vals, disp, exp, FDFDoc_Class.FieldType.FldMultiSelect, True, False)
                ElseIf flds.GetFieldType(fieldName) = iTextSharp.text.pdf.AcroFields.FIELD_TYPE_RADIOBUTTON Then
                    If Not String_IsNullOrEmpty(val & "") Then
                        FDFDox.FDFAddField(fieldName, val.TrimStart("/"c), FDFDoc_Class.FieldType.FldOption, True, True)
                    Else
                        FDFDox.FDFAddField(fieldName, "", FDFDoc_Class.FieldType.FldOption, True, True)
                    End If
                ElseIf flds.GetFieldType(fieldName) = iTextSharp.text.pdf.AcroFields.FIELD_TYPE_PUSHBUTTON Then
                    If Not String_IsNullOrEmpty(val & "") Then
                        FDFDox.FDFAddField(fieldName, val.TrimStart("/"c), FDFDoc_Class.FieldType.FldButton, True, True)
                    Else
                        FDFDox.FDFAddField(fieldName, "", FDFDoc_Class.FieldType.FldButton, True, True)
                    End If
                Else
                    If Not String_IsNullOrEmpty(val & "") Then
                        FDFDox.FDFAddField(fieldName, val, FDFDoc_Class.FieldType.FldTextual, True, True)
                    Else
                        FDFDox.FDFAddField(fieldName, "", FDFDoc_Class.FieldType.FldTextual, True, True)
                    End If
                End If
            Next
            Dim streamPDF As New MemoryStream
            Dim stp As New iTextSharp.text.pdf.PdfStamper(reader, streamPDF)
            stp.Close()
            streamPDF.Read(FDFDox.PDFData, 0, CInt(streamPDF.Length))
            FDFDox.FDFData = FDFDox.FDFSavetoStr(FDFDoc_Class.FDFType.FDF, True)
            stp = Nothing
            reader = Nothing
            xfaFrm = Nothing
            Return FDFDox
        End Function
        ''' <summary>
        ''' GOOD VERSION
        ''' </summary>
        ''' <param name="FDF"></param>
        ''' <param name="PDFFileName"></param>
        ''' <param name="FDFInitialize"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function parseXML(ByVal FDF As String, ByVal PDFFileName As String, Optional ByVal FDFInitialize As Boolean = False) As FDFDoc_Class
            If FDFInitialize Then FDFDox.Initialize(_defaultEncoding)
            If String_IsNullOrEmpty(PDFFileName) Then
                If String_IsNullOrEmpty(FDFDox.XDPGetFile) Then
                    PDFFileName = FDFDox.XDPGetFile & ""
                End If
            Else
                FDFDox.FDFSetFile(PDFFileName)
            End If
            FDFDox.DefaultEncoding = _defaultEncoding
            Dim memFDF As New MemoryStream
            Dim previousFieldAdded As String = ""
            Dim _f As New FDFDoc_Class.FDFDoc_Class
            Try
                FDF = FDF.Replace("<xfa:", "<")
                FDF = FDF.Replace("</xfa:", "</")
                FDF = FDF.Replace("xfa:", "")
                FDF = FDF.Replace(":xfa", "")
                FDF = FDF.Replace(Chr(10) & ">", ">")
                FDF = FDF.Replace(Chr(10) & "/>", "/>")
                FDF = FDF.Replace(Chr(13) & ">", ">")
                FDF = FDF.Replace(Chr(13) & "/>", "/>")
                FDF = FDF.Replace(Environment.NewLine & ">", ">")
                FDF = FDF.Replace(Environment.NewLine & "/>", "/>")
                FDF = FDF.Replace(" xmlns=""http://www.xfa.org/schema/xfa-data/1.0/""", "")
                Dim strFDF As New StringReader(FDF)
                Dim ds As New DataSet
                FDFDox.DefaultEncoding = _defaultEncoding
                Dim Name As String = "", Value As String = ""
                Dim FormName As String = "", ContentType As String = "", Href As String = ""
                Dim PreviousSubFormDepth As Integer = 0, PreviousSubFormName As String = "", SubFormNames As String() = {}, CurDepth As Integer = 0
                Dim _xml As New XmlDocument
                FDF = FDF.Replace(" xmlns=""http://www.xfa.org/schema/xfa-data/1.0/""", "")
                _xml.PreserveWhitespace = True
                _xml.LoadXml(FDF.Trim())
                FormName = _xml.Name
                ReDim Preserve SubFormNames(CurDepth)
                SubFormNames(CurDepth) = FormName
                Dim subFormLevel As String = String.Join("/", getParentLevel(_xml))
                If _xml.HasChildNodes Then
                    If _xml.ChildNodes.Count > 0 Then
                        For Each chld As XmlNode In _xml.ChildNodes
                            Try
                                If chld.HasChildNodes Then
                                    _f = New FDFDoc_Class.FDFDoc_Class
                                    _f.FormName = chld.Name
                                    _f.DocType = FDFDoc_Class.FDFDocType.XDPForm
                                    _f.FormLevel = String.Join("/", getParentLevel(chld))
                                    _f.FormLevel = _f.FormLevel.TrimStart("/"c)
                                    _f.FormLevel = _f.FormLevel.TrimEnd("/"c)
                                    _f.FormLevel = _f.FormLevel.Replace("//", "/")
                                    _f.FileName = FDFDox.FDFGetFile()
                                    _f.struc_FDFFields.AddRange(parseXMLChildItems(chld))
                                    FDFDox.XDPAddField(_f, _f.FormName, _f.FormLevel.ToString.Split(("/"c)), True, False)
                                End If
                            Catch ex As Exception
                                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, ex)
                                Err.Clear()
                            End Try
                            PreviousSubFormDepth = CurDepth
                            PreviousSubFormName = FormName
                            CurDepth += 1
                        Next
                        PreviousSubFormDepth = CurDepth
                        PreviousSubFormName = FormName
                        CurDepth += 1
                    End If
                End If
FOUNDIMAGE:
                FDFDox.XDPAdjustSubforms()
                Return FDFDox
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, Ex)
                Return Nothing
            End Try
        End Function
        Private Function getParentFormNames(ByVal formNames() As String) As String()
            Dim lst As New System.Collections.Generic.List(Of String)
            Try
                lst.AddRange(formNames)
                If lst.Count > 1 Then
                    lst.RemoveAt(lst.Count - 1)
                    Return lst.ToArray
                Else
                    Return New String() {""}
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, ex)
                Err.Clear()
            End Try
            Return formNames
        End Function
        '        Private Function parseXMLChildItems(ByVal _xml As XmlNode) As FDFApp.FDFDoc_Class.FDFField()
        '            Dim PDFFileName As String = ""
        '            Dim memFDF As New MemoryStream
        '            Dim previousFieldAdded As String = ""
        '            Dim _f As New FDFDoc_Class.FDFDoc_Class
        '            Dim Name As String = "", Value As String = ""
        '            Dim FormName As String = "", ContentType As String = "", Href As String = "", ImageFieldStr As String
        '            Dim PreviousSubFormDepth As Integer = 0, PreviousSubFormName As String = "", SubFormNames As String() = {}, CurDepth As Integer = 0
        '            Dim flds As New System.Collections.Generic.List(Of FDFApp.FDFDoc_Class.FDFField)
        '            For Each c As Xml.XmlNode In _xml.ChildNodes
        '                Select Case c.NodeType
        '                    Case XmlNodeType.Text
        '                        If Not String_IsNullOrEmpty(c.ParentNode.Name) And c.HasChildNodes = False Then
        '                            Name = c.ParentNode.Name
        '                            If Not String_IsNullOrEmpty(Name & "") Then
        '                                Value = c.Value & ""
        '                                flds.Add(New FDFDoc_Class.FDFField(Name & "", XMLCheckCharReverse(Value & ""), FDFDoc_Class.FieldType.FldTextual))
        '                                previousFieldAdded = Name
        '                                Value = ""
        '                                Name = ""
        '                            End If
        '                        End If
        '                    Case XmlNodeType.Element
        '                        Name = c.Name
        '                        If c.Attributes.Count > 0 Then
        '                            For Each a As XmlAttribute In c.Attributes
        '                                Select Case a.Name.ToLower
        '                                    Case "root"
        '                                    Case "version=""1.0"" encoding=""UTF-8"
        '                                    Case "version"
        '                                    Case "standalone"
        '                                    Case "xml"
        '                                    Case "xmlns"
        '                                    Case "encoding"
        '                                    Case "pdf"
        '                                    Case "xmlns:xdp"
        '                                    Case "xmlns:xfa"
        '                                    Case "xmlns"
        '                                    Case "xml:space"
        '                                    Case "xmlns"
        '                                    Case "contenttype"
        '                                        ContentType = a.Value & ""
        '                                        ImageFieldStr = c.InnerText & ""
        '                                        If ImageFieldStr.Length > 0 Then
        '                                            flds.Add(New FDFDoc_Class.FDFField(Name & "", (ContentType), FDFDoc_Class.FieldType.FldLiveCycleImage, ImageFieldStr & ""))
        '                                            Name = ""
        '                                        End If
        '                                        GoTo FOUNDIMAGE
        '                                    Case "href"
        '                                        Href = a.Value & ""
        '                                        FDFDox.FDFSetFile(Href)
        '                                    Case Else
        '                                End Select
        '                            Next
        '                        End If
        '                        FormName = c.Name
        '                        ReDim Preserve SubFormNames(CurDepth)
        '                        SubFormNames(CurDepth) = FormName
        '                        Dim subFormLevel As String = String.Join("/", getParentLevel(c))
        '                        If c.HasChildNodes Then
        '                            If Not c.FirstChild.NodeType = XmlNodeType.Text Then
        '                                Try
        '                                    Dim _fld As New FDFApp.FDFDoc_Class.FDFField
        '                                    _fld.FieldType = FDFDoc_Class.FieldType.FldSubform
        '                                    _fld.FieldEnabled = True
        '                                    _fld.FieldName = SubFormNames(SubFormNames.Length - 1)
        '                                    _fld.XDPAppendSubform(_fld.FieldName, String.Join("/", getParentLevel(c)).ToString.TrimStart("/"c).TrimEnd("/"c).Replace("//", "/"), FDFDox.FDFGetFile(), parseXMLChildItems(c))
        '                                    flds.Add(_fld)
        '                                Catch ex As Exception
        '                                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, ex)
        '                                    Err.Clear()
        '                                End Try
        '                            Else
        '                                Name = c.Name
        '                                If Not String_IsNullOrEmpty(Name & "") Then
        '                                    Value = c.FirstChild.Value & ""
        '                                    flds.Add(New FDFDoc_Class.FDFField(Name & "", XMLCheckCharReverse(Value & ""), FDFDoc_Class.FieldType.FldTextual))
        '                                    previousFieldAdded = Name
        '                                    Value = ""
        '                                    Name = ""
        '                                End If
        '                            End If
        '                            PreviousSubFormDepth = CurDepth
        '                            PreviousSubFormName = FormName
        '                            CurDepth += 1
        '                        Else
        '                            Name = c.Name
        '                            If Not String_IsNullOrEmpty(Name & "") Then
        '                                Value = c.InnerText & ""
        '                                flds.Add(New FDFDoc_Class.FDFField(Name & "", XMLCheckCharReverse(Value & ""), FDFDoc_Class.FieldType.FldTextual))
        '                                previousFieldAdded = Name
        '                                Value = ""
        '                                Name = ""
        '                            End If
        '                        End If
        '                    Case XmlNodeType.EndElement
        '                        CurDepth = CurDepth - 1
        '                        PreviousSubFormDepth = CurDepth
        '                        PreviousSubFormName = FormName
        '                        If CurDepth < 0 Then
        '                            ReDim Preserve SubFormNames(0)
        '                            FormName = ""
        '                        ElseIf CurDepth = 0 Then
        '                            ReDim Preserve SubFormNames(0)
        '                            FormName = SubFormNames(0)
        '                        Else
        '                            ReDim Preserve SubFormNames(CurDepth)
        '                            FormName = SubFormNames(CurDepth)
        '                        End If
        '                    Case XmlNodeType.Whitespace
        '                    Case Else
        '                End Select
        '                Dim tmpFormName As String = c.Name
        '                ReDim Preserve SubFormNames(CurDepth)
        '                SubFormNames(CurDepth) = tmpFormName
        'FOUNDIMAGE:
        '            Next
        '            Return flds.ToArray
        '        End Function
        Private Function parseXMLChildItems(ByVal _xml As XmlNode) As FDFApp.FDFDoc_Class.FDFField()
            Dim PDFFileName As String = ""
            Dim memFDF As New MemoryStream
            Dim previousFieldAdded As String = ""
            Dim _f As New FDFDoc_Class.FDFDoc_Class
            Dim Name As String = "", Value As String = ""
            Dim FormName As String = "", ContentType As String = "", Href As String = "", ImageFieldStr As String
            Dim PreviousSubFormDepth As Integer = 0, PreviousSubFormName As String = "", SubFormNames As String() = {}, CurDepth As Integer = 0
            Dim flds As New System.Collections.Generic.List(Of FDFApp.FDFDoc_Class.FDFField)
            For Each c As Xml.XmlNode In _xml.ChildNodes
                Select Case c.NodeType
                    Case XmlNodeType.Text
                        If Not String_IsNullOrEmpty(c.ParentNode.Name) And c.HasChildNodes = False Then
                            Name = c.ParentNode.Name
                            If Not String_IsNullOrEmpty(Name & "") Then
                                Value = c.Value & ""
                                flds.Add(New FDFDoc_Class.FDFField(Name & "", XMLCheckCharReverse(Value & ""), FDFDoc_Class.FieldType.FldTextual))
                                previousFieldAdded = Name
                                Value = ""
                                Name = ""
                            End If
                        End If
                    Case XmlNodeType.Element
                        Name = c.Name
                        If c.Attributes.Count > 0 Then
                            For Each a As XmlAttribute In c.Attributes
                                Select Case a.Name.ToLower
                                    Case "root"
                                    Case "version=""1.0"" encoding=""UTF-8"
                                    Case "version"
                                    Case "standalone"
                                    Case "xml"
                                    Case "xmlns"
                                    Case "encoding"
                                    Case "pdf"
                                    Case "xmlns:xdp"
                                    Case "xmlns:xfa"
                                    Case "xmlns"
                                    Case "xml:space"
                                    Case "xmlns"
                                    Case "contenttype"
                                        ContentType = a.Value & ""
                                        ImageFieldStr = c.InnerText & ""
                                        If ImageFieldStr.Length > 0 Then
                                            flds.Add(New FDFDoc_Class.FDFField(Name & "", (ContentType), FDFDoc_Class.FieldType.FldLiveCycleImage, ImageFieldStr & ""))
                                            Name = ""
                                        End If
                                        GoTo FOUNDIMAGE
                                    Case "href"
                                        Href = a.Value & ""
                                        FDFDox.FDFSetFile(Href)
                                    Case Else
                                End Select
                            Next
                        End If
                        FormName = c.Name
                        ReDim Preserve SubFormNames(CurDepth)
                        SubFormNames(CurDepth) = FormName
                        Dim subFormLevel As String = String.Join("/", getParentLevel(c))
                        If c.HasChildNodes Then
                            If Not c.FirstChild.NodeType = XmlNodeType.Text Then
                                Try
                                    Dim _fld As New FDFApp.FDFDoc_Class.FDFField
                                    _fld.FieldType = FDFDoc_Class.FieldType.FldSubform
                                    _fld.FieldEnabled = True
                                    _fld.FieldName = SubFormNames(SubFormNames.Length - 1)
                                    Dim vals As New System.Collections.Generic.List(Of String), blnValues As Boolean = False
                                    For Each chldItem As XmlNode In c.ChildNodes
                                        If chldItem.Name.ToString.ToLower = "value".ToLower Then
                                            blnValues = True
                                            vals.Add(chldItem.InnerText)
                                        Else
                                            blnValues = False
                                        End If
                                    Next
                                    If blnValues And vals.Count > 0 Then
                                        '_fld.FieldValue.AddRange(vals.ToArray)
                                        flds.Add(New FDFDoc_Class.FDFField(Name & "", vals.ToArray, FDFDoc_Class.FieldType.FldTextual))
                                        previousFieldAdded = Name
                                        Value = ""
                                        Name = ""
                                    Else
                                        _fld.XDPAppendSubform(_fld.FieldName, String.Join("/", getParentLevel(c)).ToString.TrimStart("/"c).TrimEnd("/"c).Replace("//", "/"), FDFDox.FDFGetFile(), parseXMLChildItems(c))
                                        flds.Add(_fld)
                                    End If
                                Catch ex As Exception
                                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, ex)
                                    Err.Clear()
                                End Try
                            Else
                                Name = c.Name
                                If Name = "value" Then
                                    Name = c.ParentNode.Name
                                End If
                                If Not String_IsNullOrEmpty(Name & "") Then
                                    Value = c.FirstChild.Value & ""
                                    flds.Add(New FDFDoc_Class.FDFField(Name & "", XMLCheckCharReverse(Value & ""), FDFDoc_Class.FieldType.FldTextual))
                                    previousFieldAdded = Name
                                    Value = ""
                                    Name = ""
                                End If
                            End If
                            PreviousSubFormDepth = CurDepth
                            PreviousSubFormName = FormName
                            CurDepth += 1
                        Else
                            Name = c.Name
                            If Not String_IsNullOrEmpty(Name & "") Then
                                Value = c.InnerText & ""
                                flds.Add(New FDFDoc_Class.FDFField(Name & "", XMLCheckCharReverse(Value & ""), FDFDoc_Class.FieldType.FldTextual))
                                previousFieldAdded = Name
                                Value = ""
                                Name = ""
                            End If
                        End If
                    Case XmlNodeType.EndElement
                        CurDepth = CurDepth - 1
                        PreviousSubFormDepth = CurDepth
                        PreviousSubFormName = FormName
                        If CurDepth < 0 Then
                            ReDim Preserve SubFormNames(0)
                            FormName = ""
                        ElseIf CurDepth = 0 Then
                            ReDim Preserve SubFormNames(0)
                            FormName = SubFormNames(0)
                        Else
                            ReDim Preserve SubFormNames(CurDepth)
                            FormName = SubFormNames(CurDepth)
                        End If
                    Case XmlNodeType.Whitespace
                    Case Else
                End Select
                Dim tmpFormName As String = c.Name
                ReDim Preserve SubFormNames(CurDepth)
                SubFormNames(CurDepth) = tmpFormName
FOUNDIMAGE:
            Next
            Return flds.ToArray
        End Function
        Function getParentLevel(ByVal _xmlNode As XmlNode) As String()
            Dim p As XmlNode = _xmlNode, lstParents As New System.Collections.Generic.List(Of String)
            If Not p.ParentNode Is Nothing Then
                Do While Not p.ParentNode Is Nothing
                    Try
                        lstParents.Insert(0, p.Name)
                        If Not p.ParentNode Is Nothing Then
                            p = p.ParentNode
                        Else
                            Exit Do
                        End If
                    Catch ex As Exception
                        Exit Do
                    End Try
                Loop
                Return lstParents.ToArray
            End If
            Return New String() {}
        End Function
        Private Function parseXML(ByVal FDF As String, Optional ByVal FDFInitialize As Boolean = False) As FDFDoc_Class
            If FDFInitialize Then FDFDox.Initialize(_defaultEncoding)
            FDFDox.DefaultEncoding = _defaultEncoding
            Dim PDFFileName As String = ""
            Dim memFDF As New MemoryStream
            Dim previousFieldAdded As String = ""
            Dim _f As New FDFDoc_Class.FDFDoc_Class
            Try
                FDF = FDF.Replace("<xfa:", "<")
                FDF = FDF.Replace("</xfa:", "</")
                FDF = FDF.Replace("xfa:", "")
                FDF = FDF.Replace(":xfa", "")
                FDF = FDF.Replace(Chr(10) & ">", ">")
                FDF = FDF.Replace(Chr(10) & "/>", "/>")
                FDF = FDF.Replace(Chr(13) & ">", ">")
                FDF = FDF.Replace(Chr(13) & "/>", "/>")
                FDF = FDF.Replace(Environment.NewLine & ">", ">")
                FDF = FDF.Replace(Environment.NewLine & "/>", "/>")
                Dim strFDF As New StringReader(FDF)
                Dim ds As New DataSet
                FDFDox.DefaultEncoding = _defaultEncoding
                Dim Name As String = "", Value As String = ""
                Dim FormName As String = "", ContentType As String = "", Href As String = ""
                Dim PreviousSubFormDepth As Integer = 0, PreviousSubFormName As String = "", SubFormNames As String() = {}, CurDepth As Integer = 0
                Dim _xml As New XmlDocument
                FDF = FDF.Replace(" xmlns=""http://www.xfa.org/schema/xfa-data/1.0/""", "")
                _xml.PreserveWhitespace = True
                _xml.LoadXml(FDF.Trim())
                FormName = _xml.Name
                ReDim Preserve SubFormNames(CurDepth)
                SubFormNames(CurDepth) = FormName
                Dim subFormLevel As String = String.Join("/", getParentLevel(_xml))
                If _xml.HasChildNodes Then
                    If _xml.ChildNodes.Count > 0 Then
                        For Each chld As XmlNode In _xml.ChildNodes
                            Try
                                If chld.HasChildNodes Then
                                    _f = New FDFDoc_Class.FDFDoc_Class
                                    _f.FormName = chld.Name
                                    _f.DocType = FDFDoc_Class.FDFDocType.XDPForm
                                    _f.FormLevel = String.Join("/", getParentLevel(chld))
                                    _f.FormLevel = _f.FormLevel.TrimStart("/"c)
                                    _f.FormLevel = _f.FormLevel.TrimEnd("/"c)
                                    _f.FormLevel = _f.FormLevel.Replace("//", "/")
                                    _f.FileName = FDFDox.FDFGetFile()
                                    _f.XDPAppendFields(parseXMLChildItems(chld))
                                    FDFDox.XDPAddField(_f, _f.FormName, _f.FormLevel.ToString.Split(("/"c)), True, False)
                                End If
                            Catch ex As Exception
                                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, ex)
                                Err.Clear()
                            End Try
                            PreviousSubFormDepth = CurDepth
                            PreviousSubFormName = FormName
                            CurDepth += 1
                        Next
                        PreviousSubFormDepth = CurDepth
                        PreviousSubFormName = FormName
                        CurDepth += 1
                    End If
                End If
FOUNDIMAGE:
                FDFDox.XDPAdjustSubforms()
                Return FDFDox
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, Ex)
                Return Nothing
            End Try
        End Function
        Private Function parseXFDF(ByVal FDF As String, Optional ByVal FDFInitialize As Boolean = False) As FDFDoc_Class
            If FDFInitialize Then FDFDox.Initialize(_defaultEncoding)
            FDFDox.DefaultEncoding = _defaultEncoding
            Dim strFields(2) As String
            Dim FldStart As Integer, FldEnd As Integer
            Dim intField(7) As Integer, MultiCntr As Integer
            FDF = ByteArrayToString(_defaultEncoding.GetBytes(FDF))
            Try
                FDF = FDF.Replace(Chr(10), "")
                FDF = FDF.Replace(Chr(13), "")
                FldStart = FDF.ToLower.IndexOf("<fields") + 8
                FldEnd = FDF.ToLower.IndexOf("</fields>", FldStart)
                intField(0) = FldStart
                Do While FldEnd > intField(0) + 1
                    intField(1) = FDF.IndexOf("<field name=""", intField(0)) + 13
                    intField(2) = FDF.IndexOf(""">", intField(1))
                    Dim icntr As Integer = FDF.IndexOf(CStr("""/>".ToString()), intField(1))
                    If ((intField(2) > icntr And icntr > 0) Or intField(2) = -1) Then
                        intField(2) = FDF.IndexOf("""/>", intField(1))
                        intField(0) = intField(2)
                    Else
                        strFields(1) = ""
                        MultiCntr = 1
IsMultiSelect:
                        intField(3) = FDF.IndexOf("<value>", intField(2)) + 7
                        intField(4) = FDF.IndexOf("</value>", intField(3))
                        If intField(0) < FDF.IndexOf("</field>", intField(4) + 7) Then
                            intField(0) = FDF.IndexOf("</field>", intField(4) + 7)
                        Else
                            intField(0) = FldEnd + 10
                            Exit Do
                        End If
                        If intField(3) < FDF.IndexOf("<value>", intField(4), 20) Then
                            strFields(1) = strFields(1) & CStr(IIf(MultiCntr > 1, "<value>", "")) & FDF.Substring(intField(3), intField(4) - intField(3)) & "</value>"
                            GoTo IsMultiSelect
                        Else
                            strFields(1) = FDF.Substring(intField(3), intField(4) - intField(3))
                        End If
                        strFields(0) = FDF.Substring(intField(1), intField(2) - intField(1))
                        If strFields(0) <> "" Then
                            If MultiCntr > 1 Then
                                strFields(1) = strFields(1).Replace("<value>", "(")
                                strFields(1) = strFields(1).Replace("</value>", ")")
                                FDFDox.FDFAddField(CStr(strFields(0)), CStr(XMLCheckCharReverse(strFields(1))), FDFDoc_Class.FieldType.FldMultiSelect)
                            Else
                                FDFDox.FDFAddField(CStr(strFields(0)), CStr(XMLCheckCharReverse(strFields(1))), FDFDoc_Class.FieldType.FldTextual)
                            End If
                        End If
                    End If
                    If intField(0) < 0 Then
                        intField(0) = FldEnd + 10
                        Exit Do
                    End If
                Loop
                FDFDox.FDFData = FDF
                Try
                    intField(1) = FDF.IndexOf("<f href=""") + 9
                    intField(2) = FDF.IndexOf("""/>", intField(1))
                    If intField(1) > 8 And intField(2) >= 0 Then
                        strFields(0) = FDF.Substring(intField(1), intField(2) - intField(1)) & ""
                        FDFDox.FDFSetFile(strFields(0) & "")
                    End If
                Catch exFile As Exception
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, exFile)
                    Err.Clear()
                End Try
                FDFDox.XDPAdjustSubforms()
                Return FDFDox
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.parseXFDF", 1)
                Return FDFDox
            End Try
        End Function
#End Region
        ''' <summary>
        ''' Adds a Submit Button to Existing PDF Form
        ''' </summary>
        ''' <param name="PDFForm">Byte Array containing Existing PDF Form to add a button to</param>
        ''' <param name="btnSubmitURL">URL the button submits to</param>
        ''' <param name="btnTop">Button's Top Dimension - Page location in pixels (0 being top of page)</param>
        ''' <param name="btnRight">Button's Right Dimension - Page location in pixels (0 being left side of page)</param>
        ''' <param name="btnBottom">Button's Dimension - Page location in pixels (0 being top of page)</param>
        ''' <param name="btnLeft">Button's Left Dimension - Page location in pixels (0 being left side of page)</param>
        ''' <param name="btnText">Visible Text of Button</param>
        ''' <param name="btnName">Button Field Name</param>
        ''' <param name="btnPage">Page of button to be located</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function Add_Submit_Button(ByVal PDFForm() As Byte, ByVal btnSubmitURL As String, Optional ByVal btnTop As Integer = 0, Optional ByVal btnRight As Integer = 50, Optional ByVal btnBottom As Integer = 15, Optional ByVal btnLeft As Integer = 0, Optional ByVal btnText As String = "Submit", Optional ByVal btnName As String = "btnSubmit", Optional ByVal btnPage As Integer = 1) As Byte()
            Dim pdfReader As New iTextSharp.text.pdf.PdfReader(PDFForm)
            Dim memStream As New MemoryStream
            Dim pdfStamp As New iTextSharp.text.pdf.PdfStamper(pdfReader, memStream)
            Dim submitBtn As New iTextSharp.text.pdf.PushbuttonField(pdfStamp.Writer, New iTextSharp.text.Rectangle(btnLeft, btnTop, btnRight, btnBottom), btnName)
            submitBtn.TextColor = iTextSharp.text.Color.BLACK
            submitBtn.BackgroundColor = iTextSharp.text.Color.WHITE
            submitBtn.BorderStyle = iTextSharp.text.pdf.PdfBorderDictionary.STYLE_BEVELED
            submitBtn.BorderColor = iTextSharp.text.Color.GRAY
            submitBtn.Text = btnText
            submitBtn.Alignment = iTextSharp.text.pdf.PdfAppearance.ALIGN_CENTER
            submitBtn.Options = iTextSharp.text.pdf.PushbuttonField.VISIBLE_BUT_DOES_NOT_PRINT
            Dim submitField As iTextSharp.text.pdf.PdfFormField = submitBtn.Field
            submitField.Action = iTextSharp.text.pdf.PdfAction.CreateSubmitForm(btnSubmitURL, Nothing, Nothing)
            submitField.Page = btnPage
            pdfStamp.Writer.AcroForm.AddFormField(submitField)
            Dim bytes() As Byte = Nothing
            memStream.Write(bytes, 0, CInt(memStream.Length))
            Try
                pdfStamp.Close()
                memStream.Close()
                memStream.Dispose()
                Return bytes
            Catch ex As Exception
                Return bytes
            End Try
        End Function
        ''' <summary>
        ''' Adds a Submit Button to Existing PDF Form
        ''' </summary>
        ''' <param name="PDFForm">Stream containing Existing PDF Form to add a button to</param>
        ''' <param name="btnSubmitURL">URL the button submits to</param>
        ''' <param name="btnTop">Button's Top Dimension - Page location in pixels (0 being top of page)</param>
        ''' <param name="btnRight">Button's Right Dimension - Page location in pixels (0 being left side of page)</param>
        ''' <param name="btnBottom">Button's Dimension - Page location in pixels (0 being top of page)</param>
        ''' <param name="btnLeft">Button's Left Dimension - Page location in pixels (0 being left side of page)</param>
        ''' <param name="btnText">Visible Text of Button</param>
        ''' <param name="btnName">Button Field Name</param>
        ''' <param name="btnPage">Page of button to be located</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function Add_Submit_Button(ByVal PDFForm As Stream, ByVal btnSubmitURL As String, ByVal btnTop As Integer, ByVal btnRight As Integer, ByVal btnBottom As Integer, ByVal btnLeft As Integer, Optional ByVal btnText As String = "Submit", Optional ByVal btnName As String = "btnSubmit", Optional ByVal btnPage As Integer = 1) As Byte()
            Dim pdfReader As New iTextSharp.text.pdf.PdfReader(PDFForm)
            Dim memStream As New MemoryStream
            Dim pdfStamp As New iTextSharp.text.pdf.PdfStamper(pdfReader, memStream)
            Dim submitBtn As New iTextSharp.text.pdf.PushbuttonField(pdfStamp.Writer, New iTextSharp.text.Rectangle(btnLeft, btnTop, btnRight, btnBottom), btnName)
            submitBtn.TextColor = iTextSharp.text.Color.BLACK
            submitBtn.BackgroundColor = iTextSharp.text.Color.WHITE
            submitBtn.BorderStyle = iTextSharp.text.pdf.PdfBorderDictionary.STYLE_BEVELED
            submitBtn.BorderColor = iTextSharp.text.Color.GRAY
            submitBtn.Text = btnText
            submitBtn.Alignment = iTextSharp.text.pdf.PdfAppearance.ALIGN_CENTER
            submitBtn.Options = iTextSharp.text.pdf.PushbuttonField.VISIBLE_BUT_DOES_NOT_PRINT
            Dim submitField As iTextSharp.text.pdf.PdfFormField = submitBtn.Field
            submitField.Action = iTextSharp.text.pdf.PdfAction.CreateSubmitForm(btnSubmitURL, Nothing, Nothing)
            submitField.Page = btnPage
            pdfStamp.Writer.AcroForm.AddFormField(submitField)
            Dim bytes() As Byte = Nothing
            memStream.Write(bytes, 0, CInt(memStream.Length))
            Try
                pdfStamp.Close()
                memStream.Close()
                memStream.Dispose()
                Return bytes
            Catch ex As Exception
                Return bytes
            End Try
        End Function
        ''' <summary>
        ''' Adds a Submit Button to Existing PDF Form
        ''' </summary>
        ''' <param name="PDFURL">URL location of Existing PDF Form to add a button to</param>
        ''' <param name="btnSubmitURL">URL the button submits to</param>
        ''' <param name="btnTop">Button's Top Dimension - Page location in pixels (0 being top of page)</param>
        ''' <param name="btnRight">Button's Right Dimension - Page location in pixels (0 being left side of page)</param>
        ''' <param name="btnBottom">Button's Dimension - Page location in pixels (0 being top of page)</param>
        ''' <param name="btnLeft">Button's Left Dimension - Page location in pixels (0 being left side of page)</param>
        ''' <param name="btnText">Visible Text of Button</param>
        ''' <param name="btnName">Button Field Name</param>
        ''' <param name="btnPage">Page of button to be located</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function Add_Submit_Button(ByVal PDFURL As String, ByVal btnSubmitURL As String, ByVal btnTop As Integer, ByVal btnRight As Integer, ByVal btnBottom As Integer, ByVal btnLeft As Integer, Optional ByVal btnText As String = "Submit", Optional ByVal btnName As String = "btnSubmit", Optional ByVal btnPage As Integer = 1) As Byte()
            Dim pdfReader As New iTextSharp.text.pdf.PdfReader(PDFURL)
            Dim memStream As New MemoryStream
            Dim pdfStamp As New iTextSharp.text.pdf.PdfStamper(pdfReader, memStream)
            Dim submitBtn As New iTextSharp.text.pdf.PushbuttonField(pdfStamp.Writer, New iTextSharp.text.Rectangle(btnLeft, btnTop, btnRight, btnBottom), btnName)
            submitBtn.TextColor = iTextSharp.text.Color.BLACK
            submitBtn.BackgroundColor = iTextSharp.text.Color.WHITE
            submitBtn.BorderStyle = iTextSharp.text.pdf.PdfBorderDictionary.STYLE_BEVELED
            submitBtn.BorderColor = iTextSharp.text.Color.GRAY
            submitBtn.Text = btnText
            submitBtn.Alignment = iTextSharp.text.pdf.PdfAppearance.ALIGN_CENTER
            submitBtn.Options = iTextSharp.text.pdf.PushbuttonField.VISIBLE_BUT_DOES_NOT_PRINT
            Dim submitField As iTextSharp.text.pdf.PdfFormField = submitBtn.Field
            submitField.Action = iTextSharp.text.pdf.PdfAction.CreateSubmitForm(btnSubmitURL, Nothing, Nothing)
            submitField.Page = btnPage
            pdfStamp.Writer.AcroForm.AddFormField(submitField)
            Dim bytes() As Byte = Nothing
            memStream.Write(bytes, 0, CInt(memStream.Length))
            Try
                pdfStamp.Close()
                memStream.Close()
                memStream.Dispose()
                Return bytes
            Catch ex As Exception
                Return bytes
            End Try
        End Function
        Private Function ReadXML(ByVal XML As Stream, Optional ByVal FDFInitialize As Boolean = False) As FDFDoc_Class
            Return parseXML(ReadStream_New(XML), CStr(IIf(String_IsNullOrEmpty(FDFDox.XDPGetFile() & ""), "", FDFDox.XDPGetFile())), FDFInitialize)
        End Function
        Private Function ReadXFDF(ByVal XML As Stream, Optional ByVal FDFInitialize As Boolean = False) As FDFDoc_Class
            Return parseXFDF(ReadStream_New(XML), FDFInitialize)
        End Function
        Private Function ReadFDF(ByVal FDF As Stream, Optional ByVal AppendSaves As Boolean = True, Optional ByVal FDFInitialize As Boolean = False) As FDFDoc_Class
            Return FDFOpenWithiText(ReadStream_New(FDF), FDFInitialize, AppendSaves)
        End Function
        Private Function ReturnTriggerString(ByVal whichTrigger As String) As FDFApp.FDFDoc_Class.FDFActionTrigger
            whichTrigger = whichTrigger.TrimStart(" "c)
            whichTrigger = whichTrigger.TrimStart("/"c)
            whichTrigger = whichTrigger.TrimEnd(" "c)
            whichTrigger = whichTrigger.TrimEnd("/"c)
            Dim FDFAction As FDFDoc_Class.FDFActionTrigger = FDFDoc_Class.FDFActionTrigger.FDFUp
            Select Case whichTrigger.ToUpper
                Case "E"
                    Return FDFDoc_Class.FDFActionTrigger.FDFEnter
                Case "X"
                    Return FDFDoc_Class.FDFActionTrigger.FDFExit
                Case "D"
                    Return FDFDoc_Class.FDFActionTrigger.FDFDown
                Case ""
                    Return FDFDoc_Class.FDFActionTrigger.FDFUp
                Case "FO"
                    Return FDFDoc_Class.FDFActionTrigger.FDFOnFocus
                Case "BL"
                    Return FDFDoc_Class.FDFActionTrigger.FDFOnBlur
                Case "C"
                    Return FDFDoc_Class.FDFActionTrigger.FDFCalculate
                Case "F"
                    Return FDFDoc_Class.FDFActionTrigger.FDFFormat
                Case "K"
                    Return FDFDoc_Class.FDFActionTrigger.FDFKeystroke
                Case "V"
                    Return FDFDoc_Class.FDFActionTrigger.FDFValidate
                Case Else
                    Return FDFDoc_Class.FDFActionTrigger.FDFUp
            End Select
        End Function
        Private Function FDFCheckChar(ByVal strINPUT() As String) As String()
            If strINPUT.Length <= 0 Then
                Return New String() {""}
                Exit Function
            End If
            For i As Integer = 0 To strINPUT.Length - 1
                strINPUT(i) = strINPUT(i).Replace("\".ToString, "\\".ToString)
                For Each chrReplace As Char In "/$#~%*^()+=[]{};""<>?|!'".ToCharArray
                    If strINPUT(i).IndexOf(chrReplace) >= 0 Then
                        strINPUT(i) = strINPUT(i).Replace(chrReplace.ToString, CStr("\" & chrReplace.ToString))
                    End If
                Next
                strINPUT(i) = strINPUT(i).Replace(vbNewLine, "\r")
                strINPUT(i) = strINPUT(i).Replace(Environment.NewLine, "\r")
                strINPUT(i) = strINPUT(i).Replace(Chr(13), "\r")
                strINPUT(i) = strINPUT(i).Replace(Chr(10), "\r")
            Next
            Return strINPUT
        End Function
        Private Function FDFCheckChar(ByVal strINPUT As String) As String
            If strINPUT.Length <= 0 Then
                Return ""
                Exit Function
            End If
            strINPUT = strINPUT.Replace("\".ToString, "\\".ToString)
            For Each chrReplace As Char In "/$#~%*^()+=[]{};""<>?|!'".ToCharArray
                If strINPUT.IndexOf(chrReplace) >= 0 Then
                    strINPUT = strINPUT.Replace(chrReplace.ToString, CStr("\" & chrReplace.ToString))
                End If
            Next
            strINPUT = strINPUT.Replace(vbNewLine, "\r")
            strINPUT = strINPUT.Replace(Environment.NewLine, "\r")
            strINPUT = strINPUT.Replace(Chr(13), "\r")
            strINPUT = strINPUT.Replace(Chr(10), "\r")
            Return strINPUT & ""
        End Function
        Private Function FDFCheckCharReverse(ByVal strINPUT As String) As String
            If strINPUT.Length <= 0 Then
                Return ""
                Exit Function
            End If
            strINPUT = strINPUT.Replace("\\".ToString, "\".ToString)
            For Each chrReplace As Char In "/$#~%*^()+=[]{};""<>?|!'".ToCharArray
                If strINPUT.IndexOf("\" & chrReplace) >= 0 Then
                    strINPUT = strINPUT.Replace("\" & chrReplace, chrReplace)
                End If
            Next
            strINPUT = strINPUT.Replace("\r", vbNewLine)  ' \r\n
            strINPUT = strINPUT.Replace("\" & Environment.NewLine, vbNewLine)    ' \r\n
            strINPUT = strINPUT.Replace("\" & Chr(13), vbNewLine)      ' \r\n
            strINPUT = strINPUT.Replace("\" & Chr(10), vbNewLine)      ' \r\n
            Return strINPUT.ToString
        End Function
        Private Function FDFCheckCharReverse2(ByVal strINPUT As String()) As String()
            If strINPUT.Length <= 0 Then
                Return Nothing
                Exit Function
            End If
            For Each chrReplace As Char In "/$#~%*^()+=[]{};""<>?|!'".ToCharArray
                For i As Integer = 0 To strINPUT.Length - 1
                    strINPUT(i) = strINPUT(i).Replace("\\".ToString, "\".ToString)
                    If strINPUT(i).IndexOf("\" & chrReplace) >= 0 Then
                        strINPUT(i) = strINPUT(i).Replace("\" & chrReplace, chrReplace)
                    End If
                    strINPUT(i) = strINPUT(i).Replace("\" & Environment.NewLine, vbNewLine)      ' \r\n
                    strINPUT(i) = strINPUT(i).Replace("\" & Chr(13), vbNewLine)      ' \r\n
                    strINPUT(i) = strINPUT(i).Replace("\" & Chr(10), vbNewLine)      ' \r\n
                    strINPUT(i) = strINPUT(i).Replace("\r", vbNewLine)    ' \r\n
                Next
            Next
            Return strINPUT
        End Function
        Private Function Has_Kids(ByVal startChr As Integer, ByVal endChr As Integer, ByVal strKids As String) As Boolean
            Dim strTest As String = strKids.Substring(startChr, endChr) & ""
            strTest = strTest.Replace("/  Kids", "/Kids")
            strTest = strTest.Replace("/  Kids", "/Kids")
            strTest = strTest.Replace("/ Kids", "/Kids")
            strTest = strTest.Replace("/Kids [", "/Kids[")
            strTest = strTest.Replace("/Kids  [", "/Kids[")
            strTest = strTest.Replace("/Kids   [", "/Kids[")
            If InStr(strTest, "/Kids[") > 0 Then
                Return True
            Else
                Return False
            End If
        End Function
        Private Function FDFImportAppendSaves(ByVal FDF As String, Optional ByVal FDFInitialize As Boolean = False) As FDFDoc_Class
            If FDFInitialize Then FDFDox.Initialize(_defaultEncoding)
            FDFDox.DefaultEncoding = _defaultEncoding
            Dim strFields(6) As String
            Dim FldStart As Integer, FldEnd As Integer, objIncr As Integer
            Dim intField(7) As Integer
            objIncr = 2
            FldStart = FDF.ToLower.IndexOf("1 0 obj", 1) + 7
            FldEnd = FDF.ToLower.IndexOf("trailer", FldStart)
            intField(0) = 1
            intField(2) = 1
            Dim objNum As Integer, IsNumber As Boolean, curDigit As String, intDigit As Integer, cntDigit As Integer
            Try
                If intField(0) > 0 Then
                    Do While FldEnd > intField(2) + 7
                        objNum += 1
                        intField(0) = FDF.ToLower.IndexOf(" 0 obj<<", intField(2))
                        For intDigit = intField(0) To intField(0) - 8 Step -1
                            cntDigit += 1
                            curDigit = FDF.Substring(intField(0) - cntDigit, 1)
                            IsNumber = IsNumeric(curDigit)
                            If Not IsNumber Then
                                cntDigit -= 1
                                Exit For
                            End If
                        Next intDigit
                        strFields(2) = FDF.Substring(intField(0) - cntDigit, cntDigit)
                        strFields(2) = strFields(2).Trim(" "c)
                        strFields(2) = strFields(2).Trim(Chr(10))
                        strFields(2) = strFields(2).Trim(Chr(13))
                        objNum = CInt(strFields(2).TrimEnd(" "c))
                        If objNum = 1 Then
                            intField(1) = FDF.ToLower.IndexOf("/version/", intField(0)) + 9
                            intField(2) = FDF.ToLower.IndexOf("/", intField(1))
                            If intField(1) - 9 > 2 Then
                                strFields(3) = FDF.Substring(intField(1), intField(2) - intField(1))
                            Else
                                strFields(3) = "1.6"
                            End If
                            intField(1) = FDF.ToLower.IndexOf("/annots[", intField(0)) + 8
                            intField(2) = FDF.ToLower.IndexOf("]", intField(1))
                            If intField(1) - 8 > 2 Then
                                strFields(4) = FDF.Substring(intField(1), intField(2) - intField(1))
                            Else
                                strFields(4) = ""
                            End If
                            intField(1) = FDF.ToLower.IndexOf("/differences", intField(0)) + 12
                            intField(2) = FDF.ToLower.IndexOf(">>", intField(1))
                            strFields(5) = FDF.Substring(intField(1), intField(2) - intField(1))
                            If intField(1) - 12 > 2 Then
                                strFields(5) = FDF.Substring(intField(1), intField(2) - intField(1))
                            Else
                                strFields(5) = ""
                            End If
                            FDFDox.Version = strFields(3) & ""
                            FDFDox.Annotations = strFields(4) & ""
                            FDFDox.Differences = strFields(5) & ""
                            If FDFDox.Differences.StartsWith("���") Then
                                FDFDox.Differences = ""
                            End If
                            If FDFDox.Annotations = "" And FDFDox.Differences = "" Then
                                FDFDox.FDFHasChanges = False
                                FDFDox.Annotations = ""
                                FDFDox.Differences = ""
                                Return FDFDox
                                Exit Function
                            Else
                                FDFDox.FDFHasChanges = True
                            End If
                            intField(2) = FDF.ToLower.IndexOf("endobj", intField(0)) + 6
                            If intField(2) + 20 > FldEnd Then
                                Return FDFDox
                                Exit Function
                            End If
                        ElseIf objNum > 1 Then
                            intField(0) = intField(0) - CStr(objNum).Length
                            intField(1) = FDF.ToLower.IndexOf(">>", intField(0)) + 2
                            intField(2) = FDF.ToLower.IndexOf("endobj", intField(1)) + 6
                            strFields(0) = FDF.Substring(intField(0), intField(1) - intField(0)) & ""
                            strFields(1) = FDF.Substring(intField(1), intField(2) - intField(1)) & ""
                            If strFields(0) <> "" Then
                                FDFDox.FDFAddObject(strFields(0), strFields(1))
                            End If
                            intField(4) = FDF.ToLower.IndexOf("endobj", intField(0)) + 10
                            intField(5) = FDF.ToLower.IndexOf("trailer", intField(0))
                            If intField(4) > intField(5) Then
                                Exit Do
                            End If
                            intField(0) = intField(2)
                            objIncr = objIncr + 1
                        Else
                            Exit Do
                        End If
                    Loop
                End If
                Return FDFDox
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.FDFImportAppendSaves", 1)
                Return FDFDox
            End Try
        End Function
        Private Function ReadStream(ByVal InStream As Stream, Optional ByVal omitReturn As Boolean = True) As String
            Dim iCounter As Long = 0, StreamLength As Long = 0, iRead As Integer
            Dim OutString As String
            StreamLength = CInt(InStream.Length)
            Dim bytearray(CInt(StreamLength)) As Byte
            Try
                iRead = InStream.Read(bytearray, 0, CInt(StreamLength))
                InStream.Close()
                InStream = Nothing
                OutString = ""
                For iCounter = 0 To StreamLength - 1
                    If bytearray(CInt(iCounter)) = 10 And omitReturn Then
                        OutString = ""
                    Else
                        OutString &= Chr(bytearray(CInt(iCounter)))
                    End If
                Next
                Return OutString
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.ReadStream", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        Private Function ReadStream_New(ByVal InStream As Stream, Optional ByVal omitReturn As Boolean = True) As String
            Dim OutString As String
            Dim Stream16 As New StreamReader(InStream, True)
            OutString = Stream16.ReadToEnd
            Try
                Stream16.Close()
                Stream16 = Nothing
                Return OutString
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.ReadStream", 1)
                Return OutString
                Exit Function
            End Try
        End Function
        Private Function ReadBytes(ByVal byteArray() As Byte, Optional ByVal omitReturn As Boolean = True) As String
            Dim iCounter As Long = 0, StreamLength As Long = 0
            Dim OutString As String
            Dim InStream As New MemoryStream(byteArray)
            OutString = _defaultEncoding.GetString(byteArray, 0, byteArray.Length)
            Try
                InStream.Close()
                InStream = Nothing
                Return OutString
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.ReadBytes_New", 1)
                Return OutString
                Exit Function
            End Try
        End Function
        Private Function ReadBytes_OLD(ByVal bytearray() As Byte, Optional ByVal omitReturn As Boolean = True) As String
            Try
                Dim iCounter As Long = 0, StreamLength As Long = 0      ', iRead As Integer, inStream As Stream
                Dim OutString As String = ""
                StreamLength = CInt(bytearray.Length)
                For iCounter = 0 To StreamLength - 1
                    If bytearray(CInt(iCounter)) = 10 And omitReturn Then
                    Else
                        OutString &= Chr(bytearray(CInt(iCounter)))
                    End If
                Next
                Return OutString
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.ReadBytes", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        Private Function WriteStream(ByVal InStream As Stream) As Byte()
            Dim iCounter As Long = 0, StreamLength As Long = 0, iRead As Integer
            Dim InString As String = ""
            StreamLength = CInt(InStream.Length)
            Dim bytearray(CInt(StreamLength)) As Byte
            Try
                iRead = InStream.Read(bytearray, 0, CInt(StreamLength))
                InStream.Close()
                Return bytearray
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.WriteBytes", 1)
                Return Nothing
                Exit Function
            End Try
        End Function
        Private Function StringCharToByteArray(ByVal str As String) As Byte()
            Dim oEncoder As New System.Text.UTF8Encoding
            Dim bytArray() As Byte = oEncoder.GetBytes(str)
            Return bytArray
        End Function
        Private Function StringToByteArray(ByVal str As String) As Byte()
            Dim oEncoder As New System.Text.UTF8Encoding
            Dim bytArray() As Byte = oEncoder.GetBytes(str)
            Return bytArray
        End Function
        Private Function ByteArrayToString(ByVal b() As Byte) As String
            Dim str As String
            str = _defaultEncoding.GetString(b)
            Return str
        End Function
        Private Function ByteArrayToASCII(ByVal b() As Byte) As String
            Dim i As Integer
            Dim s As New System.Text.StringBuilder
            For i = 0 To b.Length - 1
                If i <> b.Length - 1 Then
                    s.Append(Chr(b(i)) & " ")
                Else
                    If Not b(i) = 10 Then
                        s.Append(Chr(b(i)))
                    End If
                End If
            Next
            Return s.ToString
        End Function
        Private Function OpenFile(ByVal FullPath As String) As String
            Dim strContents As String
            Dim objReader As StreamReader
            Try
                If File.Exists(FullPath) Then
                    objReader = New StreamReader(FullPath, True)
                    strContents = objReader.ReadToEnd()
                    objReader.Close()
                    Return strContents
                Else
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: Path Not Found", "FDFApp.OpenFile", 1)
                    Return ""
                    Exit Function
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.OpenFile", 1)
                Return Nothing
                Exit Function
            End Try
            Return ""
        End Function
        Private Function GetFileContents(ByVal FullPath As String, Optional ByRef ErrInfo As String = "") As String
            Dim strContents As String
            Dim objReader As StreamReader
            Try
                objReader = New StreamReader(FullPath)
                strContents = objReader.ReadToEnd()
                objReader.Close()
                Return strContents
            Catch Ex As Exception
                ErrInfo = Ex.Message
                Return Nothing
            End Try
        End Function
        Private Function SaveTextToFile(ByVal strData As String, ByVal FullPath As String, Optional ByVal ErrInfo As String = "") As Boolean
            Dim bAns As Boolean = False
            Dim objReader As StreamWriter
            Try
                objReader = New StreamWriter(FullPath)
                objReader.Write(strData)
                objReader.Close()
                bAns = True
            Catch Ex As Exception
                ErrInfo = Ex.Message
            End Try
            Return bAns
        End Function
        ''' <summary>
        ''' PDFSave saves the a PDF to a file path
        ''' </summary>
        ''' <param name="PDFPath">Path and File Name of PDF to input</param>
        ''' <param name="PDFOutputFileName">PDFOutputFileName is the file name to path and name to save the PDF</param>
        ''' <returns>True or False</returns>
        ''' <remarks></remarks>
        Public Function PDFSave(ByVal PDFPath As String, ByVal PDFOutputFileName As String) As Boolean
            Try
                Dim URL As String = PDFPath
                Dim input As Stream = PDFExportStream(URL)
                Dim output As New FileStream(PDFOutputFileName, FileMode.Create)
                Dim count As Integer = 32 * 1024
                Dim buffer(count - 1) As Byte
                Do
                    count = input.Read(buffer, 0, count)
                    If count = 0 Then Exit Do
                    output.Write(buffer, 0, count)
                Loop
                input.Close()
                output.Close()
                Return True
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, Ex.Message, "FDFApp.PDFSave", 1)
                Return False
            End Try
        End Function
        ''' <summary>
        ''' PDFSavetoStream outputs a PDF to a stream object
        ''' </summary>
        ''' <param name="PDFPath">Path and File Name of PDF to input</param>
        ''' <returns>PDF Document in a Stream Object</returns>
        ''' <remarks></remarks>
        Public Function PDFSavetoStream(ByVal PDFPath As String) As Stream
            Try
                Dim URL As String = PDFPath
                Dim input As Stream = PDFExportStream(URL)
                Return input
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, Ex.Message, "FDFApp.PDFSavetoStream", 1)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' PDFSavetoBuf outputs a PDF to a byte array
        ''' </summary>
        ''' <param name="PDFPath">Path and File Name of PDF to output</param>
        ''' <returns>PDF Document in a Byte Array Object</returns>
        ''' <remarks></remarks>
        Public Function PDFSavetoBuf(ByVal PDFPath As String) As Byte()
            Try
                Dim URL As String = PDFPath
                Return PDFExportByte(URL)
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, Ex.Message, "FDFApp.PDFSavetoBuf", 1)
                Return Nothing
            End Try
        End Function
        ''' <summary>
        ''' PDFSavetoStr outputs a PDF to a String Object
        ''' </summary>
        ''' <param name="PDFPath">Path and File Name of PDF to input</param>
        ''' <returns>PDF Document in a String Object</returns>
        ''' <remarks></remarks>
        Public Function PDFSavetoStr(ByVal PDFPath As String) As String
            Try
                Dim PDFFile As String
                Dim URL As String = PDFPath
                Dim input As Stream = PDFExportStream(URL)
                Dim reader As StreamReader = New StreamReader(input)
                PDFFile = reader.ReadToEnd
                Return PDFFile
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, Ex.Message, "FDFApp.PDFSavetoStr", 1)
                Return Nothing
            End Try
        End Function
        Private Function PDFExportString(ByVal PDFPath As String) As String
            Try
                Dim PDFFile As String
                Dim URL As String = PDFPath
                Dim input As Stream = PDFExportStream(URL)
                Dim reader As StreamReader = New StreamReader(input)
                PDFFile = reader.ReadToEnd
                Return PDFFile
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, Ex.Message, "FDFApp.PDFSavetoBuf", 1)
                Return Nothing
            End Try
        End Function
        Private Function PDFExportStream(ByVal PDFPath As String) As Stream
            Try
                If IsValidUrl(PDFPath) Then
                    Dim client As New WebClient
                    Dim input As Stream = client.OpenRead(PDFPath)
                    Return input
                ElseIf File.Exists(PDFPath) Then
                    Dim rdStream As New FileStream(PDFPath, FileMode.Open, FileAccess.Read)
                    Return rdStream
                Else
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: PDF File Not Found", "FDFApp.PDFExportStream", 1)
                    Return Nothing
                End If
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, Ex.Message, "FDFApp.PDFSavetoBuf", 1)
                Return Nothing
            End Try
        End Function
        Private Function PDFExportCharArray(ByVal PDFPath As String) As Char()
            Dim PDFFile As String
            If IsValidUrl(PDFPath) Then
                Dim wrq As WebRequest = WebRequest.Create(PDFPath)
                Dim wrp As WebResponse = wrq.GetResponse()
                Dim reader As StreamReader = New StreamReader(wrp.GetResponseStream())
                PDFFile = reader.ReadToEnd
                Return ExportBuffer(PDFFile)
            ElseIf File.Exists(PDFPath) Then
                PDFFile = Me.OpenFile(PDFPath)
                Dim FS As New FileStream(PDFPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                Dim reader As StreamReader = New StreamReader(FS)
                PDFFile = reader.ReadToEnd
                Return ExportBuffer(PDFFile)
            Else
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: PDF File Not Found", "FDFApp.PDFExportBuffer", 1)
                Return Nothing
            End If
        End Function
        Private Function PDFExportByte(ByVal PDFPath As String) As Byte()
            Dim PDFFile As String = ""
            If IsValidUrl(PDFPath) Then
                Dim client As New WebClient
                Return client.DownloadData(PDFPath)
            ElseIf File.Exists(PDFPath) Then
                Dim FS As New FileStream(PDFPath, FileMode.Open, FileAccess.Read, FileShare.Read)
                Dim input As Stream = FS
                Dim br As New BinaryReader(FS)
                Dim bytesRead As Byte() = br.ReadBytes(CInt(input.Length))
                Return bytesRead
                PDFFile = Me.OpenFile(PDFPath)
                Return ExportByte(PDFFile)
            Else
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: PDF File Not Found", "FDFApp.PDFExportBuffer", 1)
                Return Nothing
            End If
        End Function
        Private Function IsValidUrl(ByVal url As String) As Boolean
            Return System.Text.RegularExpressions.Regex.IsMatch(url, "^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&amp;%\$#_]*)?$")
        End Function
        Private Function WriteAppendSaves(ByVal xType As FDFType) As String
            Dim retString As String = ""
            Select Case xType
                Case FDFType.FDF
                    retString = FDFDox.GetChanges
                Case FDFType.xFDF
                    Return ""
                Case FDFType.XML
                    Return ""
            End Select
            Return retString
        End Function
        Private Function WriteHead(ByVal xType As FDFType, Optional ByVal AppendSaves As Boolean = False) As String
            Dim retString As String = ""
            Try
                Select Case xType
                    Case FDFType.FDF
                        If AppendSaves And FDFDox.FDFHasChanges Then
                            retString = "%FDF-1.2" & vbNewLine & "%����" & vbNewLine & "1 0 obj<< /Version /" & CStr(IIf(FDFDox.Version <> "", FDFDox.Version, "1.4")) & " /FDF" & CStr(IIf(FDFDox.Annotations <> "" And AppendSaves = True, "<< /Annots [" & FDFDox.Annotations & "] ", "<<"))
                        Else
                            retString = "%FDF-1.2" & vbNewLine & "%����" & vbNewLine & "1 0 obj<< /Version/1.6 /FDF << "
                        End If
                    Case FDFType.xFDF
                        retString = "<?xml version=""1.0"" encoding=""UTF-8""?><xfdf xmlns=""http://ns.adobe.com/xfdf/"" xml:space=""preserve"">"
                    Case FDFType.XML
                        retString = "<?xml version=""1.0"" encoding=""UTF-8""?>"
                    Case FDFType.XDP
                        retString = "<?xml version=""1.0"" encoding=""UTF-8""?><?xfa generator=""XFA2_4"" APIVersion=""2.6.7120.0""?><xdp:xdp xmlns:xdp=""http://ns.adobe.com/xdp/""><xfa:datasets xmlns:xfa=""http://www.xfa.org/schema/xfa-data/1.0/""><xfa:data>"
                End Select
                Return retString
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFApp.WriteHead", 1)
                Return ""
            End Try
        End Function
        Private Function WriteFields(ByVal xType As FDFType) As String
            Dim retString As String = "", xFDFField As FDFDoc_Class.FDFField
            Dim FldValue As String = ""
            Try
                Select Case xType
                    Case FDFType.FDF
                        retString = "/Fields ["
                        For Each xFDFField In FDFDox.FDFFields
                            If xFDFField.FieldEnabled Then
                                If xFDFField.FieldType = FDFDoc_Class.FieldType.FldOption And Not xFDFField.FieldValue Is Nothing Then
                                    FldValue = xFDFField.FieldValue(0) & ""
                                    FldValue = FDFCheckChar(FldValue)
                                    retString = retString & "<< /T (" & xFDFField.FieldName & ") /V/" & FldValue & ">>"
                                ElseIf xFDFField.FieldType = FDFDoc_Class.FieldType.FldMultiSelect And Not xFDFField.FieldValue Is Nothing Then
                                    retString = retString & "<< /T (" & xFDFField.FieldName & ") /V ["
                                    For Each FldValue In xFDFField.FieldValue
                                        If FldValue.Length > 0 Then
                                            FldValue = FDFCheckChar(FldValue)
                                            retString = retString & "(" & (FldValue & "") & ")"
                                        End If
                                    Next
                                    retString = retString & "] >>"
                                ElseIf xFDFField.FieldType = FDFDoc_Class.FieldType.FldTextual And Not xFDFField.FieldValue Is Nothing Then
                                    FldValue = xFDFField.FieldValue(0) & ""
                                    FldValue = FDFCheckChar(FldValue)
                                    retString = retString & "<< /T (" & xFDFField.FieldName & ") /V (" & FldValue & ") >>"
                                End If
                            End If
                        Next
                        retString = retString & "]"
                    Case FDFType.xFDF
                        retString = "<fields>"
                        For Each xFDFField In FDFDox.FDFFields
                            If xFDFField.FieldEnabled Then
                                Select Case xFDFField.FieldType
                                    Case FDFDoc_Class.FieldType.FldOption
                                        If xFDFField.FieldValue.Count > 0 Then retString = retString & "<field name=""" & xFDFField.FieldName & """><value>" & XMLCheckChar(xFDFField.FieldValue(0) & "") & "</value></field>"
                                    Case FDFDoc_Class.FieldType.FldMultiSelect
                                        If xFDFField.FieldValue.Count > 0 Then
                                            Dim FldsVal() As String = xFDFField.FieldValue.ToArray
                                            Dim FldVal As String, FldNum As Integer = 0
                                            For Each FldVal In FldsVal
                                                FldNum += 1
                                                If FldNum = 1 Then
                                                    FldValue = FldValue & "<value>" & XMLCheckChar(FldVal.TrimStart("("c) & "") & "</value>"
                                                ElseIf FldNum = FldsVal.Length Then
                                                    FldValue = FldValue & "<value>" & XMLCheckChar(FldVal.TrimEnd(")"c) & "") & "</value>"       '
                                                Else
                                                    FldValue = FldValue & "<value>" & XMLCheckChar(FldVal & "") & "</value>"
                                                End If
                                            Next
                                            retString = retString & "<field name=""" & xFDFField.FieldName & """>" & XMLCheckChar(FldValue & "") & "</field>"
                                        End If
                                    Case FDFDoc_Class.FieldType.FldTextual
                                        retString = retString & "<field name=""" & xFDFField.FieldName & """><value>" & XMLCheckChar(xFDFField.FieldValue(0) & "") & "</value></field>"
                                End Select
                            End If
                        Next
                        retString = retString & "</fields>"
                    Case FDFType.XML
                        retString = "<fields>"
                        For Each xFDFField In FDFDox.FDFFields
                            If xFDFField.FieldEnabled Then
                                If Not xFDFField.FieldType = FDFDoc_Class.FieldType.FldMultiSelect And Not xFDFField.FieldValue Is Nothing Then
                                    FldValue = xFDFField.FieldValue(0)
                                    FldValue = XMLCheckChar(FldValue)
                                    retString = retString & "<" & xFDFField.FieldName & ">" & XMLCheckChar(FldValue & "") & "</" & xFDFField.FieldName & ">"
                                End If
                            End If
                        Next
                        retString = retString & "</fields>"
                    Case FDFType.XDP
                        retString = WriteXDPFormFields()
                End Select
                Return retString
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFApp.WriteFields", 1)
                Return ""
            End Try
        End Function
        Private Function XMLCheckChar(ByVal strXML As String) As String
            Try
                If strXML Is Nothing Then
                    Return ""
                End If
                If Not String_IsNullOrEmpty(strXML & "") Then
                    strXML = strXML.Replace("&", "&amp;")
                    strXML = strXML.Replace("<", "&lt;")
                    strXML = strXML.Replace(">", "&gt;")
                    strXML = strXML.Replace("&amp;amp;", "&amp;")
                    strXML = strXML.Replace("&amp;apos;", "&apos;")
                    strXML = strXML.Replace("&amp;quot;", "&quot;")
                    strXML = strXML.Replace("&amp;lt;", "&lt;")
                    strXML = strXML.Replace("&amp;gt;", "&gt;")
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, ex)
                Err.Clear()
            End Try
            Return strXML & ""
        End Function
        Private Function XMLCheckCharReverse(ByVal strXML As String) As String
            Try
                If strXML Is Nothing Then
                    Return ""
                End If
                If Not String_IsNullOrEmpty(strXML & "") Then
                    strXML = strXML.Replace("&amp;amp;", "&amp;")
                    strXML = strXML.Replace("&amp;apos;", "&apos;")
                    strXML = strXML.Replace("&amp;quot;", "&quot;")
                    strXML = strXML.Replace("&amp;lt;", "&lt;")
                    strXML = strXML.Replace("&amp;gt;", "&gt;")
                    strXML = strXML.Replace("&amp;", "&")
                    strXML = strXML.Replace("&lt;", "<")
                    strXML = strXML.Replace("&apos;", "'")
                    strXML = strXML.Replace("&quot;", """")
                    strXML = strXML.Replace("&lt;", "<")
                    strXML = strXML.Replace("&gt;", ">")
                    strXML = strXML.Replace("&amp;", "&")
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, ex)
                Err.Clear()
            End Try
            Return strXML & ""
        End Function
        Private Function WriteXDPFormFields() As String
            Dim retString As String = ""
            Try
                If Not FDFDox Is Nothing Then
                    Dim FormIndex As Integer = 0
                    For Each XDPDoc1 As FDFDoc_Class.FDFDoc_Class In FDFDox.GetXDPForms
                        FormIndex += 1
                        If Not XDPDoc1.FormName Is Nothing Then
                            If XDPDoc1.DocType = FDFDoc_Class.FDFDocType.XDPForm And XDPDoc1.struc_FDFFields.Count >= 1 Then
                                retString &= "<" & XMLCheckChar(XDPDoc1.FormName) & ">"
                                For Each fld As FDFDoc_Class.FDFField In XDPDoc1.struc_FDFFields
                                    If Not fld.FieldName Is Nothing Then
                                        If fld.FieldValue.Count > 0 Then
                                            If String_IsNullOrEmpty(fld.FieldValue(0).ToString) = True Then
                                                retString &= "<" & fld.FieldName & "></" & fld.FieldName & ">"
                                            Else
                                                retString &= "<" & fld.FieldName & ">" & XMLCheckChar(fld.FieldValue(0)) & "</" & fld.FieldName & ">"
                                            End If
                                        End If
                                    End If
                                Next
                                retString &= "</" & XMLCheckChar(XDPDoc1.FormName) & ">"
                            End If
                        Else
                        End If
                    Next
                End If
                Return retString & ""
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFApp.WriteTemplates", 1)
                Return ""
            End Try
        End Function
        Private Function WriteEnd(ByVal xType As FDFType, Optional ByVal AppendSaves As Boolean = False) As String
            Dim retString As String = ""
            Try
                Select Case xType
                    Case FDFType.FDF
                        If AppendSaves And FDFDox.FDFHasChanges Then
                            retString = CStr(IIf(FDFDox.FDFGetFile = "", "", " /F (" & FDFDox.FDFGetFile & ")")) & CStr(IIf(FDFDox.FDFGetStatus = "", "", " /Status (" & FDFDox.FDFGetStatus & ")")) & CStr(IIf(FDFDox.Differences <> "" And AppendSaves = True, " /Differences " & FDFDox.Differences, "")) & " >> >>" & vbNewLine & "endobj" & vbNewLine & CStr(IIf(FDFDox.FDFHasChanges = True And AppendSaves = True, WriteAppendSaves(xType), "")) & vbNewLine & "trailer" & vbNewLine & "<< /Root 1 0 R >>" & vbNewLine & "%%EOF"
                        Else
                            retString = CStr(IIf(FDFDox.FDFGetFile = "", "", " /F (" & FDFDox.FDFGetFile & ")")) & CStr(IIf(FDFDox.FDFGetStatus = "", "", " /Status (" & FDFDox.FDFGetStatus & ")")) & " >> >>" & vbNewLine & "endobj" & vbNewLine & "trailer" & vbNewLine & " << /Root 1 0 R >>" & vbNewLine & "%%EOF"
                        End If
                    Case FDFType.xFDF
                        retString = CStr(IIf(FDFDox.FDFGetFile = "", "", "<f href=""" & FDFDox.FDFGetFile & """/>")) & "</xfdf>"
                    Case FDFType.XML
                        retString = ""
                    Case FDFType.XDP
                        retString = "</xfa:data></xfa:datasets><pdf href=""" & FDFDox.FDFGetFile & """ xmlns=""http://ns.adobe.com/xdp/pdf/""/></xdp:xdp>"
                End Select
                Return retString
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFApp.WriteEnd", 1)
                Return ""
            End Try
        End Function
        Private Function FDFExtractAppendSaves(ByVal aFDFDoc As FDFDoc_Class, ByVal FDF As String, Optional ByVal FDFInitialize As Boolean = False) As String
            FDFDox = aFDFDoc
            If FDFInitialize Then FDFDox.Initialize(_defaultEncoding)
            FDFDox.DefaultEncoding = _defaultEncoding
            Dim strFields(6) As String
            Dim FldStart As Integer, FldEnd As Integer, objIncr As Integer
            Dim intField(7) As Integer
            objIncr = 2
            FldStart = FDF.ToLower.IndexOf("1 0 obj", 1) + 7
            FldEnd = FDF.ToLower.IndexOf("trailer", FldStart)
            intField(0) = 1
            intField(2) = 1
            Dim objNum As Integer
            Try
                If intField(0) > 0 Then
                    Do While FldEnd > intField(0)
                        objNum = objNum + 1
                        intField(0) = FDF.ToLower.IndexOf(" 0 obj<<", intField(2))
                        strFields(2) = FDF.Substring(intField(0) - 2, 3)
                        strFields(2) = strFields(2).Trim(" "c)
                        strFields(2) = strFields(2).Trim(Chr(10))
                        strFields(2) = strFields(2).Trim(Chr(13))
                        objNum = CInt(strFields(2))
                        If objNum = 1 Then
                            intField(1) = FDF.ToLower.IndexOf("/version/", intField(0)) + 9
                            intField(2) = FDF.ToLower.IndexOf("/", intField(1))
                            strFields(3) = FDF.Substring(intField(1), intField(2) - intField(1))
                            intField(1) = FDF.ToLower.IndexOf("/annots[", intField(0)) + 8
                            intField(2) = FDF.ToLower.IndexOf("]", intField(1))
                            strFields(4) = FDF.Substring(intField(1), intField(2) - intField(1))
                            intField(1) = FDF.ToLower.IndexOf("/differences", intField(0)) + 12
                            intField(2) = FDF.ToLower.IndexOf(">>", intField(1))
                            strFields(5) = FDF.Substring(intField(1), intField(2) - intField(1))
                            FDFDox.Version = strFields(3)
                            FDFDox.Annotations = strFields(4)
                            FDFDox.Differences = strFields(5) & ""
                            If FDFDox.Differences.StartsWith("���") Then
                                FDFDox.Differences = ""
                            End If
                            If FDFDox.Annotations = "" And FDFDox.Differences = "" Then
                                FDFDox.FDFHasChanges = False
                                FDFDox.Annotations = ""
                                FDFDox.Differences = ""
                                Return FDFDox.GetChanges
                                Exit Function
                            Else
                                FDFDox.FDFHasChanges = True
                            End If
                            intField(2) = FDF.ToLower.IndexOf("endobj", intField(0)) + 6
                        ElseIf objNum > 1 Then
                            If objNum > 9 Then
                                intField(0) = intField(0) - 2
                            Else
                                intField(0) = intField(0) - 1
                            End If
                            intField(1) = FDF.ToLower.IndexOf(">>", intField(0)) + 2
                            intField(2) = FDF.ToLower.IndexOf("endobj", intField(1)) + 6
                            strFields(0) = FDF.Substring(intField(0), intField(1) - intField(0)) & ""
                            strFields(1) = FDF.Substring(intField(1), intField(2) - intField(1)) & ""
                            If strFields(0) <> "" Then
                                FDFDox.FDFAddObject(strFields(0), strFields(1))
                            End If
                            intField(4) = FDF.ToLower.IndexOf("endobj", intField(0)) + 10
                            intField(5) = FDF.ToLower.IndexOf("trailer", intField(0))
                            If intField(4) > intField(5) Then
                                Exit Do
                            End If
                            intField(0) = intField(2)
                            objIncr = objIncr + 1
                        Else
                            Exit Do
                        End If
                    Loop
                End If
                Return FDFDox.GetChanges
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFApp.FDFExtractAppendSaves", 1)
                Return ""
            End Try
        End Function
        Private Function FDFSaveToFile(ByVal strData As String, ByVal FullPath As String, Optional ByVal ErrInfo As String = "") As Boolean
            Dim bAns As Boolean = False
            Dim objReader As StreamWriter
            Try
                objReader = New StreamWriter(FullPath)
                objReader.Write(strData)
                objReader.Close()
                bAns = True
            Catch Ex As Exception
                ErrInfo = Ex.Message
            End Try
            Return bAns
        End Function
        Private Function OpenDocument(ByVal DocName As String) As Boolean
            Try
                Start(DocName)
                Return True
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: " & ex.Message, "FDFApp.OpenDocument", 1)
                Return False
            End Try
        End Function
        Enum ExportCode
            Binary = 0
            Text = 1
        End Enum
        Private Function FDFExportString(ByVal aFDFDoc As FDFDoc_Class, Optional ByVal AppendSaves As Boolean = True) As String
            FDFDox = aFDFDoc
            Dim FDFExport As String
            FDFExport = WriteHead(FDFType.FDF, AppendSaves)
            FDFExport = FDFExport & WriteFields(FDFType.FDF)
            FDFExport = FDFExport & WriteEnd(FDFType.FDF, AppendSaves)
            Return FDFExport
        End Function
        Private Function FDFExportBuffer(ByVal aFDFDoc As FDFDoc_Class, Optional ByVal AppendSaves As Boolean = True) As Char()
            FDFDox = aFDFDoc
            Dim FDFExport As String
            FDFExport = WriteHead(FDFType.FDF, AppendSaves)
            FDFExport = FDFExport & WriteFields(FDFType.FDF)
            FDFExport = FDFExport & WriteEnd(FDFType.FDF, AppendSaves)
            Return ExportBuffer(FDFExport)
        End Function
        Private Function XMLExportString(ByVal aFDFDoc As FDFDoc_Class) As String
            FDFDox = aFDFDoc
            Dim FDFExport As String
            FDFExport = WriteHead(FDFType.XML)
            FDFExport = FDFExport & WriteFields(FDFType.XML)
            FDFExport = FDFExport & WriteEnd(FDFType.XML)
            Return FDFExport
        End Function
        Private Function XMLExportBuffer(ByVal aFDFDoc As FDFDoc_Class) As Char()
            FDFDox = aFDFDoc
            Dim FDFExport As String
            FDFExport = WriteHead(FDFType.XML)
            FDFExport = FDFExport & WriteFields(FDFType.XML)
            FDFExport = FDFExport & WriteEnd(FDFType.XML)
            Return ExportBuffer(FDFExport)
        End Function
        Private Function XMLExportStream(ByVal aFDFDoc As FDFDoc_Class) As Stream
            FDFDox = aFDFDoc
            Dim FDFExport As String
            FDFExport = WriteHead(FDFType.XML)
            FDFExport = FDFExport & WriteFields(FDFType.XML)
            FDFExport = FDFExport & WriteEnd(FDFType.XML)
            Return ExportStream(FDFExport)
        End Function
        Private Function XDPExportString(ByVal aFDFDoc As FDFDoc_Class) As String
            FDFDox = aFDFDoc
            Dim FDFExport As String
            FDFExport = WriteHead(FDFType.XDP)
            FDFExport = FDFExport & WriteFields(FDFType.XDP)
            FDFExport = FDFExport & WriteEnd(FDFType.XDP)
            Return FDFExport
        End Function
        Private Function XDPExportBuffer(ByVal aFDFDoc As FDFDoc_Class) As Char()
            FDFDox = aFDFDoc
            Dim FDFExport As String
            FDFExport = WriteHead(FDFType.XDP)
            FDFExport = FDFExport & WriteFields(FDFType.XDP)
            FDFExport = FDFExport & WriteEnd(FDFType.XDP)
            Return ExportBuffer(FDFExport)
        End Function
        Private Function XDPExportStream(ByVal aFDFDoc As FDFDoc_Class) As Stream
            FDFDox = aFDFDoc
            Dim FDFExport As String
            FDFExport = WriteHead(FDFType.XDP)
            FDFExport = FDFExport & WriteFields(FDFType.XDP)
            FDFExport = FDFExport & WriteEnd(FDFType.XDP)
            Return ExportStream(FDFExport)
        End Function
        Private Function XFDFExportString(ByVal aFDFDoc As FDFDoc_Class) As String
            FDFDox = aFDFDoc
            Dim FDFExport As String
            FDFExport = WriteHead(FDFType.xFDF)
            FDFExport = FDFExport & WriteFields(FDFType.xFDF)
            FDFExport = FDFExport & WriteEnd(FDFType.xFDF)
            Return FDFExport
        End Function
        Private Function XFDFExportBuffer(ByVal aFDFDoc As FDFDoc_Class) As Char()
            FDFDox = aFDFDoc
            Dim FDFExport As String
            FDFExport = WriteHead(FDFType.xFDF)
            FDFExport = FDFExport & WriteFields(FDFType.xFDF)
            FDFExport = FDFExport & WriteEnd(FDFType.xFDF)
            Return ExportBuffer(FDFExport)
        End Function
        Private Function XMLExportByte(ByVal aFDFDoc As FDFDoc_Class) As Byte()
            FDFDox = aFDFDoc
            Dim FDFExport As String
            FDFExport = WriteHead(FDFType.XML)
            FDFExport = FDFExport & WriteFields(FDFType.XML)
            FDFExport = FDFExport & WriteEnd(FDFType.XML)
            Return ExportByte(FDFExport)
        End Function
        Private Function XFDFExportStream(ByVal aFDFDoc As FDFDoc_Class) As Stream
            FDFDox = aFDFDoc
            Dim FDFExport As String
            FDFExport = WriteHead(FDFType.xFDF)
            FDFExport = FDFExport & WriteFields(FDFType.xFDF)
            FDFExport = FDFExport & WriteEnd(FDFType.xFDF)
            Return ExportStream(FDFExport)
        End Function
        Private Function XFDFExportByte(ByVal aFDFDoc As FDFDoc_Class) As Byte()
            FDFDox = aFDFDoc
            Dim FDFExport As String
            FDFExport = WriteHead(FDFType.xFDF)
            FDFExport = FDFExport & WriteFields(FDFType.xFDF)
            FDFExport = FDFExport & WriteEnd(FDFType.xFDF)
            Return ExportByte(FDFExport)
        End Function
        Private Function FDFSave(ByVal theFDF As FDFDoc_Class, ByVal FileName As String, Optional ByVal eFDFType As FDFType = FDFType.FDF, Optional ByVal AppendSaves As Boolean = True) As Boolean
            Dim strFDFData As String = ""
            Try
                Select Case eFDFType
                    Case FDFType.FDF
                        strFDFData = FDFExportString(theFDF, AppendSaves)
                    Case FDFType.xFDF
                        strFDFData = XFDFExportString(theFDF)
                    Case FDFType.XML
                        strFDFData = XMLExportString(theFDF)
                End Select
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.FDFSave", 1)
                Exit Function
            End Try
            Dim bAns As Boolean = False
            Dim objReader As StreamWriter
            Try
                If strFDFData <> "" Then
                    Try
                        objReader = New StreamWriter(FileName)
                        objReader.Write(strFDFData)
                        objReader.Close()
                        bAns = True
                        Return True
                    Catch Ex As Exception
                        _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, Ex.Message, "FDFApp.FDFSave", 1)
                        Return False
                    End Try
                    Return True
                Else
                    Return False
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.FDFSave", 1)
                Exit Function
            End Try
        End Function
        Private Function FDFSavetoFile(ByVal theFDF As FDFDoc_Class, ByVal FileName As String, Optional ByVal eFDFType As FDFType = FDFType.FDF, Optional ByVal AppendSaves As Boolean = True) As Boolean
            Dim strFDFData As String = ""
            Try
                Select Case eFDFType
                    Case FDFType.FDF
                        strFDFData = FDFExportString(theFDF, AppendSaves)
                    Case FDFType.xFDF
                        strFDFData = XFDFExportString(theFDF)
                    Case FDFType.XML
                        strFDFData = XMLExportString(theFDF)
                    Case FDFType.XDP
                        strFDFData = XDPExportString(theFDF)
                End Select
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.FDFSaveatoFile", 1)
                Exit Function
            End Try
            Dim bAns As Boolean = False
            Dim objReader As StreamWriter
            Try
                If strFDFData <> "" Then
                    Try
                        objReader = New StreamWriter(FileName)
                        objReader.Write(ByteArrayToString(_defaultEncoding.GetBytes(strFDFData)))
                        objReader.Close()
                        bAns = True
                        Return True
                    Catch Ex As Exception
                        _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFApp.FDFSavetoFile", 1)
                        Return False
                    End Try
                Else
                    _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcFileSysErr, "Error: File Path Error", "FDFApp.FDFSavetoFile", 1)
                    Return False
                End If
            Catch ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & ex.Message, "FDFApp.FDFSavetoFile", 1)
                Exit Function
            End Try
        End Function
        Private Function FDFSavetoBuf(ByVal theFDF As FDFDoc_Class, Optional ByVal eFDFType As FDFType = FDFType.FDF, Optional ByVal AppendSaves As Boolean = True) As Byte()
            Dim strFDFData As String = ""
            Try
                Select Case eFDFType
                    Case FDFType.FDF
                        strFDFData = FDFExportString(theFDF, AppendSaves)
                    Case FDFType.xFDF
                        strFDFData = XFDFExportString(theFDF)
                    Case FDFType.XML
                        strFDFData = XMLExportString(theFDF)
                    Case FDFType.XDP
                        strFDFData = XDPExportString(theFDF)
                End Select
                Return _defaultEncoding.GetBytes(strFDFData)
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFApp.FDFSavetoBuf", 1)
                Return Nothing
            End Try
        End Function
        Private Function FDFSavetoStream(ByVal theFDF As FDFDoc_Class, Optional ByVal eFDFType As FDFType = FDFType.FDF, Optional ByVal AppendSaves As Boolean = True) As Stream
            Dim strFDFData As String = ""
            Try
                Select Case eFDFType
                    Case FDFType.FDF
                        strFDFData = FDFExportString(theFDF, AppendSaves)
                    Case FDFType.xFDF
                        strFDFData = XFDFExportString(theFDF)
                    Case FDFType.XML
                        strFDFData = XMLExportString(theFDF)
                    Case FDFType.XDP
                        strFDFData = XDPExportString(theFDF)
                End Select
                Dim memStream As New MemoryStream(ExportByte(strFDFData))
                Return memStream
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFApp.FDFSavetoBuf", 1)
                Return Nothing
            End Try
        End Function
        Private Function FDFSavetoStr(ByVal theFDF As FDFDoc_Class, Optional ByVal eFDFType As FDFType = FDFType.FDF, Optional ByVal AppendSaves As Boolean = True) As String
            Dim strFDFData As String = ""
            Try
                Select Case eFDFType
                    Case FDFType.FDF
                        strFDFData = FDFExportString(theFDF, AppendSaves)
                    Case FDFType.xFDF
                        strFDFData = XFDFExportString(theFDF)
                    Case FDFType.XML
                        strFDFData = XMLExportString(theFDF)
                    Case FDFType.XDP
                        strFDFData = XDPExportString(theFDF)
                End Select
                Return strFDFData & ""
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFApp.FDFSavetoStr", 1)
                Return ""
            End Try
        End Function
        Private Function FDFSavetoArray(ByVal theFDF As FDFDoc_Class, Optional ByVal eFDFType As FDFType = FDFType.FDF, Optional ByVal AppendSaves As Boolean = True) As Char()
            Dim strFDFData As String = ""
            Try
                Select Case eFDFType
                    Case FDFType.FDF
                        strFDFData = FDFExportBuffer(theFDF, AppendSaves)
                    Case FDFType.xFDF
                        strFDFData = XFDFExportBuffer(theFDF)
                    Case FDFType.XML
                        strFDFData = XMLExportBuffer(theFDF)
                    Case FDFType.XDP
                        strFDFData = XDPExportString(theFDF)
                End Select
                Return strFDFData.ToCharArray
            Catch Ex As Exception
                _FDFErrors.FDFAddError(FDFErrors.FDFErc.FDFErcInternalError, "Error: " & Ex.Message, "FDFApp.FDFSavetoBuf", 1)
                Return "".ToCharArray
            End Try
        End Function
        Private Function ExportStream(ByVal str As String) As Stream
            Dim s As Char()
            Dim xStream As New MemoryStream
            s = str.ToCharArray
            Dim b(CInt(CInt(s.Length))) As Byte
            Dim i As Integer
            For i = 0 To s.Length - 1
                b(i) = System.Convert.ToByte(s(i))
            Next
            xStream.Read(b, 0, CInt(b.Length))
            Return xStream
        End Function
        Private Function ExportByte(ByVal str As String) As Byte()
            Dim buffer() As Byte
            Dim encoder As New System.Text.UTF8Encoding
            ReDim buffer(str.Length - 1)
            encoder.GetBytes(str, 0, str.Length, buffer, 0)
            Return buffer
        End Function
        Private Function ExportBuffer(ByVal str As String) As Char()
            Dim s As Char()
            s = str.ToCharArray
            Return s
        End Function
        ''' <summary>
        ''' Prints a Document to the default printer using the default application using adobe products
        ''' </summary>
        ''' <param name="FileName">Path to PDF File</param>
        ''' <returns>TRUE if printing has started</returns>
        ''' <remarks></remarks>
        Public Function PrintPDF(ByVal FileName As String) As Boolean
            If File.Exists(FileName) Then
                Dim myProcess As Process = New Process
                Try
                    myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
                    myProcess.StartInfo.FileName = FileName
                    myProcess.StartInfo.Verb = "Print"
                    myProcess.StartInfo.UseShellExecute = True
                    myProcess.Start()
                    Dim x As Integer = 0
                    Do While Not myProcess.HasExited
                        System.Threading.Thread.Sleep(100)
                        x += 1
                        If myProcess.CloseMainWindow Or x > 360 Then
                            Exit Do
                        End If
                    Loop
                    myProcess.Close()
                    myProcess.Dispose()
                    myProcess = Nothing
                    Return True
                Catch ex As Exception
                    myProcess.Close()
                    myProcess.Dispose()
                    myProcess = Nothing
                    Return False
                End Try
            End If
        End Function
        ''' <summary>
        ''' Prints a Document to the specified printer using the specified application
        ''' </summary>
        ''' <param name="FileName">Path to PDF File</param>
        ''' <param name="PrinterName">Name of printer</param>
        ''' <param name="ApplicationPath">Application path to PDF reader</param>
        ''' <returns>TRUE if printing has finished</returns>
        ''' <remarks></remarks>
        Public Function PrintPDF(ByVal Filename As String, ByVal PrinterName As String, ByVal ApplicationPath As String) As Boolean
            Dim starter As New ProcessStartInfo(ApplicationPath, " /t """ + Filename + """ """ + PrinterName + """")
            Dim Process1 As New Process
            Dim buffer As New StringBuilder()
            Try
                starter.CreateNoWindow = True
                starter.WindowStyle = ProcessWindowStyle.Hidden
                starter.RedirectStandardOutput = True
                starter.UseShellExecute = False
                Process1.StartInfo = starter
                Process1.Start()
                Dim x As Integer = 0
                Do While Not Process1.HasExited
                    System.Threading.Thread.Sleep(100)
                    x += 1
                    If Process1.CloseMainWindow Or x > 360 Then
                        Exit Do
                    End If
                Loop
                Process1.Close()
                Process1.Dispose()
                Process1 = Nothing
                starter = Nothing
                Return True
            Catch ex As Exception
                Process1.Close()
                Process1.Dispose()
                Process1 = Nothing
                starter = Nothing
                Return False
            End Try
        End Function
        Private Function TO_IMAGE_MIME_TYPES(ByVal ImageMime As String) As FDFApp.FDFDoc_Class.ImageFieldMime
            Select Case ImageMime.ToLower
                Case "image/jpg"
                    Return FDFDoc_Class.ImageFieldMime.JPG
                Case "image/jpeg"
                    Return FDFDoc_Class.ImageFieldMime.JPG
                Case "image/png"
                    Return FDFDoc_Class.ImageFieldMime.PNG
                Case "image/gif"
                    Return FDFDoc_Class.ImageFieldMime.GIF
                Case "image/bmp"
                    Return FDFDoc_Class.ImageFieldMime.BMP
                Case "image/x-emf"
                    Return FDFDoc_Class.ImageFieldMime.EMF
                Case Else
                    Return FDFDoc_Class.ImageFieldMime.JPG
            End Select
            Return 0
        End Function
#Region " IDisposable Support "
        Private disposedValue As Boolean = False                ' To detect redundant calls
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    Try
                        _FDFErrors = Nothing
                        _FDFMIME = Nothing
                        _HTMMIME = Nothing
                        _PDFMIME = Nothing
                        _TXTMIME = Nothing
                        _XDPMIME = Nothing
                        _FDFErrors.Dispose()
                        _FDFErrors = Nothing
                        FDFDox.Dispose()
                        FDFDox = Nothing
                    Catch ex As Exception
                    End Try
                End If
            End If
            Me.disposedValue = True
        End Sub
        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region
#Region "STRING FUNCTIONS"
        Protected Function Decimal_IsNullOrEmpty(ByVal o As Object) As Boolean
            If IsDBNull(o) Then
                Return True
            Else
                Return False
            End If
        End Function
        Protected Function Integer_IsNullOrEmpty(ByVal i As Object) As Boolean
            If IsDBNull(i) Then
                Return True
            Else
                Return False
            End If
        End Function
        Protected Function String_IsNullOrEmpty(ByVal s As Object) As Boolean
            Return String.IsNullOrEmpty(CStr(s))
        End Function
        Protected Function SNE(ByVal s As Object) As String
            If IsDBNull(s) Then
                Return String.Empty
            Else
                If String.Empty = CStr(s) Then
                    Return String.Empty
                Else
                    Return CType(s, String)
                End If
            End If
        End Function
#End Region
#Region "FULL VERSION"
        Public Sub New()
            Initialize()
        End Sub
        Private Sub Initialize()
            Try
                FDFDox = New FDFApp.FDFDoc_Class()
                FDFDox.Initialize(_defaultEncoding)
                _FDFErrors = New FDFErrors
                _FDFErrors.ThrowErrors = ThrowErrors
            Catch ex As Exception
                Throw ex
            End Try
        End Sub
        Public Function FDFCreate() As FDFDoc_Class
            Try
                FDFDox = New FDFApp.FDFDoc_Class()
                FDFDox.Initialize(_defaultEncoding)
                FDFDox.ThrowErrors = ThrowErrors
                Return FDFDox
            Catch ex As Exception
                Throw ex
            End Try
        End Function
#End Region
    End Class
End Namespace
