using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace WiFiManager.Common.BusinessObjects
{
    public class WifiNetwork : BaseObj
    {
        public bool IsEnabled { get; set; }
        public string BssID { get; set; }
        public string NetworkType { get; set; }
        public string Password { get; set; }
        public string WpsPin { get; set; }

        public ObservableCollection<CoordsAndPower> CoordsAndPower { get; set; }

        public WifiNetwork()
        {
            IsEnabled = true;
            CoordsAndPower = new ObservableCollection<CoordsAndPower>();
        }

        public override bool Fit(string filter)
        {
            var r = base.Fit(filter);
            return r || BssID.Contains(filter);
        }
    }
}
