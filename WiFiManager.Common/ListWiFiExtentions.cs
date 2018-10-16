using System.Linq;
using System.Collections.Generic;
using System.Text;
using WiFiManager.Common.BusinessObjects;

namespace WiFiManager.Common
{
    public static   class ListWiFiExtentions
    {
        public static WifiNetworkDto GetExistingWifiDto(this List<WifiNetworkDto> lst1, WifiNetworkDto wifiDtoFromFile)
        {
            if (string.IsNullOrEmpty(wifiDtoFromFile.BssID))
            {
                var foundByName = lst1.FirstOrDefault(r => r.Name == wifiDtoFromFile.Name);
                return foundByName;
            }
            var foundByBssId = lst1.FirstOrDefault(r => r.BssID .ToUpper ()== wifiDtoFromFile.BssID.ToUpper());
            return foundByBssId;
        }

    }
}
