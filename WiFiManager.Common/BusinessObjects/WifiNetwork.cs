using System;
using System.Collections.Generic;
using System.Text;

namespace WiFiManager.Common.BusinessObjects
{
    public class WifiNetwork
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
    }
}
