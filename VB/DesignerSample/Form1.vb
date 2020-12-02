Imports DevExpress.XtraCharts
Imports MultiPaneExtension
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports System.Linq
Imports System.Text
Imports System.Threading.Tasks
Imports System.Windows.Forms

Namespace DesignerSample
	Partial Public Class Form1
		Inherits Form

		Public Sub New()
			InitializeComponent()
			dashboardDesigner1.CreateRibbon()
			Dim [module] As New MultiPaneModule()
			[module].Attach(dashboardDesigner1)

			dashboardDesigner1.LoadDashboard("Data/MultiPaneCharts.xml")
		End Sub
	End Class
End Namespace
