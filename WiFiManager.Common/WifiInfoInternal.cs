using System;
using System.Collections.Generic;
using System.Text;

namespace WiFiManager.Common
{
    public class WifiInfoInternal
    {
        //
        // Summary:
        //     Returns the service set identifier (SSID) of the current 802.11 network.
        //
        // Remarks:
        //     Returns the service set identifier (SSID) of the current 802.11 network. If the
        //     SSID can be decoded as UTF-8, it will be returned surrounded by double quotation
        //     marks. Otherwise, it is returned as a string of hex digits. The SSID may be <unknown
        //     ssid> if there is no network currently connected.
        //     [Android Documentation]
        public virtual string SSID { get; }
        //
        // Summary:
        //     Returns the received signal strength indicator of the current 802.11 network,
        //     in dBm.
        //
        // Remarks:
        //     Returns the received signal strength indicator of the current 802.11 network,
        //     in dBm.
        //     Use Android.Net.Wifi.WifiManager.CalculateSignalLevel(System.Int32, System.Int32)
        //     to convert this number into an absolute signal level which can be displayed to
        //     a user.
        //     [Android Documentation]
        public virtual int Rssi { get; }
        //
        // Summary:
        //     Each configured network has a unique small integer ID, used to identify the network
        //     when performing operations on the supplicant.
        //
        // Remarks:
        //     Each configured network has a unique small integer ID, used to identify the network
        //     when performing operations on the supplicant. This method returns the ID for
        //     the currently connected network.
        //     [Android Documentation]
        public virtual int NetworkId { get; }
        //
        // Remarks:
        //     [Android Documentation]
        public virtual string MacAddress { get; set; }
        //
        // Summary:
        //     Returns the current link speed in Android.Net.Wifi.WifiInfo.LinkSpeedUnits.
        //
        // Remarks:
        //     Returns the current link speed in Android.Net.Wifi.WifiInfo.LinkSpeedUnits.
        //     [Android Documentation]
        public virtual int LinkSpeed { get; }
        //
        // Remarks:
        //     [Android Documentation]
        public virtual int IpAddress { get; }
        //
        // Remarks:
        //     [Android Documentation]
        public virtual bool HiddenSSID { get; }
        //
        // Summary:
        //     Return the basic service set identifier (BSSID) of the current access point.
        //
        // Remarks:
        //     Return the basic service set identifier (BSSID) of the current access point.
        //     The BSSID may be null if there is no network currently connected.
        //     [Android Documentation]
        public virtual string BSSID { get; }
    }
}
