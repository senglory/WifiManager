using System;
using System.Collections.Generic;
using System.Text;

namespace WiFiManager.Common.BusinessObjects
{
    public struct WifiNetwork
    {
        public string Name { get; set; }
        public string BssID { get; set; }
        public string NetworkType { get; set; }
        public string Password { get; set; }
        public string WpsPin { get; set; }
        public int Level { get; set; }
        public string Provider { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime? FirstConnectWhen { get; set; }
        public string FirstConnectMac { get; set; }
        public string FirstConnectPublicIP { get; set; }
	public string RouterWebUIIP { get; set; }
	public double? FirstCoordLat { get; set; }
        public double? FirstCoordLong { get; set; }
        public double? FirstCoordAlt { get; set; }
        public double? LastCoordLat { get; set; }
        public double? LastCoordLong { get; set; }
        public double? LastCoordAlt { get; set; }
    }
}
