using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
 
using WiFiManager.Common.BusinessObjects;



namespace WiFiManager.Common
{
    public delegate void ConnectionSTateHandler(string connectionState);

    public interface IWifiManagerOperations
    {
        List<WifiNetworkDto> GetActiveWifiNetworks();
        WifiNetworkDto FindWifiInCSV(WifiNetworkDto nw );

        void MoveCSVFromBluetoothFolder();

        IEnumerable<WifiNetworkDto> FindWifiInCSV(string wifiNameOrBssId);

        Task SaveToCSVAsync(List<WifiNetworkDto> wifiNetworks);
        Task DumpRawListAsync(List<WifiNetworkDto> wifiNetworks);

        bool CanLoadFromFile { get; }

        bool IsConnected();

        void DeleteInfoAboutWifiNetworks();

        Task<Tuple<double, double, double>> GetCoordsAsync();

        Task ActualizeCoordsWifiNetworkAsync(WifiNetworkDto network);
        Task<WifiConnectionInfo>  ConnectAsync(WifiNetworkDto network);

        Task DisConnectAsync();

        bool UseInternalStorageForCSV { get; set; }
        bool UseCachedNetworkLookup { get; set; }
        void ClearCachedCSVNetworkList();

        void CreateUnixFiles(WifiNetworkDto network);
    }
}
