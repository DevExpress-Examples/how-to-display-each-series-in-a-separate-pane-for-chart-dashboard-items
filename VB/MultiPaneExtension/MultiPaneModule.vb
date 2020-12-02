Imports DevExpress.DashboardCommon
Imports DevExpress.DashboardWin
Imports DevExpress.XtraBars
Imports DevExpress.XtraBars.Ribbon
Imports DevExpress.XtraCharts
Imports DevExpress.XtraReports.UI
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks

Namespace MultiPaneExtension
	Public Class MultiPaneModule
		Private Const customPropertyName As String = "MultiPaneSettings"
		Private Const barButtonEnableCaption As String = "Autogenerate Panes"
		Private Const barButtonShowTitlesCaption As String = "Show Pane Titles"
		Private Const barButtonAllowCollapsingCaption As String = "Allow Pane Collapsing"
		Private Const barListLayoutCaption As String = "Pane Layout Mode"


		Private Const ribonPageGroupName As String = "Custom Properties"
		Private dashboardControl As IDashboardControl
		Private ReadOnly Property dashboardDesigner() As DashboardDesigner
			Get
				Return TryCast(dashboardControl, DashboardDesigner)
			End Get
		End Property
		Private enableBarItem As BarCheckItem
		Private showTitlesBarItem As BarCheckItem
		Private allowCollapsingBarItem As BarCheckItem
		Private layoutModeBarItem As BarListItem


		#Region "Assigning Logic"
		Public Sub Attach(ByVal dashboardControl As IDashboardControl)
			Detach()
			Me.dashboardControl = dashboardControl
			Me.dashboardControl.CalculateHiddenTotals = True
			AddHandler dashboardControl.DashboardItemControlUpdated, AddressOf DashboardItemControlUpdated
			AddHandler dashboardControl.CustomExport, AddressOf CustomExport

			If dashboardDesigner IsNot Nothing Then
				AddButtonToRibbon()
				AddHandler dashboardDesigner.DashboardItemSelected, AddressOf DashboardDesigner_DashboardItemSelected
				AddHandler dashboardDesigner.Dashboard.OptionsChanged, AddressOf Dashboard_OptionsChanged
			End If
		End Sub

		Private Sub Dashboard_OptionsChanged(ByVal sender As Object, ByVal e As DashboardOptionsChangedEventArgs)
			UpdateBarItems()
		End Sub

		Public Sub Detach()
			If dashboardControl Is Nothing Then
				Return
			End If
			If dashboardDesigner IsNot Nothing Then
				RemoveButtonFromRibbon()
			End If
			RemoveHandler dashboardControl.DashboardItemControlUpdated, AddressOf DashboardItemControlUpdated
			RemoveHandler dashboardControl.CustomExport, AddressOf CustomExport
			If dashboardDesigner IsNot Nothing Then
				RemoveButtonFromRibbon()
				RemoveHandler dashboardDesigner.DashboardItemSelected, AddressOf DashboardDesigner_DashboardItemSelected
			End If
			dashboardControl = Nothing
		End Sub
		#End Region

		#Region "Common Logic"
		Private Sub CustomExport(ByVal sender As Object, ByVal e As CustomExportEventArgs)
			For Each printControl In e.GetPrintableControls()
				If TypeOf printControl.Value Is XRChart Then
					Dim chartItemName = printControl.Key
					Dim dashboardControl As IDashboardControl = DirectCast(sender, IDashboardControl)
					Dim chartItem As ChartDashboardItem = TryCast(dashboardControl.Dashboard.Items(chartItemName), ChartDashboardItem)
					If chartItem Is Nothing OrElse chartItem.Panes.Count > 1 Then
						Return
					End If
					Dim xrChart As XRChart = TryCast(printControl.Value, XRChart)
					If xrChart Is Nothing Then
						Return
					End If
					Dim settings As MultiPaneSettings = MultiPaneSettings.FromJson(chartItem.CustomProperties(customPropertyName))
					CustomizeDiagram(TryCast(xrChart.Diagram, XYDiagram), xrChart.Series, settings)
				End If
			Next printControl
		End Sub
		Private Sub DashboardItemControlUpdated(ByVal sender As Object, ByVal e As DashboardItemControlEventArgs)
			Dim chartItem As ChartDashboardItem = TryCast(dashboardControl.Dashboard.Items(e.DashboardItemName), ChartDashboardItem)
			If chartItem Is Nothing OrElse chartItem.Panes.Count>1 Then
				Return
			End If
			Dim settings As MultiPaneSettings = MultiPaneSettings.FromJson(chartItem.CustomProperties(customPropertyName))
			CustomizeDiagram(TryCast(e.ChartControl.Diagram, XYDiagram), e.ChartControl.Series, settings)
		End Sub

		Private Sub CustomizeDiagram(ByVal diagram As XYDiagram, ByVal series As SeriesCollection, ByVal settings As MultiPaneSettings)
			If settings.MultiPaneEnabled Then
				diagram.PaneLayout.AutoLayoutMode = If(settings.UseGridLayout, PaneAutoLayoutMode.Grid, PaneAutoLayoutMode.Linear)
				diagram.RuntimePaneCollapse = settings.AllowPaneCollapsing
				Dim seriesWithPoints As List(Of Series) = series.Cast(Of Series)().Where(Function(s) s.Points.Any()).ToList()
				For i As Integer = 0 To seriesWithPoints.Count - 1
					If i <> 0 Then
						diagram.Panes.Add(New XYDiagramPane())
					End If
					Dim pane As XYDiagramPane = diagram.Panes(i)
					TryCast(seriesWithPoints(i).View, XYDiagramSeriesViewBase).Pane = pane
					If settings.ShowPaneTitles Then
						pane.Title.Visibility = DevExpress.Utils.DefaultBoolean.True
						pane.Title.Text = seriesWithPoints(i).Name
					Else
						pane.Title.Visibility = DevExpress.Utils.DefaultBoolean.False
					End If
				Next i
				'foreach (XYDiagramPane pane in diagram.Panes)
				'{
				'    if (diagram.Rotated)
				'    {
				'        if (pane == diagram.Panes[0])
				'            diagram.AxisX.SetVisibilityInPane(true, pane);
				'        else
				'            diagram.AxisX.SetVisibilityInPane(false, pane);
				'    }
				'    else //not rotated
				'    {
				'        if (pane == diagram.Panes[diagram.Panes.Count - 1])
				'            diagram.AxisX.SetVisibilityInPane(true, pane);
				'        else
				'            diagram.AxisX.SetVisibilityInPane(false, pane);
				'    }
				'}
			End If

		End Sub

		#End Region

		#Region "Designer Logic"
		Private Sub DashboardDesigner_DashboardItemSelected(ByVal sender As Object, ByVal e As DashboardItemSelectedEventArgs)
			UpdateBarItems()
		End Sub
		Private Sub UpdateBarItems()
			If TypeOf dashboardDesigner.SelectedDashboardItem Is ChartDashboardItem Then
				Dim chartItem As ChartDashboardItem = TryCast(dashboardDesigner.SelectedDashboardItem, ChartDashboardItem)
				If chartItem.Panes.Count>1 Then
'INSTANT VB WARNING: An assignment within expression was extracted from the following statement:
'ORIGINAL LINE: enableBarItem.Checked = showTitlesBarItem.Checked = allowCollapsingBarItem.Checked = false;
					allowCollapsingBarItem.Checked = False
'INSTANT VB WARNING: An assignment within expression was extracted from the following statement:
'ORIGINAL LINE: enableBarItem.Checked = showTitlesBarItem.Checked = allowCollapsingBarItem.Checked
					showTitlesBarItem.Checked = allowCollapsingBarItem.Checked
					enableBarItem.Checked = showTitlesBarItem.Checked
'INSTANT VB WARNING: An assignment within expression was extracted from the following statement:
'ORIGINAL LINE: enableBarItem.Enabled = showTitlesBarItem.Enabled = allowCollapsingBarItem.Enabled = layoutModeBarItem.Enabled = false;
					layoutModeBarItem.Enabled = False
'INSTANT VB WARNING: An assignment within expression was extracted from the following statement:
'ORIGINAL LINE: enableBarItem.Enabled = showTitlesBarItem.Enabled = allowCollapsingBarItem.Enabled = layoutModeBarItem.Enabled
					allowCollapsingBarItem.Enabled = layoutModeBarItem.Enabled
'INSTANT VB WARNING: An assignment within expression was extracted from the following statement:
'ORIGINAL LINE: enableBarItem.Enabled = showTitlesBarItem.Enabled = allowCollapsingBarItem.Enabled
					showTitlesBarItem.Enabled = allowCollapsingBarItem.Enabled
					enableBarItem.Enabled = showTitlesBarItem.Enabled
					Return
				End If
				Dim settings As MultiPaneSettings = MultiPaneSettings.FromJson(chartItem.CustomProperties(customPropertyName))
				enableBarItem.Enabled = True
				enableBarItem.Checked = settings.MultiPaneEnabled
				If settings.MultiPaneEnabled Then
					layoutModeBarItem.Enabled = True
					showTitlesBarItem.Enabled = True
					showTitlesBarItem.Checked = settings.ShowPaneTitles
				Else
					layoutModeBarItem.Enabled = False
					showTitlesBarItem.Enabled = False
					showTitlesBarItem.Checked = False
				End If
				If settings.MultiPaneEnabled AndAlso settings.ShowPaneTitles Then
					allowCollapsingBarItem.Enabled = True
					allowCollapsingBarItem.Checked = settings.AllowPaneCollapsing
				Else
					allowCollapsingBarItem.Enabled = False
					allowCollapsingBarItem.Checked = False
				End If
				layoutModeBarItem.ItemIndex = If(settings.UseGridLayout, 0, 1)
			End If

		End Sub
		Private Function CreateEnableBarItem() As BarCheckItem
			Dim barItem As New BarCheckItem()
			barItem.Caption = barButtonEnableCaption
			barItem.ImageOptions.SvgImage = Global.MultiPaneExtension.Properties.Resources.AddChartPaneButton
			barItem.RibbonStyle = RibbonItemStyles.All
			AddHandler barItem.ItemClick, AddressOf OnEnableClick
			Return barItem
		End Function
		Private Sub OnEnableClick(ByVal sender As Object, ByVal e As ItemClickEventArgs)
			Dim dashboardItem As ChartDashboardItem = TryCast(dashboardDesigner.SelectedDashboardItem, ChartDashboardItem)
			Dim settings As MultiPaneSettings = MultiPaneSettings.FromJson(dashboardItem.CustomProperties(customPropertyName))
			settings.MultiPaneEnabled = Not settings.MultiPaneEnabled
			Dim status As String = If(settings.MultiPaneEnabled = True, "enabled", "disabled")
			Dim historyItem As New CustomPropertyHistoryItem(dashboardItem, customPropertyName, settings.ToJson(), $"Autogenerate Panes for {dashboardItem.ComponentName} is {status}")
			dashboardDesigner.AddToHistory(historyItem)
			UpdateBarItems()
		End Sub
		Private Function CreateShowTitlesBarItem() As BarCheckItem
			Dim barItem As New BarCheckItem()
			barItem.Caption = barButtonShowTitlesCaption
			barItem.ImageOptions.SvgImage = Global.MultiPaneExtension.Properties.Resources.ShowPaneTitlesButton


			AddHandler barItem.ItemClick, AddressOf OnShowTitlesClick
			barItem.RibbonStyle = RibbonItemStyles.All
			Return barItem
		End Function
		Private Sub OnShowTitlesClick(ByVal sender As Object, ByVal e As ItemClickEventArgs)
			Dim dashboardItem As ChartDashboardItem = TryCast(dashboardDesigner.SelectedDashboardItem, ChartDashboardItem)
			Dim settings As MultiPaneSettings = MultiPaneSettings.FromJson(dashboardItem.CustomProperties(customPropertyName))
			settings.ShowPaneTitles = Not settings.ShowPaneTitles
			Dim status As String = If(settings.ShowPaneTitles = True, "enabled", "disabled")
			Dim historyItem As New CustomPropertyHistoryItem(dashboardItem, customPropertyName, settings.ToJson(), $"Pane Titles for {dashboardItem.ComponentName} is {status}")
			dashboardDesigner.AddToHistory(historyItem)
			UpdateBarItems()
		End Sub

		Private Function CreateAllowCollapsingBarItem() As BarCheckItem
			Dim barItem As New BarCheckItem()
			barItem.Caption = barButtonAllowCollapsingCaption
			barItem.ImageOptions.SvgImage = Global.MultiPaneExtension.Properties.Resources.CollapsePaneButton

			AddHandler barItem.ItemClick, AddressOf OnAllowCollapsingClick
			barItem.RibbonStyle = RibbonItemStyles.All
			Return barItem
		End Function
		Private Sub OnAllowCollapsingClick(ByVal sender As Object, ByVal e As ItemClickEventArgs)
			Dim dashboardItem As ChartDashboardItem = TryCast(dashboardDesigner.SelectedDashboardItem, ChartDashboardItem)
			Dim settings As MultiPaneSettings = MultiPaneSettings.FromJson(dashboardItem.CustomProperties(customPropertyName))
			settings.AllowPaneCollapsing = Not settings.AllowPaneCollapsing
			Dim status As String = If(settings.AllowPaneCollapsing = True, "enabled", "disabled")
			Dim historyItem As New CustomPropertyHistoryItem(dashboardItem, customPropertyName, settings.ToJson(), $"Pane Collapsing for {dashboardItem.ComponentName} is {status}")
			dashboardDesigner.AddToHistory(historyItem)
			UpdateBarItems()
		End Sub

		Private Function CreateLayoutModeBarItem() As BarListItem
			Dim barItem As New BarListItem()
			barItem.Caption = barListLayoutCaption
			barItem.ImageOptions.SvgImage = Global.MultiPaneExtension.Properties.Resources.LayoutModeButton
			barItem.ShowChecks = True
			barItem.Strings.Add("Grid")
			barItem.Strings.Add("Linear")
			AddHandler barItem.ListItemClick, AddressOf BarItem_ListItemClick
			barItem.RibbonStyle = RibbonItemStyles.All
			Return barItem
		End Function

		Private Sub BarItem_ListItemClick(ByVal sender As Object, ByVal e As ListItemClickEventArgs)
			Dim dashboardItem As ChartDashboardItem = TryCast(dashboardDesigner.SelectedDashboardItem, ChartDashboardItem)
			Dim settings As MultiPaneSettings = MultiPaneSettings.FromJson(dashboardItem.CustomProperties(customPropertyName))
			settings.UseGridLayout = e.Index=0
			Dim status As String = If(settings.UseGridLayout = True, "Grid", "Linear")
			Dim historyItem As New CustomPropertyHistoryItem(dashboardItem, customPropertyName, settings.ToJson(), $"Layout Mode for {dashboardItem.ComponentName} is {status}")
			dashboardDesigner.AddToHistory(historyItem)
			UpdateBarItems()
		End Sub

		'void OnAllowCollapsingClick(object sender, ItemClickEventArgs e)
		'{
		'    ChartDashboardItem dashboardItem = dashboardDesigner.SelectedDashboardItem as ChartDashboardItem;
		'    MultiPaneSettings settings = MultiPaneSettings.FromJson(dashboardItem.CustomProperties[customPropertyName]);
		'    settings.AllowPaneCollapsing = !settings.AllowPaneCollapsing;
		'    string status = settings.AllowPaneCollapsing == true ? "enabled" : "disabled";
		'    CustomPropertyHistoryItem historyItem = new CustomPropertyHistoryItem(dashboardItem, customPropertyName, settings.ToJson(), $"Pane Collapsing for {dashboardItem.ComponentName} is {status}");
		'    dashboardDesigner.AddToHistory(historyItem);
		'    UpdateBarItems();
		'}


		Private Sub AddButtonToRibbon()
			Dim ribbon As RibbonControl = dashboardDesigner.Ribbon
			Dim page As RibbonPage = ribbon.GetDashboardRibbonPage(DashboardBarItemCategory.ChartTools, DashboardRibbonPage.Design)
			Dim group As RibbonPageGroup = page.GetGroupByName(ribonPageGroupName)
			If group Is Nothing Then
				group = New RibbonPageGroup(ribonPageGroupName) With {.Name = ribonPageGroupName}
				page.Groups.Add(group)
			End If
			enableBarItem = CreateEnableBarItem()
			showTitlesBarItem = CreateShowTitlesBarItem()
			allowCollapsingBarItem = CreateAllowCollapsingBarItem()
			layoutModeBarItem = CreateLayoutModeBarItem()

			group.ItemLinks.Add(enableBarItem)
			group.ItemLinks.Add(showTitlesBarItem)
			group.ItemLinks.Add(allowCollapsingBarItem)
			group.ItemLinks.Add(layoutModeBarItem)

		End Sub
		Private Sub RemoveButtonFromRibbon()
			Dim ribbon As RibbonControl = dashboardDesigner.Ribbon
			Dim page As RibbonPage = ribbon.GetDashboardRibbonPage(DashboardBarItemCategory.PiesTools, DashboardRibbonPage.Design)
			Dim group As RibbonPageGroup = page.GetGroupByName(ribonPageGroupName)
			page.Groups.Remove(group)
		End Sub

		#End Region

	End Class
End Namespace
