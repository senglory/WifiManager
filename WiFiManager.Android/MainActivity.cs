using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

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
using Android;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Plugin.Permissions;
using Plugin.Geolocator;
using Plugin.Connectivity;
using Plugin.Geolocator.Abstractions;
using Plugin.Permissions.Abstractions;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Java.IO;

using Newtonsoft.Json;
using AutoMapper;

using WiFiManager.Common.BusinessObjects;
using WiFiManager.Common;



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
        const int RC_WRITE_EXTERNAL_STORAGE_PERMISSION = 1000;
        const int RC_READ_EXTERNAL_STORAGE_PERMISSION = 1100;
        const int RC_DELETE_STORAGE_FILE = 1200;
        static readonly string TAG = "WiFiManager";
        static readonly string[] PERMISSIONS_TO_REQUEST = { Manifest.Permission.WriteExternalStorage };

        delegate bool FindDelegate(WifiNetworkDto nw, WifiNetworkDto nw2);

        string filePathCSV
        {
            get {
                var sdCardPathDCIM = GetSDCardDir();
                return Path.Combine(sdCardPathDCIM, "WIFINETWORKS.csv");
            }
        }

        string filePathCSVBAK
        {
            get
            {
                var sdCardPathDCIM = GetSDCardDir();
                return Path.Combine(sdCardPathDCIM, "WIFINETWORKS.bak");
            }
        }

        string filePathTemplateJSON
        {
            get
            {
                var sdCardPathDCIM = GetSDCardDir();
                return Path.Combine(sdCardPathDCIM, "WIFINETWORKS-{0}.JSON");
            }
        }

        static  MapperConfiguration _config;
        static  IMapper _mapper;
        static  CultureInfo _cultUS;
        static  CultureInfo _cultRU;

        protected override void OnCreate(Bundle bundle)
        {
            _cultUS = new CultureInfo("en-us");
            _cultRU = new CultureInfo("ru-ru");
            _config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<WifiNetwork, WifiNetworkDto>();
                cfg.CreateMap<WifiNetworkDto, WifiNetwork>();
                cfg.IgnoreUnmapped();
            });
            _config.AssertConfigurationIsValid();
            _mapper = _config.CreateMapper();

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            RequestedOrientation = ScreenOrientation.Portrait;

            base.OnCreate(bundle);

            Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity = this;
            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, bundle);
            global::Xamarin.Forms.Forms.Init(this, bundle);

            LoadApplication(new App(this));
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }


        public bool UsePhoneMemory { get; set; }

        public async Task<Tuple<double, double, double>> GetCoordsAsync()
        {
            try
            {
                var hasPermission = await Utils.CheckPermissions(Plugin.Permissions.Abstractions.Permission.Location);
                if (!hasPermission)
                    return null;

                var locator = CrossGeolocator.Current;
                if (!locator.IsGeolocationAvailable || !locator.IsGeolocationEnabled)
                {
                    //not available or enabled
                    throw new ApplicationException("!locator.IsGeolocationAvailable || !locator.IsGeolocationEnabled");
                }
                locator.DesiredAccuracy = Constants.GPS_ACCURACY;
                
                var includeHeading = true;

                var position = await locator.GetPositionAsync(
                    TimeSpan.FromSeconds(Constants.GPS_TIMEOUT),
                    null,
                    includeHeading
                    );

                return new Tuple<double, double, double>(position.Latitude, position.Longitude, position.Altitude);
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error("WiFiManager", "GetCoordsAsync " + ex.Message);
                throw;
            }
        }

        public void DeleteInfoAboutWifiNetworks() {
            var wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Android.Content.Context.WifiService);
            foreach(var nw in wifiManager.ConfiguredNetworks)
            {
                wifiManager.RemoveNetwork(nw.NetworkId);
            }
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
                   || netw.Name == "Globus_Gast"
                   || netw.Name == "mycloud"
                   || netw.Name == "MT_FREE"
                   || netw.Name == "AndroidAP"
                   || netw.Name == "TSUM Discount"
                   || netw.Name == "CPPK_Free"
                   || netw.Name == "Metropolis_FREE"
                   || netw.Name == "Mosinter"
                   || netw.Name == "Beeline_WiFi_FREE"
                   || netw.Name == "Beeline_WiFi_Starbucks_FREE"
                   || netw.Name == "Starbucks_Beeline_Free"
                   || netw.Name == "Moscow_WiFi_Free"
                   || netw.Name == "MetropolisNew-WiFi_FREE"
                   || netw.Name == "Aeroexpress_iras"
                   || netw.Name == "Shokoladniza-Guest";
        }

        WifiNetworkDto FindInternal(WifiNetworkDto nw, FindDelegate findMethod)
        {
            using (var fs = new FileStream(filePathCSV, FileMode.Open, FileAccess.Read))
            {
                using (var fr = new StreamReader(fs, Constants.UNIVERSAL_ENCODING))
                {
                    // skip header
                    var ss2 = fr.ReadLine();
                    while (!fr.EndOfStream)
                    {
                        var lineFromCSV = fr.ReadLine();
                        WifiNetworkDto wifiDtoFromFile = GetWifiDtoFromString(lineFromCSV);
                        if (findMethod(nw, wifiDtoFromFile))
                            return wifiDtoFromFile;
                    }
                }
            }

            return null;
        }

        public WifiNetworkDto FindWifiInCSV(WifiNetworkDto nw)
        {
            WifiNetworkDto res=null;

            try
            {
                if (!System.IO.File.Exists(filePathCSV))
                {
                    return null;
                }

                res = FindInternal(nw, (nw1, wifiDtoFromFile)=> {
                    return nw1.Name == wifiDtoFromFile.Name && nw1.BssID.ToUpper() == wifiDtoFromFile.BssID.ToUpper();
                });
                if (res == null)
                {
                    res = FindInternal(nw, (nw1, wifiDtoFromFile) =>
                    {
                        return nw1.BssID.ToUpper() == wifiDtoFromFile.BssID.ToUpper();
                    });
                }
                if (res == null)
                {
                    res = FindInternal(nw, (nw1, wifiDtoFromFile) =>
                    {
                        return nw1.Name == wifiDtoFromFile.Name;
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error("WiFiManager", "FindWifiInCSV " + ex.Message);
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
                    nw.FirstCoordLat = Convert.ToDouble(arrs[10], _cultUS);
                if (!string.IsNullOrEmpty(arrs[11]))
                    nw.FirstCoordLong = Convert.ToDouble(arrs[11], _cultUS);
                if (!string.IsNullOrEmpty(arrs[12]))
                    nw.FirstCoordAlt = Convert.ToDouble(arrs[12], _cultUS);

                if (!string.IsNullOrEmpty(arrs[13]))
                    nw.LastCoordLat = Convert.ToDouble(arrs[13], _cultUS);
                if (!string.IsNullOrEmpty(arrs[14]))
                    nw.LastCoordLong = Convert.ToDouble(arrs[14], _cultUS);
                if (!string.IsNullOrEmpty(arrs[15]))
                    nw.LastCoordAlt = Convert.ToDouble(arrs[15], _cultUS);

                // get first connection date
                if (!string.IsNullOrEmpty(arrs[7]))
                {
                    try
                    {
                        nw.FirstConnectWhen = DateTime.Parse(arrs[7], _cultUS);
                    }
                    catch (Exception)
                    {
                        nw.FirstConnectWhen = DateTime.Parse(arrs[7], _cultRU);
                    }
                }
            }

            var wifiDtoFromFile = _mapper.Map<WifiNetwork, WifiNetworkDto>(nw);
            return wifiDtoFromFile;
        }

        public async void SaveToCSV(List<WifiNetworkDto> wifiNetworksOnAir)
        {
            FileStream fsBAK = null;

            try
            {
                //await RequestStorageWritePermission();
                //var newStatus = await CrossPermissions.Current.RequestPermissionsAsync(Plugin.Permissions.Abstractions.Permission.Storage);
                //var br = RequestExternalStoragePermissionIfNecessary(RC_WRITE_EXTERNAL_STORAGE_PERMISSION);

                var hasPermission = await Utils.CheckPermissions(Plugin.Permissions.Abstractions.Permission.Storage);
                if (!hasPermission)
                    return;
                //RequestPermissions(PERMISSIONS_TO_REQUEST, requestCode);

                Thread.CurrentThread.CurrentCulture = _cultUS;
                Thread.CurrentThread.CurrentUICulture = _cultUS;
                Android.Util.Log.Info("SaveToCSV", "Saving CSV in BAK as " + filePathCSVBAK);
                var csvAlreadyExists = System.IO. File.Exists(filePathCSV);
                if (csvAlreadyExists)
                {
                    System.IO.File.Copy(filePathCSV, filePathCSVBAK, true);
                }
                var alreadySaved = new List<WifiNetworkDto>();
                if (csvAlreadyExists)
                {
                    fsBAK = new FileStream(filePathCSVBAK, FileMode.Open);
                }
                using (var fsw = new FileStream(filePathCSV, FileMode.Create))
                {
                    using (var fw = new StreamWriter(fsw, Constants.UNIVERSAL_ENCODING))
                    {
                        // write header
                        fw.WriteLine("Name;Bssid;Password;IsBanned;NetworkType;Provider;WpsPin;FirstConnectWhen;FirstConnectPublicIP;FirstConnectMac;FirstCoordLat;FirstCoordLong;FirstCoordAlt;LastCoordLat;LastCoordLong;LastCoordAlt;");

                        if (csvAlreadyExists)
                        {
                            using (var sr = new StreamReader(fsBAK, Constants.UNIVERSAL_ENCODING))
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
                                        var firstCOnnectWhen = wifiOnAir.FirstConnectWhen.HasValue ? wifiOnAir.FirstConnectWhen.Value.ToString(_cultUS) : "";
                                        fw.WriteLine($"{wifiOnAir.Name};{wifiOnAir.BssID};{wifiOnAir.Password};{isBanned};{wifiOnAir.NetworkType};{wifiOnAir.Provider};{wifiOnAir.WpsPin};{firstCOnnectWhen};{wifiOnAir.FirstConnectPublicIP};{wifiOnAir.FirstConnectMac};{wifiOnAir.FirstCoordLat};{wifiOnAir.FirstCoordLong};{wifiOnAir.FirstCoordAlt};{wifiOnAir.LastCoordLat};{wifiOnAir.LastCoordLong};{wifiOnAir.LastCoordAlt}");
                                        alreadySaved.Add(wifiOnAir);
                                    }
                                    else
                                    {
                                        fw.WriteLine(s);
                                    }
                                }
                            }
                        }

                        foreach (var wifiOnAir in wifiNetworksOnAir)
                        {
                            var wifiAlreadySaved = alreadySaved.GetExistingWifiDto(wifiOnAir);
                            if (wifiAlreadySaved == null)
                            {
                                var isBanned = wifiOnAir.IsEnabled ? 0 : 1;
                                fw.WriteLine($"{wifiOnAir.Name};{wifiOnAir.BssID};{wifiOnAir.Password};{isBanned};{wifiOnAir.NetworkType};{wifiOnAir.Provider};{wifiOnAir.WpsPin};{wifiOnAir.FirstConnectWhen};{wifiOnAir.FirstConnectPublicIP};{wifiOnAir.FirstConnectMac};{wifiOnAir.FirstCoordLat};{wifiOnAir.FirstCoordLong};{wifiOnAir.FirstCoordAlt};{wifiOnAir.LastCoordLat};{wifiOnAir.LastCoordLong};{wifiOnAir.LastCoordAlt}");
                            }
                        }
                    }
                }

                System.IO.File.Delete(filePathCSVBAK);
            }
            catch (Exception ex)
            {
                Log.Error("WiFiManager", "SaveToCSV "+ ex.Message);
                //throw ex;
            }
            finally
            {
                fsBAK?.Close();
                fsBAK?.Dispose();
            }
        }

        public void SaveToJSON(List<WifiNetworkDto> wifiNetworks)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            string str = JsonConvert.SerializeObject(wifiNetworks, settings);
            var filePathJSON = string.Format(filePathTemplateJSON, DateTime.Now.ToString("yyyyMMdd-HHmm"));
            System.IO.File.WriteAllText(filePathJSON, str);
        }

        public async Task<WifiConnectionInfo> ConnectAsync(WifiNetworkDto nw)
        {
            string bssid = nw.BssID;
            string ssid = nw.Name;
            string password = nw.Password;

            WifiConnectionInfo info2;

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
                //WifiConnectionInfo info = new WifiConnectionInfo
                //{
                //    FirstConnectMac = wifiManager.ConnectionInfo.MacAddress,

                //};
                return null;
            }
            else
            {
                wifiManager.SetWifiEnabled(true);
                var addNetworkIdx = wifiManager.AddNetwork(wifiConfig);
                var bd = wifiManager.Disconnect();
                var enableNetwork = wifiManager.EnableNetwork(addNetworkIdx, true);
                var brc = wifiManager.Reconnect();
                info2 = new WifiConnectionInfo
                {
                    FirstConnectMac = wifiManager.ConnectionInfo.MacAddress,

                };
            }

            var coords = await GetCoordsAsync();
            if (coords != null)
            {
                info2.FirstCoordLat = coords.Item1;
                info2.FirstCoordLong = coords.Item2;
                info2.FirstCoordAlt = coords.Item3;
                info2.FirstConnectWhen = DateTime.Now;
            }
            return info2;
        }
        public async Task DisConnectAsync()
        {
            await Task.Run(() => DisConnect());
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
                    Level = signalLevel,
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
                Log.WriteLine(LogPriority.Error, TAG, "GetBaseFolderPath caused the follwing exception: {0}", ex );
            }

            return baseFolderPath;
        }

        private string GetSDCardDir()
        {
            if (UsePhoneMemory)
                return Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDcim).AbsolutePath;
            else
            {
                var context = Android.App.Application.Context;
                var dirs = context.GetExternalCacheDirs();
                foreach (Java.IO.File folder in dirs)
                {
                    try
                    {
                        bool IsRemovable = Android.OS.Environment.InvokeIsExternalStorageRemovable(folder);
                        bool IsEmulated = Android.OS.Environment.InvokeIsExternalStorageEmulated(folder);

                        if (IsRemovable && !IsEmulated)
                            return folder.Path;
                    }
                    catch (Exception ex)
                    {
                        Android.Util.Log.Error("!!!",ex.Message );
                        return "/storage/sdcard1/Android/data/WiFiManager.WiFiManager/cache";
                    }
                }
                return Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDcim).AbsolutePath;
            }
        }

        #region PERMISSIONS
        bool RequestExternalStoragePermissionIfNecessary(int requestCode)
        {
            if (Android.OS.Environment.MediaMounted.Equals(Android.OS.Environment.ExternalStorageState))
            {
                if (CheckSelfPermission(Manifest.Permission.WriteExternalStorage) == Android.Content.PM.Permission.Granted)
                {
                    return true;
                }

                if (ShouldShowRequestPermissionRationale(Manifest.Permission.WriteExternalStorage))
                {
                    RequestPermissions(PERMISSIONS_TO_REQUEST, requestCode);
                    //Snackbar.Make(FindViewById(Android.Resource.Id.Content),
                    //              Resource.String.write_external_permissions_rationale,
                    //              Snackbar.LengthIndefinite)
                    //        .SetAction(Resource.String.ok, delegate { RequestPermissions(PERMISSIONS_TO_REQUEST, requestCode); });
                }
                else
                {
                    RequestPermissions(PERMISSIONS_TO_REQUEST, requestCode);
                }

                return false;
            }

            Log.Warn(TAG, "External storage is not mounted; cannot request permission");
            return false;
        }


        private async Task RequestStorageWritePermission()
        {
            var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Storage);

                var results = await CrossPermissions.Current.RequestPermissionsAsync(Plugin.Permissions.Abstractions.Permission.Storage);
                status = results[Plugin.Permissions.Abstractions.Permission.Storage];
        }

        private async Task RequestStorageWritePermission2()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Storage);
                if (status != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Plugin.Permissions.Abstractions.Permission.Storage))
                    {
                        await Xamarin.Forms.Application.Current?.MainPage?.DisplayAlert("Storage Permission", "OK?", "OK");
                    }

                    var results = await CrossPermissions.Current.RequestPermissionsAsync(Plugin.Permissions.Abstractions.Permission.Storage);
                    status = results[Plugin.Permissions.Abstractions.Permission.Storage];
                }

                if (status == PermissionStatus.Granted)
                {
                    await Xamarin.Forms.Application.Current?.MainPage?.DisplayAlert("Storage Permission", "Granted!", "OK");
                }
                else if (status != PermissionStatus.Unknown)
                {
                    await Xamarin.Forms.Application.Current?.MainPage?.DisplayAlert("Storage Permission", "Denied!", "OK");
                }
            }
            catch (Exception ex)
            {


            }
        }
        #endregion
    }
}

