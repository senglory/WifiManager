using System;
using System.Collections.Generic;
using System.Text;

namespace WiFiManager.Common.BusinessObjects
{
    public class CoordsAndPower
    {
        public DateTime When { get; set; }
        public string IP { get; set; }
        public double Power { get; set; }
        public double Long { get; set; }
        public double Lat { get; set; }
        public double Alt { get; set; }
    }

}
