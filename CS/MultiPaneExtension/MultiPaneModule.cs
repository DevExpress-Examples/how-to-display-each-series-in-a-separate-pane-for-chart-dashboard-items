using DevExpress.DashboardCommon;
using DevExpress.DashboardWin;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraCharts;
using DevExpress.XtraReports.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiPaneExtension
{
    public class MultiPaneModule
    {
        const string customPropertyName = "MultiPaneSettings";
        const string barButtonEnableCaption = "Autogenerate Panes";
        const string barButtonShowTitlesCaption = "Show Pane Titles";
        const string barButtonAllowCollapsingCaption = "Allow Pane Collapsing";
        const string barListLayoutCaption = "Pane Layout Mode";


        const string ribonPageGroupName = "Custom Properties";
        IDashboardControl dashboardControl;
        DashboardDesigner dashboardDesigner
        {
            get { return dashboardControl as DashboardDesigner; }
        }
        BarCheckItem enableBarItem;
        BarCheckItem showTitlesBarItem;
        BarCheckItem allowCollapsingBarItem;
        BarListItem layoutModeBarItem;


        #region Assigning Logic
        public void Attach(IDashboardControl dashboardControl)
        {
            Detach();
            this.dashboardControl = dashboardControl;
            this.dashboardControl.CalculateHiddenTotals = true;
            this.dashboardControl.DashboardItemControlUpdated += DashboardItemControlUpdated;
            this.dashboardControl.CustomExport += CustomExport;

            if (dashboardDesigner != null)
            {
                AddButtonToRibbon();
                dashboardDesigner.DashboardItemSelected += DashboardDesigner_DashboardItemSelected;
                dashboardDesigner.Dashboard.OptionsChanged += Dashboard_OptionsChanged;
            }
        }

        private void Dashboard_OptionsChanged(object sender, DashboardOptionsChangedEventArgs e)
        {
            UpdateBarItems();
        }

        public void Detach()
        {
            if (dashboardControl == null) return;
            if (dashboardDesigner != null)
                RemoveButtonFromRibbon();
            this.dashboardControl.DashboardItemControlUpdated -= DashboardItemControlUpdated;
            this.dashboardControl.CustomExport -= CustomExport;
            if (dashboardDesigner != null)
            {
                RemoveButtonFromRibbon();
                dashboardDesigner.DashboardItemSelected -= DashboardDesigner_DashboardItemSelected;
            }
            dashboardControl = null;
        }
        #endregion

        #region Common Logic
        private void CustomExport(object sender, CustomExportEventArgs e)
        {
            foreach (var printControl in e.GetPrintableControls())
            {
                if (printControl.Value is XRChart)
                {
                    var chartItemName = printControl.Key;
                    IDashboardControl dashboardControl = (IDashboardControl)sender;
                    ChartDashboardItem chartItem = dashboardControl.Dashboard.Items[chartItemName] as ChartDashboardItem;
                    if (chartItem == null || chartItem.Panes.Count > 1) return;
                    XRChart xrChart = printControl.Value as XRChart;
                    if (xrChart == null ) return;
                    MultiPaneSettings settings = MultiPaneSettings.FromJson(chartItem.CustomProperties[customPropertyName]);
                    CustomizeDiagram(xrChart.Diagram as XYDiagram, xrChart.Series, settings);
                }
            }
        }
        private void DashboardItemControlUpdated(object sender, DashboardItemControlEventArgs e)
        {
            ChartDashboardItem chartItem = dashboardControl.Dashboard.Items[e.DashboardItemName] as ChartDashboardItem;
            if (chartItem == null || chartItem.Panes.Count>1) return;
            MultiPaneSettings settings = MultiPaneSettings.FromJson(chartItem.CustomProperties[customPropertyName]);
            CustomizeDiagram(e.ChartControl.Diagram as XYDiagram, e.ChartControl.Series, settings);
        }

        void CustomizeDiagram(XYDiagram diagram, SeriesCollection series, MultiPaneSettings settings)
        {
            if (settings.MultiPaneEnabled)
            {
                diagram.PaneLayout.AutoLayoutMode = settings.UseGridLayout ? PaneAutoLayoutMode.Grid : PaneAutoLayoutMode.Linear;
                diagram.RuntimePaneCollapse = settings.AllowPaneCollapsing;
                List<Series> seriesWithPoints = series.Cast<Series>().Where(s => s.Points.Any()).ToList();
                for (int i = 0; i < seriesWithPoints.Count; i++)
                {
                    if (i != 0)
                        diagram.Panes.Add(new XYDiagramPane());
                    XYDiagramPane pane = diagram.Panes[i];
                    (seriesWithPoints[i].View as XYDiagramSeriesViewBase).Pane = pane;
                    if (settings.ShowPaneTitles)
                    {
                        pane.Title.Visibility = DevExpress.Utils.DefaultBoolean.True;
                        pane.Title.Text = seriesWithPoints[i].Name;
                    }
                    else
                        pane.Title.Visibility = DevExpress.Utils.DefaultBoolean.False;
                }
                //foreach (XYDiagramPane pane in diagram.Panes)
                //{
                //    if (diagram.Rotated)
                //    {
                //        if (pane == diagram.Panes[0])
                //            diagram.AxisX.SetVisibilityInPane(true, pane);
                //        else
                //            diagram.AxisX.SetVisibilityInPane(false, pane);
                //    }
                //    else //not rotated
                //    {
                //        if (pane == diagram.Panes[diagram.Panes.Count - 1])
                //            diagram.AxisX.SetVisibilityInPane(true, pane);
                //        else
                //            diagram.AxisX.SetVisibilityInPane(false, pane);
                //    }
                //}
            }

        }

        #endregion

        #region Designer Logic
        private void DashboardDesigner_DashboardItemSelected(object sender, DashboardItemSelectedEventArgs e)
        {
            UpdateBarItems();
        }
        void UpdateBarItems()
        {
            if (dashboardDesigner.SelectedDashboardItem is ChartDashboardItem)
            {
                ChartDashboardItem chartItem = dashboardDesigner.SelectedDashboardItem as ChartDashboardItem;
                if(chartItem.Panes.Count>1)
                {
                    enableBarItem.Checked = showTitlesBarItem.Checked = allowCollapsingBarItem.Checked = false;
                    enableBarItem.Enabled = showTitlesBarItem.Enabled = 
                        allowCollapsingBarItem.Enabled = layoutModeBarItem.Enabled = false;
                    return;
                }
                MultiPaneSettings settings = MultiPaneSettings.FromJson(chartItem.CustomProperties[customPropertyName]);
                enableBarItem.Enabled = true;
                enableBarItem.Checked = settings.MultiPaneEnabled;
                if (settings.MultiPaneEnabled)
                {
                    layoutModeBarItem.Enabled = true;
                    showTitlesBarItem.Enabled = true;
                    showTitlesBarItem.Checked = settings.ShowPaneTitles;
                }
                else
                {
                    layoutModeBarItem.Enabled = false;
                    showTitlesBarItem.Enabled = false;
                    showTitlesBarItem.Checked = false;
                }
                if (settings.MultiPaneEnabled && settings.ShowPaneTitles)
                {
                    allowCollapsingBarItem.Enabled = true;
                    allowCollapsingBarItem.Checked = settings.AllowPaneCollapsing;
                }
                else
                {
                    allowCollapsingBarItem.Enabled = false;
                    allowCollapsingBarItem.Checked = false;
                }
                layoutModeBarItem.ItemIndex = settings.UseGridLayout? 0:1;
            }

        }
        BarCheckItem CreateEnableBarItem()
        {
            BarCheckItem barItem = new BarCheckItem();
            barItem.Caption = barButtonEnableCaption;
            barItem.ImageOptions.SvgImage = global::MultiPaneExtension.Properties.Resources.AddChartPaneButton;
            barItem.RibbonStyle = RibbonItemStyles.All;
            barItem.ItemClick += OnEnableClick;
            return barItem;
        }
        private void OnEnableClick(object sender, ItemClickEventArgs e)
        {
            ChartDashboardItem dashboardItem = dashboardDesigner.SelectedDashboardItem as ChartDashboardItem;
            MultiPaneSettings settings = MultiPaneSettings.FromJson(dashboardItem.CustomProperties[customPropertyName]);
            settings.MultiPaneEnabled = !settings.MultiPaneEnabled;
            string status = settings.MultiPaneEnabled == true ? "enabled" : "disabled";
            CustomPropertyHistoryItem historyItem = new CustomPropertyHistoryItem(dashboardItem, customPropertyName, settings.ToJson(), $"Autogenerate Panes for {dashboardItem.ComponentName} is {status}");
            dashboardDesigner.AddToHistory(historyItem);
            UpdateBarItems();
        }
        BarCheckItem CreateShowTitlesBarItem()
        {
            BarCheckItem barItem = new BarCheckItem();
            barItem.Caption = barButtonShowTitlesCaption;
            barItem.ImageOptions.SvgImage = global::MultiPaneExtension.Properties.Resources.ShowPaneTitlesButton;


            barItem.ItemClick += OnShowTitlesClick;
            barItem.RibbonStyle = RibbonItemStyles.All;
            return barItem;
        }
        void OnShowTitlesClick(object sender, ItemClickEventArgs e)
        {
            ChartDashboardItem dashboardItem = dashboardDesigner.SelectedDashboardItem as ChartDashboardItem;
            MultiPaneSettings settings = MultiPaneSettings.FromJson(dashboardItem.CustomProperties[customPropertyName]);
            settings.ShowPaneTitles = !settings.ShowPaneTitles;
            string status = settings.ShowPaneTitles == true ? "enabled" : "disabled";
            CustomPropertyHistoryItem historyItem = new CustomPropertyHistoryItem(dashboardItem, customPropertyName, settings.ToJson(), $"Pane Titles for {dashboardItem.ComponentName} is {status}");
            dashboardDesigner.AddToHistory(historyItem);
            UpdateBarItems();
        }

        BarCheckItem CreateAllowCollapsingBarItem()
        {
            BarCheckItem barItem = new BarCheckItem();
            barItem.Caption = barButtonAllowCollapsingCaption;
            barItem.ImageOptions.SvgImage = global::MultiPaneExtension.Properties.Resources.CollapsePaneButton;

            barItem.ItemClick += OnAllowCollapsingClick;
            barItem.RibbonStyle = RibbonItemStyles.All;
            return barItem;
        }
        void OnAllowCollapsingClick(object sender, ItemClickEventArgs e)
        {
            ChartDashboardItem dashboardItem = dashboardDesigner.SelectedDashboardItem as ChartDashboardItem;
            MultiPaneSettings settings = MultiPaneSettings.FromJson(dashboardItem.CustomProperties[customPropertyName]);
            settings.AllowPaneCollapsing = !settings.AllowPaneCollapsing;
            string status = settings.AllowPaneCollapsing == true ? "enabled" : "disabled";
            CustomPropertyHistoryItem historyItem = new CustomPropertyHistoryItem(dashboardItem, customPropertyName, settings.ToJson(), $"Pane Collapsing for {dashboardItem.ComponentName} is {status}");
            dashboardDesigner.AddToHistory(historyItem);
            UpdateBarItems();
        }

        BarListItem CreateLayoutModeBarItem()
        {
            BarListItem barItem = new BarListItem();
            barItem.Caption = barListLayoutCaption;
            barItem.ImageOptions.SvgImage = global::MultiPaneExtension.Properties.Resources.LayoutModeButton;
            barItem.ShowChecks = true;
            barItem.Strings.Add("Grid");
            barItem.Strings.Add("Linear");
            barItem.ListItemClick += BarItem_ListItemClick;
            barItem.RibbonStyle = RibbonItemStyles.All;
            return barItem;
        }

        private void BarItem_ListItemClick(object sender, ListItemClickEventArgs e)
        {
            ChartDashboardItem dashboardItem = dashboardDesigner.SelectedDashboardItem as ChartDashboardItem;
            MultiPaneSettings settings = MultiPaneSettings.FromJson(dashboardItem.CustomProperties[customPropertyName]);
            settings.UseGridLayout = e.Index==0;
            string status = settings.UseGridLayout == true ? "Grid" : "Linear";
            CustomPropertyHistoryItem historyItem = new CustomPropertyHistoryItem(dashboardItem, customPropertyName, settings.ToJson(), $"Layout Mode for {dashboardItem.ComponentName} is {status}");
            dashboardDesigner.AddToHistory(historyItem);
            UpdateBarItems();
        }

        //void OnAllowCollapsingClick(object sender, ItemClickEventArgs e)
        //{
        //    ChartDashboardItem dashboardItem = dashboardDesigner.SelectedDashboardItem as ChartDashboardItem;
        //    MultiPaneSettings settings = MultiPaneSettings.FromJson(dashboardItem.CustomProperties[customPropertyName]);
        //    settings.AllowPaneCollapsing = !settings.AllowPaneCollapsing;
        //    string status = settings.AllowPaneCollapsing == true ? "enabled" : "disabled";
        //    CustomPropertyHistoryItem historyItem = new CustomPropertyHistoryItem(dashboardItem, customPropertyName, settings.ToJson(), $"Pane Collapsing for {dashboardItem.ComponentName} is {status}");
        //    dashboardDesigner.AddToHistory(historyItem);
        //    UpdateBarItems();
        //}


        void AddButtonToRibbon()
        {
            RibbonControl ribbon = dashboardDesigner.Ribbon;
            RibbonPage page = ribbon.GetDashboardRibbonPage(DashboardBarItemCategory.ChartTools, DashboardRibbonPage.Design);
            RibbonPageGroup group = page.GetGroupByName(ribonPageGroupName);
            if (group == null)
            {
                group = new RibbonPageGroup(ribonPageGroupName) { Name = ribonPageGroupName };
                page.Groups.Add(group);
            }
            enableBarItem = CreateEnableBarItem();
            showTitlesBarItem = CreateShowTitlesBarItem();
            allowCollapsingBarItem = CreateAllowCollapsingBarItem();
            layoutModeBarItem = CreateLayoutModeBarItem();

            group.ItemLinks.Add(enableBarItem);
            group.ItemLinks.Add(showTitlesBarItem);
            group.ItemLinks.Add(allowCollapsingBarItem);
            group.ItemLinks.Add(layoutModeBarItem);

        }
        void RemoveButtonFromRibbon()
        {
            RibbonControl ribbon = dashboardDesigner.Ribbon;
            RibbonPage page = ribbon.GetDashboardRibbonPage(DashboardBarItemCategory.PiesTools, DashboardRibbonPage.Design);
            RibbonPageGroup group = page.GetGroupByName(ribonPageGroupName);
            page.Groups.Remove(group);
        }

        #endregion

    }
}
