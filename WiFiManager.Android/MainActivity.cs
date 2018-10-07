using System;
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
using Android.Util;
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
using AutoMapper;
using System.Text;

namespace WiFiManager.Droid
{
    [Activity(Label = "WiFiManager", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IWifiOperations
    {
        string filePathCSV = Path.Combine(
"/storage/sdcard0/DCIM"
, "WIFINETWORKS.csv");

        string filePathTemplateJSON = Path.Combine(
"/storage/sdcard0/DCIM"
, "WIFINETWORKS-{0}.JSON");

        public event ConnectionSTateHandler ConnectionStateChanged;

        static  MapperConfiguration config;
        static  IMapper mapper;

        protected override void OnCreate(Bundle bundle)
        {
            config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<WifiNetwork, WifiNetworkDto>();
                cfg.CreateMap<WifiNetworkDto, WifiNetwork>();
                cfg.IgnoreUnmapped();
            });
            config.AssertConfigurationIsValid();
            mapper = config.CreateMapper();

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

        public List<WifiNetworkDto> GetActiveWifiNetworks()
        {
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
                        IsEnabled = true
                    };
                    wifiNetworks.Add(netw);
                }
                catch (Exception ex)
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        //DisplayAlert("Error", ex.Message, "OK");
                    });
                }
            }

            return wifiNetworks;
        }

        public List<WifiNetworkDto> GetWifiNetworksFromCSV( )
        {
            var res = new List<WifiNetworkDto>();

            if (!File.Exists(filePathCSV))
                return res;
            using (var fs = new FileStream(filePathCSV, FileMode.Open))
            {
                using (var fr = new StreamReader(fs))
                {
                    fr.ReadLine();
                    while (!fr.EndOfStream)
                    {
                        var s = fr.ReadLine();
                        var arrs = s.Split(new char[] { ';' });
                        try
                        {
                            // parse potential BSSID value
                            var bssidRaw = arrs[1].ToUpper().Trim();
                            var bssid = bssidRaw;
                            if (!bssidRaw.Contains(':'))
                            {
                                var sb = new StringBuilder();
                                bssid = sb.AppendFormat("{0}:{0}:{0}:{0}:{0}:{0}",
                                    bssidRaw.Substring(0, 2),
                                    bssidRaw.Substring(3, 2),
                                    bssidRaw.Substring(6, 2),
                                    bssidRaw.Substring(9, 2),
                                    bssidRaw.Substring(12, 2),
                                    bssidRaw.Substring(15, 2)).ToString ();
                            }
                            var nw = new WifiNetwork
                            {
                                BssID = bssid,
                                Name = arrs[0].Trim(),
                                Password = arrs[2].Trim(),
                                IsEnabled = !Convert.ToBoolean(int.Parse(arrs[3])),
                                NetworkType = arrs[4],
                                Provider = arrs[5],
                                Level = -1 * Constants.NO_SIGNAL_LEVEL
                            };
                            var wifiDtoFromFile = mapper.Map<WifiNetwork, WifiNetworkDto>(nw);
                            res.Add(wifiDtoFromFile);
                        }
                        catch (Exception ex)
                        {
                            Log.Error("ReadCSV", "", ex.Message);
                        }
                    }
                }
            }

            return res;
        }
        public void SaveToCSV(List<WifiNetworkDto> wifiNetworks)
        {
            using (var fs = new FileStream(filePathCSV, FileMode.Create))
            {
                using (var fw = new StreamWriter(fs))
                {
                    fw.WriteLine("Name;Bssid;Password;IsBanned;NetworkType;Provider");
                    foreach (var nw in wifiNetworks)
                    {
                        var isBanned = nw.IsEnabled ? 0 : 1;
                        fw.WriteLine($"{nw.Name};{nw.BssID};{nw.Password};{isBanned};{nw.NetworkType};{nw.Provider}");
                    }
                }
            }
        }

        public void SaveToJSON(List<WifiNetworkDto> wifiNetworks)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            string str = JsonConvert.SerializeObject(wifiNetworks, settings);
            var filePathJSON = string.Format(filePathTemplateJSON, DateTime.Now.ToString("yyyyMMdd-HH:mm"));
            File.WriteAllText(filePathJSON, str);
        }

        public async Task ConnectAsync(string bssid, string ssid, string password)
        {
            await Task.Run(() => Connect(bssid, ssid,  password));
        }
        public async Task DisConnectAsync()
        {
            await Task.Run(() => DisConnect());
        }

        void Connect(string bssid, string ssid, string password)
        {
            var formattedSsid = $"\"{ssid}\"";
            var formattedPassword = $"\"{password}\"";

            var wifiConfig = new WifiConfiguration
            {
                Bssid = bssid,
                Ssid = formattedSsid,
                PreSharedKey = formattedPassword,
                Priority = 10000
            };

            var wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.WifiService);
            var connManager = (ConnectivityManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.ConnectivityService);
            var ni1 = connManager.ActiveNetworkInfo;

            if (ni1 != null && ni1.IsConnected && ni1.Type == ConnectivityType.Wifi)
            {
            }
            else
            {
                wifiManager.SetWifiEnabled(true);
                var addNetworkIdx = wifiManager.AddNetwork(wifiConfig);
                var bd = wifiManager.Disconnect();
                var enableNetwork = wifiManager.EnableNetwork(addNetworkIdx, true);
                var brc = wifiManager.Reconnect();
            }
        }

        void DisConnect()
        {
            var wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.WifiService);
            var connManager = (ConnectivityManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.ConnectivityService);
            var ni1 = connManager.ActiveNetworkInfo;

            if (ni1 != null && ni1.IsConnected && ni1.Type == ConnectivityType.Wifi)
            {
                var wifiInfo = wifiManager.ConnectionInfo;
                var bdi = wifiManager.Disconnect();
                var brr = wifiManager.RemoveNetwork(wifiInfo.NetworkId);
            }
        }

        public async Task<List<WifiNetworkDto>> GetActiveWifiNetworksAsync()
        {
            var locator = CrossGeolocator.Current;
            locator.DesiredAccuracy = 1;
            var includeHeading = true;

            var position = await locator.GetPositionAsync(TimeSpan.FromSeconds(10), null, includeHeading);
            bool b1 = locator.IsGeolocationAvailable;
            bool b2 = locator.IsGeolocationEnabled;


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
                        Level = n.Level,
                        IsEnabled = true
                    };
                    wifiNetworks.Add(netw);

                    System.Diagnostics.Debug.WriteLine(n.Level);
                }
                catch (Exception ex)
                {
                    Log.Error("GetActiveWifiNetworksAsync", "", ex.Message);
                }
            }

            return wifiNetworks ;
        }

        public bool CanLoadFromFile()
        {
            try
            {
                var t = Utils.CheckPermissions(Plugin.Permissions.Abstractions.Permission.Storage);
                var hasPermission = t.Result;
                return hasPermission;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task ActualizeCoordsWifiNetworkAsync(WifiNetworkDto network) {
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
                    Power = signalLevel,
                    When = DateTime.Now
                });
            }

        }

    }
}

