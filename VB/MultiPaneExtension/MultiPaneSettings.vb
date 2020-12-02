Imports Newtonsoft.Json
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks

Namespace MultiPaneExtension
	Friend Class MultiPaneSettings
		Public Property MultiPaneEnabled() As Boolean
		Public Property ShowPaneTitles() As Boolean
		Public Property AllowPaneCollapsing() As Boolean
		Public Property UseGridLayout() As Boolean

		Public Sub New()
			MultiPaneEnabled = False
			ShowPaneTitles = True
			AllowPaneCollapsing = True
			UseGridLayout = True
		End Sub

		Public Shared Function FromJson(ByVal json As String) As MultiPaneSettings
			If String.IsNullOrEmpty(json) Then
				Return New MultiPaneSettings()
			End If
			Return TryCast(JsonConvert.DeserializeObject(Of MultiPaneSettings)(json), MultiPaneSettings)
		End Function

		Public Function ToJson() As String
			Return JsonConvert.SerializeObject(Me)
		End Function


	End Class
End Namespace
