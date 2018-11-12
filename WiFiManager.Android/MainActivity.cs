﻿using System;
using System.IO;
using System.Text;
using System.Globalization;
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
using Android.Net;
using Android.Net.Wifi;
using Android.Locations;
using Plugin.Permissions;
using Plugin.Geolocator;
using Plugin.Connectivity;
using Plugin.Geolocator.Abstractions;
using Plugin.Permissions.Abstractions;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using WiFiManager.Common.BusinessObjects;
using WiFiManager.Common;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Newtonsoft.Json;
using AutoMapper;



namespace WiFiManager.Droid
{
    [Activity(Label = "WiFiManager", 
        Icon = "@mipmap/icon", 
        Theme = "@style/MainTheme", 
        MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, 
        IWifiOperations
    {
        string filePathCSV;

        string filePathCSVBAK;

        string filePathTemplateJSON;

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

            // get link to SDCard DCIM
            var sdCardPathDCIM = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDcim).AbsolutePath;
            filePathCSV = Path.Combine(sdCardPathDCIM, "WIFINETWORKS.csv");
            filePathCSVBAK = Path.Combine(sdCardPathDCIM, "WIFINETWORKS.bak");
            filePathTemplateJSON = Path.Combine(sdCardPathDCIM, "WIFINETWORKS-{0}.JSON");

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            RequestedOrientation = ScreenOrientation.Portrait;

            base.OnCreate(bundle);

            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, bundle);
            global::Xamarin.Forms.Forms.Init(this, bundle);

            LoadApplication(new App(this));
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public async Task<Tuple<double, double, double>> GetCoordsAsync()
        {
            var hasPermission = await Utils.CheckPermissions(Plugin.Permissions.Abstractions.Permission.Location);
            if (!hasPermission)
                return null;

            var locator = CrossGeolocator.Current;
            locator.DesiredAccuracy = Constants.GPS_ACCURACY;

            var includeHeading = true;

            var position = await locator.GetPositionAsync(TimeSpan.FromSeconds(Constants.GPS_TIMEOUT), null, includeHeading);

            return new Tuple<double, double, double>(position.Latitude, position.Longitude, position.Altitude);
        }

        public List<WifiNetworkDto> GetActiveWifiNetworks()
        {
            var wifiNetworks = new List<WifiNetworkDto>();
            var wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.WifiService);
            //wifiManager.SetWifiEnabled(false);
            //wifiManager.SetWifiEnabled(true);
            wifiManager.StartScan();
            //var networks = wifiManager.ConfiguredNetworks;
            //var connectionInfo = wifiManager.ConnectionInfo;
            var results = wifiManager.ScanResults;
            foreach (ScanResult n in results)
            {
                var netw = new WifiNetworkDto()
                {
                    BssID = n.Bssid.ToUpper(),
                    Name = n.Ssid,
                    NetworkType = n.Capabilities,
                    Level = n.Level,
                    IsEnabled = true
                };
                if (IsIgnoredNetwork(netw))
                    continue;
                wifiNetworks.Add(netw);
            }

            return wifiNetworks;
        }

        bool IsIgnoredNetwork(WifiNetworkDto netw)
        {
            return netw.Name == "Telekom_FON"
                   || netw.Name == "Unitymedia WifiSpot"
                   || netw.Name == "MT_FREE"
                   || netw.Name == "AndroidAP"
                   || netw.Name == "CPPK_Free"
                   || netw.Name == "Metropolis_FREE"
                   || netw.Name == "Mosinter"
                   || netw.Name == "Beeline_WiFi_FREE"
                   || netw.Name == "Starbucks_Beeline_Free"
                   || netw.Name == "Moscow_WiFi_Free"
                   || netw.Name == "Shokoladniza-Guest";
        }

        public List<WifiNetworkDto> GetWifiNetworksFromCSV( out string firstFailedLine)
        {
            firstFailedLine = null;
            var res = new List<WifiNetworkDto>();
            var lineFromCSV = "";
            HashSet<string> allBSSIDs = new HashSet<string>();

            try
            {
                if (!File.Exists(filePathCSV))
                {
                    //var t1 = Task.Run(() => { return CrossFilePicker.Current.PickFile(); });
                    //FileData filedata = t1.Result;
                    return res;
                }
                using (var fs = new FileStream(filePathCSV, FileMode.Open, FileAccess.Read))
                {
                    using (var fr = new StreamReader(fs, Constants.UNIVERSAL_ENCODING))
                    {
                        // skip header
                        var ss2=fr.ReadLine();
                        while (!fr.EndOfStream)
                        {
                            lineFromCSV = fr.ReadLine();
                            WifiNetworkDto wifiDtoFromFile = GetWifiDtoFromString(lineFromCSV);
                            if (IsIgnoredNetwork(wifiDtoFromFile))
                                continue;
                            if (!string.IsNullOrEmpty(wifiDtoFromFile.BssID) && allBSSIDs.Contains(wifiDtoFromFile.BssID)) {
                                break;
                            }
                            res.Add(wifiDtoFromFile);
                            if (!string.IsNullOrEmpty(wifiDtoFromFile.BssID))
                            {
                                allBSSIDs.Add(wifiDtoFromFile.BssID);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("WiFiManager", "ReadCSV" + ex.Message);
                firstFailedLine= lineFromCSV;
            }

            return res;
        }

        public bool IsConnected()
        {
            var wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.WifiService);
            var connManager = (ConnectivityManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.ConnectivityService);
            var ni1 = connManager.ActiveNetworkInfo;

            return (ni1 != null && ni1.IsConnected && ni1.Type == ConnectivityType.Wifi);
        }

        private static WifiNetworkDto GetWifiDtoFromString(string s)
        {
            var arrs = s.Split(new char[] { ';' }, StringSplitOptions .None );
            // parse potential BSSID value
            var bssidRaw = arrs[1].ToUpper().Trim();
            var bssid = bssidRaw;
            if (!string .IsNullOrEmpty (bssidRaw) && !bssidRaw.Contains(':'))
            {
                var sb = new StringBuilder();
                bssid = sb.AppendFormat("{0}:{1}:{2}:{3}:{4}:{5}",
                    bssidRaw.Substring(0, 2),
                    bssidRaw.Substring(2, 2),
                    bssidRaw.Substring(4, 2),
                    bssidRaw.Substring(6, 2),
                    bssidRaw.Substring(8, 2),
                    bssidRaw.Substring(10, 2)).ToString();
            }
            WifiNetwork nw = null;
            // legacy
            if (arrs.Length == 6)
            {
                nw = new WifiNetwork
                {
                    BssID = bssid,
                    Name = arrs[0].Trim(),
                    Password = arrs[2].Trim(),
                    IsEnabled = !Convert.ToBoolean(int.Parse(arrs[3])),
                    NetworkType = arrs[4],
                    Provider = arrs[5],
                    Level = -1 * Constants.NO_SIGNAL_LEVEL
                };
            }
            // + extra info about connection
            if (arrs.Length >= 10)
            {
                nw = new WifiNetwork
                {
                    BssID = bssid,
                    Name = arrs[0].Trim(),
                    Password = arrs[2].Trim(),
                    IsEnabled = !Convert.ToBoolean(int.Parse(arrs[3])),
                    NetworkType = arrs[4],
                    Provider = arrs[5],
                    WpsPin = arrs[6],
                    FirstConnectPublicIP = arrs[8],
                    FirstConnectMac = arrs[9],
                    Level = -1 * Constants.NO_SIGNAL_LEVEL
                };
                if (!string.IsNullOrEmpty (arrs[10]))
                    nw.FirstCoordLat = Convert.ToDouble(arrs[10]);
                if (!string.IsNullOrEmpty(arrs[11]))
                    nw.FirstCoordLong = Convert.ToDouble(arrs[11]);

                // get first connection date
                if (!string.IsNullOrEmpty(arrs[7]))
                {
                    try
                    {
                        var cultUS = new CultureInfo("en-us");
                        nw.FirstConnectWhen = DateTime.Parse(arrs[7], cultUS);
                    }
                    catch (Exception)
                    {
                        var cultRU = new CultureInfo("ru-ru");
                        nw.FirstConnectWhen = DateTime.Parse(arrs[7], cultRU);
                    }
                }
            }

            var wifiDtoFromFile = mapper.Map<WifiNetwork, WifiNetworkDto>(nw);
            return wifiDtoFromFile;
        }

        public void SaveToCSV(List<WifiNetworkDto> wifiNetworksOnAir)
        {
            try
            {
                var cult = new CultureInfo("en-us");
                File.Copy(filePathCSV, filePathCSVBAK, true);
                var alreadySaved = new List<WifiNetworkDto>();

                using (var fs = new FileStream(filePathCSVBAK, FileMode.Open))
                {
                    using (var fsw = new FileStream(filePathCSV, FileMode.Create))
                    {
                        using (var fw = new StreamWriter(fsw, Constants.UNIVERSAL_ENCODING))
                        {
                            // write header
                            fw.WriteLine("Name;Bssid;Password;IsBanned;NetworkType;Provider;WpsPin;FirstConnectWhen;FirstConnectPublicIP;FirstConnectMac;FirstCoordLat;FirstCoordLong;");

                            using (var sr = new StreamReader(fs, Constants.UNIVERSAL_ENCODING))
                            {
                                var s = sr.ReadLine();
                                while (!sr.EndOfStream)
                                {
                                    s = sr.ReadLine();
                                    WifiNetworkDto wifiDtoFromFile = GetWifiDtoFromString(s);

                                    var wifiOnAir = wifiNetworksOnAir.GetExistingWifiDto(wifiDtoFromFile);
                                    if (wifiOnAir != null)
                                    {
                                        var isBanned = wifiOnAir.IsEnabled ? 0 : 1;
                                        var firstCOnnectWhen = wifiOnAir.FirstConnectWhen.HasValue? wifiOnAir.FirstConnectWhen.Value.ToString(cult) :"";
                                        fw.WriteLine($"{wifiOnAir.Name};{wifiOnAir.BssID};{wifiOnAir.Password};{isBanned};{wifiOnAir.NetworkType};{wifiOnAir.Provider};{wifiOnAir.WpsPin};{firstCOnnectWhen};{wifiOnAir.FirstConnectPublicIP};{wifiOnAir.FirstConnectMac};{wifiOnAir.FirstCoordLat};{wifiOnAir.FirstCoordLong}");
                                        alreadySaved.Add(wifiOnAir);
                                    }
                                    else
                                    {
                                        fw.WriteLine(s);
                                    }
                                }
                            }

                            foreach (var wifiOnAir in wifiNetworksOnAir)
                            {
                                var wifiAlreadySaved = alreadySaved.GetExistingWifiDto(wifiOnAir);
                                if (wifiAlreadySaved == null)
                                {
                                    var isBanned = wifiOnAir.IsEnabled ? 0 : 1;
                                    fw.WriteLine($"{wifiOnAir.Name};{wifiOnAir.BssID};{wifiOnAir.Password};{isBanned};{wifiOnAir.NetworkType};{wifiOnAir.Provider};{wifiOnAir.WpsPin};{wifiOnAir.FirstConnectWhen};{wifiOnAir.FirstConnectPublicIP};{wifiOnAir.FirstConnectMac};{wifiOnAir.FirstCoordLat};{wifiOnAir.FirstCoordLong}");
                                }
                            }
                        }
                    }
                }

                File.Delete(filePathCSVBAK);
            }
            catch (Exception ex)
            {
                Log.Error("WiFiManager", "SaveToCSV"+ ex.Message);
            }
        }

        public void SaveToJSON(List<WifiNetworkDto> wifiNetworks)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            string str = JsonConvert.SerializeObject(wifiNetworks, settings);
            var filePathJSON = string.Format(filePathTemplateJSON, DateTime.Now.ToString("yyyyMMdd-HHmm"));
            File.WriteAllText(filePathJSON, str);
        }

        public async Task<WifiInfoInternal> ConnectAsync(string bssid, string ssid, string password)
        {
            return await Task.Run(() => Connect(bssid, ssid,  password));
        }
        public async Task DisConnectAsync()
        {
            await Task.Run(() => DisConnect());
        }

        WifiInfoInternal Connect(string bssid, string ssid, string password)
        {
            var formattedSsid = $"\"{ssid}\"";
            var formattedPassword = $"\"{password}\"";

            var wifiConfig = new WifiConfiguration
            {
                Bssid = bssid,
                Ssid = formattedSsid,
                PreSharedKey = formattedPassword,
                Priority = Constants.WIFI_CONFIG_PRIORITY
            };

            var wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.WifiService);
            var connManager = (ConnectivityManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.ConnectivityService);
            var ni1 = connManager.ActiveNetworkInfo;

            if (ni1 != null && ni1.IsConnected && ni1.Type == ConnectivityType.Wifi)
            {
                WifiInfoInternal info = new WifiInfoInternal
                {
                    MacAddress = wifiManager.ConnectionInfo.MacAddress,

                };
                return info;
            }
            else
            {
                wifiManager.SetWifiEnabled(true);
                var addNetworkIdx = wifiManager.AddNetwork(wifiConfig);
                var bd = wifiManager.Disconnect();
                var enableNetwork = wifiManager.EnableNetwork(addNetworkIdx, true);
                var brc = wifiManager.Reconnect();
                WifiInfoInternal info2 = new WifiInfoInternal {
                    MacAddress = wifiManager.ConnectionInfo.MacAddress ,

                } ;
                return info2;
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
                    Log.Error("WiFiManager", "GetActiveWifiNetworksAsync "+ ex.Message);
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
                Log.Error("WiFiManager", "CanLoadFromFile "+ ex.Message);
                throw;
            }
        }

        public async Task ActualizeCoordsWifiNetworkAsync(WifiNetworkDto network)
        {
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

                if (!network.FirstCoordLat.HasValue)
                {
                    network.FirstCoordLat = coords2.Item1;
                }
                if (!network.FirstCoordLong.HasValue)
                {
                    network.FirstCoordLong = coords2.Item2;
                }
            }
        }

        /// <summary>
        /// Taken from https://forums.xamarin.com/discussion/103478/how-to-get-path-to-external-sd-card
        /// Returns path in 
        /// </summary>
        /// <param name="getSDPath"></param>
        /// <returns></returns>
        public static string GetBaseFolderPath(bool getSDPath = false)
        {
            string baseFolderPath = "";

            try
            {
                var context = Android.App.Application.Context;
                Java.IO.File[] dirs = context.GetExternalFilesDirs(null);

                foreach (Java.IO.File folder in dirs)
                {
                    bool IsRemovable = Android.OS.Environment.InvokeIsExternalStorageRemovable(folder);
                    bool IsEmulated = Android.OS.Environment.InvokeIsExternalStorageEmulated(folder);

                    if (getSDPath ? IsRemovable && !IsEmulated : !IsRemovable && IsEmulated)
                        baseFolderPath = folder.Path;
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("GetBaseFolderPath caused the follwing exception: {0}", ex);
            }

            return baseFolderPath;
        }
    }
}

