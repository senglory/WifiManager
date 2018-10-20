using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
 

using WiFiManager.Common.BusinessObjects;



namespace WiFiManager.Common
{
    public delegate void ConnectionSTateHandler(string connectionState);

    public interface IWifiOperations
    {
        List<WifiNetworkDto> GetActiveWifiNetworks();
        List<WifiNetworkDto> GetWifiNetworksFromCSV( );

        void SaveToCSV(List<WifiNetworkDto> wifiNetworks);
        void SaveToJSON(List<WifiNetworkDto> wifiNetworks);

        bool CanLoadFromFile();

        bool IsConnected();

        Task<List<WifiNetworkDto>> GetActiveWifiNetworksAsync();

        Task<Tuple<double, double, double>> GetCoordsAsync();

        Task ActualizeCoordsWifiNetworkAsync(WifiNetworkDto network);
        Task<WifiInfoInternal>  ConnectAsync(string bssid, string ssid,string password);

        Task DisConnectAsync();
    }
}
