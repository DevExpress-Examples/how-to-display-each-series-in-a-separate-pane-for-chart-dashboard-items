using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiPaneExtension
{
    class MultiPaneSettings
    {
        public bool MultiPaneEnabled { get; set; }
        public bool ShowPaneTitles { get; set; }
        public bool AllowPaneCollapsing { get; set; }
        public bool UseGridLayout { get; set; }

        public MultiPaneSettings()
        {
            MultiPaneEnabled = false;
            ShowPaneTitles = true;
            AllowPaneCollapsing = true;
            UseGridLayout = true;
        }

        public static MultiPaneSettings FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return new MultiPaneSettings();
            return JsonConvert.DeserializeObject<MultiPaneSettings>(json) as MultiPaneSettings;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }


    }
}
