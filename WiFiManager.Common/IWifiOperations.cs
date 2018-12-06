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
        List<WifiNetworkDto> GetWifiNetworksFromCSV(out string firstFailedLine);

        void SaveToCSV(List<WifiNetworkDto> wifiNetworks);
        void SaveToJSON(List<WifiNetworkDto> wifiNetworks);

        bool CanLoadFromFile();

        bool IsConnected();

        void DeleteInfoAboutWifiNetworks();

        Task<List<WifiNetworkDto>> GetActiveWifiNetworksAsync();

        Task<Tuple<double, double, double>> GetCoordsAsync();

        Task ActualizeCoordsWifiNetworkAsync(WifiNetworkDto network);
        Task<WifiConnectionInfo>  ConnectAsync(WifiNetworkDto nw);

        Task DisConnectAsync();

        bool UsePhoneMemory { get; set; }
    }
}
