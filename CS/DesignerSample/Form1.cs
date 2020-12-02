using DevExpress.XtraCharts;
using MultiPaneExtension;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesignerSample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            dashboardDesigner1.CreateRibbon();
            MultiPaneModule module = new MultiPaneModule();
            module.Attach(dashboardDesigner1);

            dashboardDesigner1.LoadDashboard(@"Data/MultiPaneCharts.xml");
        }
    }
}
