using System;
using System.Collections.Generic;
using System.Text;

namespace WiFiManager.Common.BusinessObjects
{
    public class WifiNetwork : BaseObj
    {
        public string BssID { get; set; }
        public string NetworkType { get; set; }
        public string Password { get; set; }
        public string WpsPin { get; set; }

        public List<CoordsAndPower> CoordsAndPower { get; set; }

        public WifiNetwork()
        {
            CoordsAndPower = new List<CoordsAndPower>();
        }

        public override bool Fit(string filter)
        {
            var r = base.Fit(filter);
            return r || BssID.Contains(filter);
        }
    }
}
