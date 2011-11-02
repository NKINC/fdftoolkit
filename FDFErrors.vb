Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Web
Imports System.Web.Mail
'Imports System.Web.Mail.MailFormat
Imports System.Diagnostics.Process
Imports System.Data.OleDb
Imports System.Data
' MAILMESSAGE CLASS
Namespace FDFApp
	' FDFERRORS CLASS
	Public Class FDFErrors
		Implements IDisposable
		Enum FDFErc
			FDFErcOK = 0
			FDFErcInternalError = 1				 'Internal FDF Library error */
			FDFErcBadParameter = 2				' One or more of the parameters passed to the function are invalid */
			FDFErcFileSysErr = 3			  ' Some error using the file system,including file not found"
			FDFErcBadFDF = 4			' The FDF file being opened/parsed is invalid */
			FDFErcFieldNotFound = 5				'- The field whose name was passed in parameter "fieldName" does not exist in the FDF */
			FDFErcNoValue = 6			 ' The field whose value was requested has no value */
			FDFErcEnumStopped = 7			  ' Enumeration was stopped by FDFEnumValuesProc by returning false */
			FDFErcCantInsertField = 8			' The field whose name was passed in parameter "fieldName" can't be inserted into the FDF. This might happen, for example, if you try to insert "a.b" into an FDF that already has a field such as "a.b.c" in it. Or, conversely, if you try to insert "a.b.c" into an FDF that already has "a.b" */
			FDFErcNoOption = 9			  ' The requested element in a field's /Opt key does not exist, or the field has no /Opt key */
			FDFErcNoFlags = 10			  ' The field has no /F or /Ff keys */
			FDFErcBadPDF = 11			  ' The PDF passed as parameter to FDFSetAP is invalid, or doesn't contain pageNum */
			FDFErcBufTooShort = 12				  ' The buffer passed as parameter is too short for the length of the data that the function wants to return in it */
			FDFErcNoAP = 13				  ' The field has no /AP key */
			FDFErcIncompatibleFDF = 14				  ' Cannot mix "classic" and "template-based" FDF (see FDFAddTemplate) */
			FDFErcNoAppendSaves = 15			  ' The FDF does not include a /Differences key */
			FDFErcValueIsArray = 16				' The value of this field is an array. Use FDFGetNthValue */
			FDFErcEmbeddedFDFs = 17				  ' The FDF that you passed as parameter is a container for one or more FDFs embedded within it. Use FDFOpenFromEmbedded to gain access to each embedded FDF */
			FDFErcNoMoreFDFs = 18			  ' Returned by FDFOpenFromEmbedded when parameter iWhich >= to the number of embedded FDFs (including the case when the passed FDF does not contain any embedded FDFs) */
			FDFErcInvalidPassword = 19				  ' Returned by FDFOpenFromEmbedded when the embedded FDF is encrypted, and the correct password to open it was not provided */
			FDFErcLast = 20				' Unknown
			FDFErcUnknown = 50			 ' Unknown
		End Enum
		Private Function ReturnErrCodeStr(ByVal intCode As Integer) As String
			Select Case intCode
				Case 0
					Return "FDFErcOK"
				Case 1
					Return "FDFErcInternalError"
				Case 2
					Return "FDFErcBadParamete"
				Case 3
					Return "FDFErcFileSysErr"
				Case 4
					Return "FDFErcBadFDF"
				Case 5
					Return "FDFErcFieldNotFound"
				Case 6
					Return "FDFErcNoValue"
				Case 7
					Return "FDFErcEnumStopped"
				Case 8
					Return "FDFErcCantInsertField"
				Case 9
					Return "FDFErcNoOption"
				Case 10
					Return "FDFErcNoFlags"
				Case 11
					Return "FDFErcBadPDF"
				Case 12
					Return "FDFErcBufTooShort"
				Case 13
					Return "FDFErcNoAP"
				Case 14
					Return "FDFErcIncompatibleFDF"
				Case 15
					Return "FDFErcNoAppendSaves"
				Case 16
					Return "FDFErcValueIsArray"
				Case 17
					Return "FDFErcEmbeddedFDFs"
				Case 18
					Return "FDFErcNoMoreFDFs"
				Case 19
					Return "FDFErcInvalidPassword"
				Case 20
					Return "FDFErcLast"
				Case 50
					Return "FDFErcUnknown"
				Case Else
					Return "FDFErcUnknown"
			End Select
		End Function
		Structure FDFError
			Dim FDFError As FDFErc
			Dim FDFError_Msg As String
			Dim FDFError_Module As String
			Dim FDFError_Number As Integer
			Dim FDFError_Code As String
		End Structure
		Private _FDFErrors(0) As FDFError
		Public Sub FDFAddError(ByVal FDFErrCode As FDFErc, ByVal FDFErrMessage As String, ByVal FDFErrModule As String, ByVal FDFErrNumber As Integer)
			If Not _FDFErrors Is Nothing Then
				ReDim Preserve _FDFErrors(_FDFErrors.Length)
				_FDFErrors(_FDFErrors.Length - 1).FDFError = FDFErrCode
				_FDFErrors(_FDFErrors.Length - 1).FDFError_Module = FDFErrModule
				_FDFErrors(_FDFErrors.Length - 1).FDFError_Number = FDFErrNumber
				_FDFErrors(_FDFErrors.Length - 1).FDFError_Msg = FDFErrMessage
				_FDFErrors(_FDFErrors.Length - 1).FDFError_Code = ReturnErrCodeStr(FDFErrNumber)
			ElseIf Not FDFErrCode = FDFErc.FDFErcOK Then
				ReDim Preserve _FDFErrors(0)
				_FDFErrors(0).FDFError = FDFErrCode
				_FDFErrors(0).FDFError_Module = FDFErrModule
				_FDFErrors(0).FDFError_Number = FDFErrNumber
				_FDFErrors(0).FDFError_Msg = FDFErrMessage
				_FDFErrors(0).FDFError_Code = ReturnErrCodeStr(FDFErrNumber)
			End If
		End Sub
		Public Function FDFHasErrors() As Boolean
			If _FDFErrors(0).FDFError = FDFErc.FDFErcOK Then
				Return False
			Else
				Return True
			End If
		End Function
		Public Sub ResetErrors()
			ReDim _FDFErrors(0)
			_FDFErrors(0).FDFError = FDFErc.FDFErcOK
			_FDFErrors(0).FDFError_Msg = "OK"
			_FDFErrors(0).FDFError_Number = -1
			_FDFErrors(0).FDFError_Module = ""
		End Sub
		Public Property FDFErrors() As FDFError()
			Get
				Return _FDFErrors
			End Get
			Set(ByVal Value As FDFError())
				_FDFErrors = Value
			End Set
		End Property
		Public Function FDFErrorsStr(Optional ByVal HTMLFormat As Boolean = False) As String
			Dim FDFErrorx As FDFError
			Dim retString As String
			retString = IIf(HTMLFormat, "<br>", vbNewLine) & "FDF Errors:"
			For Each FDFErrorx In FDFErrors
				retString = retString & IIf(HTMLFormat, "<br>", vbNewLine) & vbTab & "Error: " & FDFErrorx.FDFError_Code & " - " & FDFErrorx.FDFError & IIf(HTMLFormat, "<br>", vbNewLine) & vbTab & "#: " & FDFErrorx.FDFError_Number & IIf(HTMLFormat, "<br>", vbNewLine) & vbTab & "Module: " & FDFErrorx.FDFError_Module & IIf(HTMLFormat, "<br>", vbNewLine) & vbTab & "Message: " & FDFErrorx.FDFError_Msg & IIf(HTMLFormat, "<br>", vbNewLine)
			Next
			Return retString
		End Function
#Region " IDisposable Support "
		Private disposedValue As Boolean = False		  ' To detect redundant calls

		' IDisposable
		Protected Overridable Sub Dispose(ByVal disposing As Boolean)
			If Not Me.disposedValue Then
				If disposing Then
					' TODO: free other state (managed objects).
					_FDFErrors = Nothing
				End If

				' TODO: free your own state (unmanaged objects).
				' TODO: set large fields to null.
			End If
			Me.disposedValue = True
		End Sub


		' This code added by Visual Basic to correctly implement the disposable pattern.
		Public Sub Dispose() Implements IDisposable.Dispose
			' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
			Dispose(True)
			GC.SuppressFinalize(Me)
		End Sub
#End Region

	End Class
End Namespace