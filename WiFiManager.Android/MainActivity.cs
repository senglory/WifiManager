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
using Plugin.Permissions;


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

            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, bundle);
            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App(this));
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
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
                    var netw = new WifiNetwork()
                    {
                        BssID = n.Bssid,
                        Name = n.Ssid,
                        NetworkType = n.Capabilities,

                    };
                    vm.WifiNetworks.Add(netw);
                    //netw.CoordsAndPower.Add(new CoordsAndPower
                    //{
                    //    Lat = 11.3,
                    //    Long = 54.7888,
                    //    Alt = 122
                    //});
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

