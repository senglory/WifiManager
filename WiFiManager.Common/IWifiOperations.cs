using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WiFiManager.Common
{
    public interface IWifiOperations
    {
        MainPageVM GetActiveWifiNetworks();
        Task<MainPageVM> GetActiveWifiNetworksAsync();
    }
}
