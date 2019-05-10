using System;
using System.Collections.Generic;
using System.Text;

namespace WiFiManager.Common
{
    public class WifiConnectionInfo
    {
        public string FirstConnectMac;
        public DateTime FirstConnectWhen;
        public string FirstConnectPublicIP;
        public string RouterWebUIIP;
        public double FirstCoordLat;
        public double FirstCoordLong;
        public double FirstCoordAlt;
    }
}
