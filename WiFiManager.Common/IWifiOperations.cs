using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using WiFiManager.Common.BusinessObjects;



namespace WiFiManager.Common
{
    public interface IWifiOperations
    {
        MainPageVM GetActiveWifiNetworks();
        Task<MainPageVM> GetActiveWifiNetworksAsync();

        Task<Tuple<double, double, double>> GetCoordsAsync();

        Task GetActualCoordsAsync(WifiNetworkDto network);
        void Connect(string bssid, string ssid,string password);
    }
}
