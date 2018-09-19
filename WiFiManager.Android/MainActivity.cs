﻿using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Net.Wifi;
using Android.Locations;
using Plugin.Permissions;
using Plugin.Geolocator;
using Plugin.Connectivity;
using Plugin.Geolocator.Abstractions;
using Newtonsoft.Json;

using WiFiManager.Common.BusinessObjects;
using WiFiManager.Common;
using Android.Net;
using Xamarin.Forms;



namespace WiFiManager.Droid
{
    [Activity(Label = "WiFiManager", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IWifiOperations
    {
        string filePathCSV = Path.Combine(
"/storage/sdcard0/DCIM"
, "WIFINETWORKS.csv");

        string filePathJSON = Path.Combine(
"/storage/sdcard0/DCIM"
, "WIFINETWORKS.JSON");

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

        public async Task<Tuple<double, double, double>> GetCoordsAsync()
        {
            var locator = CrossGeolocator.Current;
            locator.DesiredAccuracy = 1;

            var includeHeading = true;

            var t = await locator.GetPositionAsync(TimeSpan.FromSeconds(10), null, includeHeading);

            return new Tuple<double, double, double>(t.Latitude, t.Longitude, t.Altitude);
        }

        public MainPageVM GetActiveWifiNetworks()
        {
            //bool b1 = locator.IsGeolocationAvailable;
            //bool b2 = locator.IsGeolocationEnabled;
            
            //coords.Add(new CoordsAndPower
            //{
            //    Lat = position.Latitude,
            //    Long = position.Longitude,
            //    Alt = position.Altitude
            //});




            var wifiNetworks = new List<WifiNetworkDto>();
            var wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.WifiService);

            wifiManager.StartScan();
            //var networks = wifiManager.ConfiguredNetworks;
            //var connectionInfo = wifiManager.ConnectionInfo;
            var results = wifiManager.ScanResults;
            foreach (ScanResult n in results)
            {
                try
                {
                    var netw = new WifiNetworkDto()
                    {
                        BssID = n.Bssid.ToUpper(),
                        Name = n.Ssid,
                        NetworkType = n.Capabilities,
                        Level = n.Level,
                        IsEnabled=true 
                    };
                    wifiNetworks.Add(netw);
                }
                catch (Exception ex)
                {
                    Device.BeginInvokeOnMainThread(() => {
                        //DisplayAlert("Error", ex.Message, "OK");
                    });
                }
            }

            var vm = new MainPageVM(wifiNetworks, filePathCSV, filePathJSON);
            try
            {
                var t = Utils.CheckPermissions(Plugin.Permissions.Abstractions.Permission.Storage);
                var hasPermission = t.Result;
                if (hasPermission)
                    vm.AppendNetworksFromCSV(filePathCSV);
                vm.SortList();
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(() => {
                    //DisplayAlert("Error", ex.Message, "OK");
                });
            }

            return vm;
        }



        public bool Connect(string bssid, string ssid, string password)
        {
            var formattedSsid = $"\"{ssid}\"";
            var formattedPassword = $"\"{password}\"";

            var wifiConfig = new WifiConfiguration
            {
                Bssid = bssid,
                Ssid = formattedSsid,
                PreSharedKey = formattedPassword,
                Priority=10000
            };

            var wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.WifiService);
            var connManager =  (ConnectivityManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.ConnectivityService);
            var ni1=connManager.ActiveNetworkInfo;
            
            if (ni1!=null && ni1.IsConnected && ni1.Type==ConnectivityType.Wifi)
            {
                var wifiInfo = wifiManager.ConnectionInfo;
                var bdi = wifiManager.Disconnect();
                var brr=wifiManager.RemoveNetwork(wifiInfo.NetworkId);

                return true;
            }
            else
            {
                //wifiConfig.AllowedKeyManagement.Set((int)KeyManagementType.WpaPsk);
                //var connectivityManager = (ConnectivityManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.ConnectivityService);
                //var mobileState = connectivityManager.GetNetworkInfo(ConnectivityType.Wifi).GetState();
                //if (mobileState != NetworkInfo.State.Connected)
                //{
                //    var mawifi = (WifiManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.WifiService);
                //    mawifi.SetWifiEnabled(true);
                //}

                wifiManager.SetWifiEnabled(true);
                var addNetworkIdx = wifiManager.AddNetwork(wifiConfig);
                var bd = wifiManager.Disconnect();
                var enableNetwork = wifiManager.EnableNetwork(addNetworkIdx, true);
                var brc = wifiManager.Reconnect();

                //foreach (var n in wifiManager.ConfiguredNetworks)
                //{
                //    System.Diagnostics.Debug.WriteLine(n.Bssid);
                //}
                //var network = wifiManager.ConfiguredNetworks
                //     .FirstOrDefault(n => n.NetworkId == addNetwork);
                //var bd = wifiManager.Disconnect();
                //var enableNetwork = wifiManager.EnableNetwork(network.NetworkId, true);

                var wifiInfo = wifiManager.ConnectionInfo;
                //var current = Xamarin.esse;
                //var bd2 = wifiManager.Reconnect();
                //wifiManager.UpdateNetwork(wifiConfig);
                var finalState = connManager.GetNetworkInfo(ConnectivityType.Wifi).GetState();
                return finalState == NetworkInfo.State.Connected;
            }
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

            var wifiNetworks = new List<WifiNetworkDto>();
            var wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.WifiService);
            wifiManager.StartScan();
            var networks = wifiManager.ConfiguredNetworks;
            var connectionInfo = wifiManager.ConnectionInfo;
            var results = wifiManager.ScanResults;
            foreach (var n in results)
            {
                try
                {
                    var netw = new WifiNetworkDto()
                    {
                        BssID = n.Bssid.ToUpper(),
                        Name = n.Ssid,
                        NetworkType = n.Capabilities,
                        IsEnabled = true
                    };
                    wifiNetworks.Add(netw);
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
                    Device.BeginInvokeOnMainThread(() => {
                        //DisplayAlert("Error", ex.Message, "OK");
                    });
                }
            }
            var vm = new MainPageVM(wifiNetworks, filePathCSV, filePathJSON);
            try
            {
                var t = Utils.CheckPermissions(Plugin.Permissions.Abstractions.Permission.Storage);
                var hasPermission = t.Result;
                if (hasPermission)
                {
                    vm.AppendNetworksFromCSV(filePathCSV);
                }
                vm.SortList();
            }
            catch (Exception ex)
            {

                throw;
            }
            return vm;
        }

        public async Task GetActualCoordsAsync(WifiNetworkDto network) {
            var wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.WifiService);
            wifiManager.StartScan();
            var networks = wifiManager.ConfiguredNetworks;
            var connectionInfo = wifiManager.ConnectionInfo;
            var results = wifiManager.ScanResults;

            int signalLevel = 0;
            bool networkWasFound = false;
            for (int i = 0; i < wifiManager.ScanResults.Count; i++)
            {
                var n = wifiManager.ScanResults[i];
                if (n.Ssid == network.Name)
                {
                    networkWasFound = true;
                    signalLevel = n.Level;
                    break;
                }
            }

            if (networkWasFound)
            {
                var coords2 = await GetCoordsAsync();

                network.CoordsAndPower.Add(new CoordsAndPower
                {
                    Lat = coords2.Item1,
                    Long = coords2.Item2,
                    Alt = coords2.Item3,
                    Power = signalLevel
                });
            }

        }
    }
}

