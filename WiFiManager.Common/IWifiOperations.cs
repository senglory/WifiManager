using System;
using System.Collections.Generic;
using System.Text;

namespace WiFiManager.Common
{
    public interface IWifiOperations
    {
        MainPageVM GetActiveWifiNetworks();
    }
}
