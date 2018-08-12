using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Net.Wifi;
using Android.Locations;
using WiFiManager.Common.BusinessObjects;
using WiFiManager.Common;
using WiFiManager;


namespace WiFiManager.Droid
{
    [Activity(Label = "WiFiManager", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IWifiOperations
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App(this));
        }
        public MainPageVM GetActiveWifiNetworks()
        {
            var vm = new MainPageVM();
            var wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.WifiService);
            var networks = wifiManager.ConfiguredNetworks;
            var connectionInfo = wifiManager.ConnectionInfo;
            var results = wifiManager.ScanResults;
            foreach (var n in results)
            {
                try
                {
                    vm.WifiNetworks.Add(new WifiNetwork()
                    {
                        BssID = n.Bssid,
                        Name = n.Ssid,
                        NetworkType = n.Capabilities,

                    }
                            );
                    //Debug.WriteLine(n.Level);
                }
                catch (Exception ex)
                {
                    int y = 0;
                }
            }

            return vm;
        }

    }
}

