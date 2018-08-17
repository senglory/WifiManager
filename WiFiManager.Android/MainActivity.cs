using System;
using System.IO;

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
using Plugin.Geolocator;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Plugin.Geolocator.Abstractions;

namespace WiFiManager.Droid
{
    [Activity(Label = "WiFiManager", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IWifiOperations
    {
        Position position;
        
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, bundle);
            global::Xamarin.Forms.Forms.Init(this, bundle);

            var locator = CrossGeolocator.Current;
            locator.DesiredAccuracy = 1;
            var includeHeading = true;

            //var t1 = locator.GetPositionAsync(TimeSpan.FromSeconds(10), null, includeHeading);
            //position = t1.GetAwaiter ().GetResult();

            LoadApplication(new App(this));
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public MainPageVM GetActiveWifiNetworks()
        {
            //bool b1 = locator.IsGeolocationAvailable;
            //bool b2 = locator.IsGeolocationEnabled;
            if (position == null)
            {
                //return;
            }
            //coords.Add(new CoordsAndPower
            //{
            //    Lat = position.Latitude,
            //    Long = position.Longitude,
            //    Alt = position.Altitude
            //});

            var _filePath = Path.Combine(
    //Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
    "/storage/sdcard0/DCIM"
    , "WIFINETWORKS.txt");


            var vm = new MainPageVM();
            vm.WifiNetworks = DoLoad(_filePath);
            return vm;
            var wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.WifiService);
            wifiManager.StartScan();
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
                    //    Lat = position.Latitude,
                    //    Long = position.Longitude,
                    //    Alt = position.Altitude,
                    //    Power = n.Level 
                    //});
                }
                catch (Exception ex)
                {
                    int y = 0;
                }
            }

            return vm;
        }

        ObservableCollection<WifiNetwork> DoLoad(string filePath)
        {
            var t = Utils.CheckPermissions(Plugin.Permissions.Abstractions.Permission.Storage);
            var hasPermission = t.Result;
            if (!hasPermission)
                return null;


            if (File.Exists(filePath))
            {
                JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
                var s = File.ReadAllText(filePath);
                var ocwn = JsonConvert.DeserializeObject<ObservableCollection<WifiNetwork>>(s, settings);
                return ocwn;
            }
            return null;
        }

        public async Task<MainPageVM> GetActiveWifiNetworksAsync()
        {
            var locator = CrossGeolocator.Current;
            locator.DesiredAccuracy = 1;
            var includeHeading = true;

            var position = await locator.GetPositionAsync(TimeSpan.FromSeconds(10), null, includeHeading);
            bool b1 = locator.IsGeolocationAvailable;
            bool b2 = locator.IsGeolocationEnabled;
            if (position == null)
            {
                //return;
            }
            //coords.Add(new CoordsAndPower
            //{
            //    Lat = position.Latitude,
            //    Long = position.Longitude,
            //    Alt = position.Altitude
            //});

            var vm = new MainPageVM();
            var wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.WifiService);
            wifiManager.StartScan();
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
                    System.Diagnostics.Debug.WriteLine(n.Level);
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

