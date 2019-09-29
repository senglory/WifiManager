using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Reflection;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;
using Android.OS;
using Android.Net;
using Android.Net.Wifi;
using Android.Locations;
using Android.Hardware;
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

using AutoMapper;

using WiFiManager.Common.BusinessObjects;
using WiFiManager.Common;
using Java.Util;
using Android.Support.V4.Content;
using Android.Support.V4.App;



namespace WiFiManager.Droid
{
    [Activity(Label = "WiFiManager", 
        Icon = "@mipmap/icon", 
        Theme = "@style/MainTheme", 
        MainLauncher = true, 
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity,
        Android.Hardware.ISensorEventListener,
        Android.Locations.ILocationListener,
        IWifiManagerOperations
    {
        const int RC_WRITE_EXTERNAL_STORAGE_PERMISSION = 1000;
        const int RC_READ_EXTERNAL_STORAGE_PERMISSION = 1100;
        const int RC_DELETE_STORAGE_FILE = 1200;

        static readonly string TAG = "WiFiManager";
        static readonly string[] PERMISSIONS_TO_REQUEST = { Manifest.Permission.WriteExternalStorage };

        delegate bool FindDelegate(WifiNetworkDto nw, WifiNetworkDto nw2);
        delegate bool FindDelegateBulk( WifiNetworkDto nw2);

        static readonly CultureInfo _cultUS = new CultureInfo("en-us");
        static readonly CultureInfo _cultRU = new CultureInfo("ru-ru");

        string _filePathCSV
        {
            get {
                var sdCardPathDCIM = GetSDCardDir();
                return Path.Combine(sdCardPathDCIM, "WIFINETWORKS.csv");
            }
        }

        string _filePathCSVBAK
        {
            get
            {
                var sdCardPathDCIM = GetSDCardDir();
                return Path.Combine(sdCardPathDCIM, "WIFINETWORKS.bak");
            }
        }

        string _filePathRawDumpCSV
        {
            get
            {
                var sdCardPathDCIM = GetSDCardDir();
                return Path.Combine(sdCardPathDCIM, "WIFINETWORKS-RAW.csv");
            }
        }

        string _filePathCSVinBluetooth
        {
            get
            {
                var sdCardPathBluetooth = "/storage/sdcard0/bluetooth/";
                return Path.Combine(sdCardPathBluetooth, "WIFINETWORKS.csv");
            }
        }


        LocationManager _locationManager;
        LocationManager Manager
        {
            get
            {
                if (_locationManager == null)
                    _locationManager = (LocationManager)Android.App.Application.Context.GetSystemService(Context.LocationService);

                return _locationManager;
            }
        }
        string[] Providers => Manager.GetProviders(enabledOnly: false).ToArray();
        string[] IgnoredProviders => new string[] { LocationManager.PassiveProvider, "local_database" };

        Location _location;



        protected override void OnCreate(Bundle bundle)
        {
            //Log.Info(TAG, "MainActivity - OnCreate - before  CreateMapper");

            //Log.Info(TAG, "MainActivity - OnCreate - after  CreateMapper");

            _CachedCSVNetworkList = new List<WifiNetworkDto>();

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            RequestedOrientation = ScreenOrientation.Portrait;

            base.OnCreate(bundle);

            // shake management set up
            try
            {
                var sensorManager = GetSystemService(SensorService) as Android.Hardware.SensorManager;
                var sensor = sensorManager.GetDefaultSensor(Android.Hardware.SensorType.Accelerometer);
                sensorManager.RegisterListener(this, sensor, Android.Hardware.SensorDelay.Game);
            }
            catch (Exception ex)
            {
                Log.Error(TAG, "MainActivity - OnCreate - failed to work with SensorManager " + ex.Message );
            }

            // GPS listener
            try
            {

                var targetsMOrHigher = ApplicationInfo.TargetSdkVersion >= Android.OS.BuildVersionCodes.M;
                if (targetsMOrHigher)
                {
                    if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) !=
                        Android.Content.PM.Permission.Granted)
                    {
                        ActivityCompat.RequestPermissions(this,
                            new[] { Manifest.Permission.AccessFineLocation, Manifest.Permission.AccessCoarseLocation }, 12);
                    }
                }

                Xamarin.Essentials.Platform.Init(this, bundle);

                var locationCriteria = new Criteria();

                locationCriteria.Accuracy = Accuracy.Medium;
                locationCriteria.PowerRequirement = Power.High;

                // get provider: GPS, Network, etc.
                var locationProvider = Manager.GetBestProvider(locationCriteria, true);

                Manager.RequestLocationUpdates("gps", 1000, 1, this);
            }
            catch (Exception ex)
            {
                Log.Error(TAG, "MainActivity - OnCreate - failed to work with RequestLocationUpdates " + ex.Message);
            }

            Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity = this;
            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, bundle);
            global::Xamarin.Forms.Forms.Init(this, bundle);

            LoadApplication(new App(this));
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            //Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }


        public bool UseInternalStorageForCSV { get; set; }

        public bool UseCachedNetworkLookup { get; set; }

        List<WifiNetworkDto> _CachedCSVNetworkList { get; set; }
        public void ClearCachedCSVNetworkList()
        {
            _CachedCSVNetworkList.Clear();
        }

        public async Task<Tuple<double, double, double>> GetCoordsAsync()
        {
            try
            {
                var ts0 = DateTime.Now;
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
                var gpsStatus = Manager.GetGpsStatus(null);
                var prov = Manager.GetProvider("gps");

                var isSat = prov.RequiresSatellite();

                _location = Manager.GetLastKnownLocation("gps");

                //var position = await locator.GetPositionAsync(
    //                TimeSpan.FromSeconds(Constants.GPS_TIMEOUT),
    //                null,
    //                includeHeading
    //                );

                //var ts1 = DateTime.Now;
                //var elapsed = ts1 - ts0;
                //Android.Util.Log.Info(TAG, $"GetCoordsAsync: got GPS coords, elapsed: {elapsed.TotalSeconds} sec" );

                return new Tuple<double, double, double>(_location.Latitude, _location.Longitude, _location.Altitude);
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error(TAG, "GetCoordsAsync " + ex.Message);
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
            foreach (ScanResult nw in results)
            {
                var netw = new WifiNetworkDto(wifiManager.ConnectionInfo?.BSSID)
                {
                    BssID = nw.Bssid.ToUpper(),
                    Name = nw.Ssid,
                    NetworkType = nw.Capabilities,
                    Level = nw.Level,
                    IsEnabled = true
                };
                if (netw.IsIgnoredNetwork)
                    continue;
                wifiNetworks.Add(netw);
            }

            return wifiNetworks;
        }

        IEnumerable<WifiNetworkDto> FindInternal(FindDelegateBulk findMethod)
        {
            using (var fs = new FileStream(_filePathCSV, FileMode.Open, FileAccess.Read))
            {
                using (var fr = new StreamReader(fs, Constants.UNIVERSAL_ENCODING))
                {
                    // skip header
                    var ss2 = fr.ReadLine();
                    while (!fr.EndOfStream)
                    {
                        var lineFromCSV = fr.ReadLine();
                        if (string.IsNullOrWhiteSpace(lineFromCSV))
                            continue;
                        var wifiDtoFromFile = WifiNetworkDto. GetWifiDtoFromString(lineFromCSV);

                        if (findMethod( wifiDtoFromFile))
                            yield return wifiDtoFromFile;
                    }
                }
            }
        }



        WifiNetworkDto FindInternal(WifiNetworkDto nw, bool byBssIdOnly, FindDelegate findMethod)
        {
            WifiNetworkDto wifiDtoFromFile;

            if (UseCachedNetworkLookup)
            {
                if (_CachedCSVNetworkList.Count == 0)
                {
                    #region Populate CSV cache
                    using (var fs = new FileStream(_filePathCSV, FileMode.Open, FileAccess.Read))
                    {
                        using (var fr = new StreamReader(fs, Constants.UNIVERSAL_ENCODING))
                        {
                            // skip header
                            var ss2 = fr.ReadLine();
                            while (!fr.EndOfStream)
                            {
                                var lineFromCSV = fr.ReadLine();
                                if (string.IsNullOrWhiteSpace(lineFromCSV))
                                    continue;
                                wifiDtoFromFile = WifiNetworkDto. GetWifiDtoFromString(lineFromCSV);
                                _CachedCSVNetworkList.Add(wifiDtoFromFile);
                            }
                        }
                    } 
                    #endregion
                }
                // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                // Refactor lookup code, also do the same for FindWifiInCSV()
                // Use FindDelegate
                var wifiDtoFromFileKVP = _CachedCSVNetworkList.FirstOrDefault(
                    (nwFromCache) => {
                        if (string.IsNullOrEmpty(nwFromCache.BssID) )
                        {
                            return nwFromCache.Name == nw.Name;
                        }
                        if ( string.IsNullOrEmpty (  nw.BssID))
                            return nwFromCache.Name == nw.Name;
                        else
                            return nwFromCache.BssID.ToUpper() == nw.BssID.ToUpper();
                    });

                wifiDtoFromFile = wifiDtoFromFileKVP;
            }
            else
            {
                using (var fs = new FileStream(_filePathCSV, FileMode.Open, FileAccess.Read))
                {
                    using (var fr = new StreamReader(fs, Constants.UNIVERSAL_ENCODING))
                    {
                        // skip header
                        var ss2 = fr.ReadLine();
                        while (!fr.EndOfStream)
                        {
                            var lineFromCSV = fr.ReadLine();
                            if (string.IsNullOrWhiteSpace(lineFromCSV))
                                continue;
                            wifiDtoFromFile = WifiNetworkDto.GetWifiDtoFromString(lineFromCSV);

                            if (findMethod(nw, wifiDtoFromFile))
                                return wifiDtoFromFile;
                        }
                    }
                }
                wifiDtoFromFile = null;
            }

            return wifiDtoFromFile;
        }

        public WifiNetworkDto FindWifiInCSV(WifiNetworkDto nw, bool byBssIdOnly)
        {
            WifiNetworkDto res=null;

            try
            {
                if (!System.IO.File.Exists(_filePathCSV))
                {
                    return null;
                }

                res = FindInternal(nw, byBssIdOnly, (nw1, wifiDtoFromFile)=> {
                    return (byBssIdOnly && nw1.BssID.ToUpper() == wifiDtoFromFile.BssID.ToUpper()) || (nw1.Name == wifiDtoFromFile.Name && !byBssIdOnly && nw1.BssID.ToUpper() == wifiDtoFromFile.BssID.ToUpper());
                });
                if (byBssIdOnly)
                {
                    return res;
                }
                if (res == null)
                {
                    res = FindInternal(nw, byBssIdOnly, (nw1, wifiDtoFromFile) =>
                    {
                        return nw1.BssID.ToUpper() == wifiDtoFromFile.BssID.ToUpper();
                    });
                }
                if (res == null)
                {
                    res = FindInternal(nw, byBssIdOnly, (nw1, wifiDtoFromFile) =>
                    {
                        return nw1.Name == wifiDtoFromFile.Name;
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(TAG, "FindWifiInCSV " + ex.Message);
            }

            return res;
        }

        public IEnumerable<WifiNetworkDto> FindWifiInCSV(string wifiNameOrBssId)
        {
            IEnumerable<WifiNetworkDto> res = new List<WifiNetworkDto>();

            try
            {
                if (!System.IO.File.Exists(_filePathCSV))
                {
                    return null;
                }

                res = FindInternal( (nw1) => {
                    return nw1.Name.StartsWithNullSafe(wifiNameOrBssId);
                });
                if (res.Count() == 0)
                {
                    res = FindInternal(( nw1) =>
                    {
                        return nw1.BssID.StartsWithNullSafe(wifiNameOrBssId);
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error(TAG, "FindWifiInCSV " + ex.Message);
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

        public void MoveCSVFromBluetoothFolder()
        {
            if (System.IO.File.Exists(_filePathCSVinBluetooth))
            {
                // works in 2017, busted in 2019 - "Access denied"
                //System.IO.File.Copy(_filePathCSVinBluetooth, _filePathCSV, true);
                SafeFileCopy(_filePathCSVinBluetooth, _filePathCSV);
                System.IO.File.Delete(_filePathCSVinBluetooth);
            }
        }

        public async Task SaveToCSVAsync(List<WifiNetworkDto> wifiNetworksOnAir)
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

                Android.Util.Log.Info(TAG, "Saving CSV in BAK as " + _filePathCSVBAK);
                var csvAlreadyExists = System.IO. File.Exists(_filePathCSV);
                if (csvAlreadyExists)
                {
                    if (System.IO.File.Exists(_filePathCSVBAK))
                    {
                        //System.IO.File.Delete(_filePathCSVBAK);
                    }
                    // works in 2017, busted in 2019 - "Access denied"
                    //System.IO.File.Copy(_filePathCSV, _filePathCSVBAK, true);
                    SafeFileCopy(_filePathCSV, _filePathCSVBAK);
                }
                var alreadySaved = new List<WifiNetworkDto>();
                if (csvAlreadyExists)
                {
                    fsBAK = new FileStream(_filePathCSVBAK, FileMode.Open);
                }
                var s = "";
                using (var fsw = new FileStream(_filePathCSV, FileMode.Create))
                {
                    using (var fw = new StreamWriter(fsw, Constants.UNIVERSAL_ENCODING))
                    {
                        // write header
                        fw.WriteLine("Name;Bssid;Password;IsBanned;NetworkType;Provider;WpsPin;FirstConnectWhen;FirstConnectPublicIP;RouterWebUIIP;FirstConnectMac;FirstCoordLat;FirstCoordLong;FirstCoordAlt;LastCoordLat;LastCoordLong;LastCoordAlt;");


                        if (csvAlreadyExists)
                        {
                            using (var sr = new StreamReader(fsBAK, Constants.UNIVERSAL_ENCODING))
                            {
                                s = sr.ReadLine();
                                while (!sr.EndOfStream)
                                {
                                    s = sr.ReadLine();
                                    WifiNetworkDto wifiDtoFromFile = WifiNetworkDto. GetWifiDtoFromString(s);

                                    var wifiOnAir = wifiNetworksOnAir.GetExistingWifiDto(wifiDtoFromFile);
                                    if (wifiOnAir == null)
                                    {
                                        fw.WriteLine(s);
                                    }
                                    else
                                    {
                                        var res = WifiNetworkDto.NetworkToStringInCSV(wifiOnAir);
                                        fw.WriteLine(res);

                                        alreadySaved.Add(wifiOnAir);
                                    }
                                }
                            }
                        }

                        foreach (var wifiOnAir in wifiNetworksOnAir)
                        {
                            var wifiAlreadySaved = alreadySaved.GetExistingWifiDto(wifiOnAir);
                            if (wifiAlreadySaved == null)
                            {
                                var res = WifiNetworkDto.NetworkToStringInCSV(wifiOnAir);
                                fw.WriteLine(res);
                            }
                        }
                    }
                }

                System.IO.File.Delete(_filePathCSVBAK);
            }
            catch (Exception ex)
            {
                Log.Error(TAG, "SaveToCSV "+ ex.Message);
                throw ex;
            }
            finally
            {
                fsBAK?.Close();
                fsBAK?.Dispose();
            }
        }

        public async Task DumpRawListAsync(List<WifiNetworkDto> wifiNetworksOnAir)
        {
            try
            {
                var hasPermission = await Utils.CheckPermissions(Plugin.Permissions.Abstractions.Permission.Storage);
                if (!hasPermission)
                    return;

                var fileAlreadyExists = System.IO.File.Exists(_filePathRawDumpCSV);
                using (var fsw = new FileStream(_filePathRawDumpCSV, fileAlreadyExists? FileMode.Append : FileMode.Create))
                {
                    using (var fw = new StreamWriter(fsw, Constants.UNIVERSAL_ENCODING))
                    {
                        if (!fileAlreadyExists)
                        {
                            // write header
                            fw.WriteLine("Name  Bssid   Level   TimeUtc    Lat   Long  Alt");
                        }
                        foreach (var wifiOnAir in wifiNetworksOnAir)
                        {
                            fw.WriteLine($"{WifiNetworkDto.ToStringInCSV (wifiOnAir.Name)}\t{wifiOnAir.BssID}\t{wifiOnAir.Level}\t{DateTime.UtcNow.ToString(_cultUS)}\t{wifiOnAir.LastCoordLat}\t{wifiOnAir.LastCoordLong}\t{wifiOnAir.LastCoordAlt}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(TAG, "DumpRawListAsync " + ex.Message);
                throw ex;
            }
            finally
            {
            }
        }

        private void SafeFileCopy(string filePathCSV, string filePathCSVBAK)
        {
            using (var fsr = new FileStream(filePathCSV, FileMode.Open))
            {
                using (var sr = new StreamReader(fsr, Constants.UNIVERSAL_ENCODING))
                {
                    using (var fsw = new FileStream(filePathCSVBAK, FileMode.Create))
                    {
                        using (var fw = new StreamWriter(fsw, Constants.UNIVERSAL_ENCODING))
                        {
                            while (!sr.EndOfStream)
                            {
                                var s2 = sr.ReadLine();

                                fw.WriteLine(s2);
                            }
                        }
                    }
                }
            }
        }


        //public void SaveToJSON(List<WifiNetworkDto> wifiNetworks)
        //{
        //    JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        //    string str = JsonConvert.SerializeObject(wifiNetworks, settings);
        //    var filePathJSON = string.Format(filePathTemplateJSON, DateTime.Now.ToString("yyyyMMdd-HHmm"));
        //    System.IO.File.WriteAllText(filePathJSON, str);
        //}

        public async Task<WifiConnectionInfo> ConnectAsync(WifiNetworkDto network)
        {
            WifiConnectionInfo info2 = null;

            info2 = TryConnectViaMethod(network);
            if (info2 != null)
            {
                var coords = await GetCoordsAsync();
                if (coords != null)
                {
                    info2.FirstCoordLat = coords.Item1;
                    info2.FirstCoordLong = coords.Item2;
                    info2.FirstCoordAlt = coords.Item3;
                    info2.FirstConnectWhen = DateTime.Now;
                }
                info2.RouterWebUIIP = "http://" + info2.RouterWebUIIP;
            }
            return info2;
        }

        WifiConnectionInfo TryConnectViaMethod(WifiNetworkDto dto)
        {
            string bssid = dto.BssID;
            string ssid = dto.Name;
            string password = dto.Password;
            var formattedSsid = $"\"{ssid}\"";
            var formattedPassword = $"\"{password}\"";
            WifiConnectionInfo info2 = null;


            var wifiConfig = new WifiConfiguration
            {
                Bssid = bssid,
                Ssid = formattedSsid,
                Priority = Constants.WIFI_CONFIG_PRIORITY
            };

            if (dto.NetworkType.Contains("[WEP]"))
            {
                // for WEP
                wifiConfig.WepKeys[0] = formattedPassword;

                wifiConfig.WepTxKeyIndex = 0;
                wifiConfig.AllowedKeyManagement.Set((int)KeyManagementType.None);

                wifiConfig.AllowedProtocols.Set((int)WifiConfiguration.Protocol.Rsn);
                wifiConfig.AllowedGroupCiphers.Set((int)WifiConfiguration.GroupCipher.Wep40);
                wifiConfig.AllowedProtocols.Set((int)WifiConfiguration.Protocol.Wpa);

                wifiConfig.AllowedGroupCiphers.Set((int)WifiConfiguration.GroupCipher.Wep104);
            }
            else
            if (dto.NetworkType.Contains("[WPA"))
            {
                //wifiConfig.AllowKeyManagement.Set((int)KeyManagementType.WpaPsk);
                wifiConfig.PreSharedKey = formattedPassword;
            }
            else
            {
                // open networks
                wifiConfig.AllowedProtocols.Set((int)WifiConfiguration.Protocol.Rsn);
                wifiConfig.AllowedKeyManagement.Set((int)KeyManagementType.None);
            }


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

                DhcpInfo dhcpInfo = wifiManager.DhcpInfo;
                //byte[] ipAddress = BitConverter.GetBytes(dhcpInfo.Gateway);
                int gwip = wifiManager.DhcpInfo.Gateway;
                Task.Delay(500);
                gwip = wifiManager.DhcpInfo.Gateway;

                var gwAddr = (gwip & 0xFF) + "." +
                    ((gwip >> 8) & 0xFF) + "." +
                    ((gwip >> 16) & 0xFF) + "." +
                    ((gwip >> 24) & 0xFF);
                info2 = new WifiConnectionInfo
                {
                    FirstConnectMac = wifiManager.ConnectionInfo.MacAddress,
                    // https://theconfuzedsourcecode.wordpress.com/2015/05/16/how-to-easily-get-device-ip-address-in-xamarin-forms-using-dependencyservice/
                    RouterWebUIIP = gwAddr //DependencyService.Get<IIPAddressManager>().GetIPAddress(),
                };
            }

            var isConnectedToAP = wifiManager.ConnectionInfo.BSSID != "00:00:00:00:00:00" 
                && info2.RouterWebUIIP != "127.0.0.1"
                && info2.RouterWebUIIP != "0.0.0.0";

            return isConnectedToAP ? info2 : null;
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

        public bool CanLoadFromFile
        {
            get
            {
                try
                {
                    var t = Utils.CheckPermissions(Plugin.Permissions.Abstractions.Permission.Storage);
                    var hasPermission = t.Result;
                    return hasPermission;
                }
                catch (Exception ex)
                {
                    Log.Error(TAG, "CanLoadFromFile " + ex.Message);
                    throw;
                }
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

                network.LastCoordLat = coords2.Item1;
                network.LastCoordLong = coords2.Item2;
                network.LastCoordAlt = coords2.Item3;
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
            if (UseInternalStorageForCSV)
            {
                return Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDcim).AbsolutePath;
            }
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
                        Android.Util.Log.Error("!!!", ex.Message);
                        break;
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

        // https://alexdunn.org/2018/05/08/xamarin-tip-add-easter-eggs-on-shake/
        #region Shake properties
        bool hasUpdated = false;
        DateTime lastUpdate;
        float lastX = 0.0f;
        float lastY = 0.0f;
        float lastZ = 0.0f;

        const int ShakeDetectionTimeLapse = 250;
        const double ShakeThreshold = 800;
        #endregion

        #region Android.Hardware.ISensorEventListener implementation

        /// <summary>
        /// Handles when the sensor range changes
        /// </summary>
        /// <param name="sensor">Sensor.</param>
        /// <param name="accuracy">Accuracy.</param>
        public void OnAccuracyChanged(Android.Hardware.Sensor sensor, Android.Hardware.SensorStatus accuracy)
        {
        }


        /// <summary>
        /// Detects sensor changes and is set up to listen for shakes.
        /// </summary>
        public async void OnSensorChanged(Android.Hardware.SensorEvent e)
        {
                //System.Diagnostics.Debug.WriteLine("OnSensorChanged - START");

                //if (handleSensorChanged)
                //    return;
                //handleSensorChanged = true;
                if (e.Sensor.Type == Android.Hardware.SensorType.Accelerometer)
                {
                    var x = e.Values[0];
                    var y = e.Values[1];
                    var z = e.Values[2];

                    // use to check against last time it was called so we don't register every delta
                    var currentTime = DateTime.Now;
                    if (hasUpdated == false)
                    {
                        hasUpdated = true;
                        lastUpdate = currentTime;
                        lastX = x;
                        lastY = y;
                        lastZ = z;
                    }
                    else
                    {
                        if ((currentTime - lastUpdate).TotalMilliseconds > ShakeDetectionTimeLapse)
                        {
                            var diffTime = (float)(currentTime - lastUpdate).TotalMilliseconds;
                            lastUpdate = currentTime;
                            var total = x + y + z - lastX - lastY - lastZ;
                            var speed = Math.Abs(total) / diffTime * 10000;

                            if (speed > ShakeThreshold)
                            {
                                // We have a shake folks!
                                await HandleShaking();
                            }

                            lastX = x;
                            lastY = y;
                            lastZ = z;
                        }
                    }
                }
        }

        /// <summary>
        /// Execute the easter egg async.
        /// </summary>
        protected virtual async Task HandleShaking()
        {
            await Task.Run(() => {
                var wnd = Xamarin.Forms.Application.Current?.MainPage as MainPage;
                wnd?.RefreshAvailableNetworks(true);
            });
        }

        public void OnLocationChanged(Location location)
        {
            _location = location;
        }

        public void OnProviderDisabled(string provider)
        {
            //throw new NotImplementedException();
        }

        public void OnProviderEnabled(string provider)
        {
            //throw new NotImplementedException();
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            //throw new NotImplementedException();
        }
        #endregion

        public void CreateUnixFiles(WifiNetworkDto network)
        {
            var sdCardPathDCIM = GetSDCardDir();
            var pwdFileName = "";

            if (network.NetworkType.Contains("[WEP]"))
            {
/*
network={
	ssid="JODEL"
	key_mgmt=NONE
	wep_key0="1234567..."
	wep_tx_keyidx=0
}
*/
            }
            else
            {
                pwdFileName = network.BssID.ReplaceNullSafe(":", "-") + "--1";
                var fn1 = Path.Combine(sdCardPathDCIM, pwdFileName + ".sh");
                using (var fsw = new FileStream(fn1, FileMode.Create))
                {
                    using (var fw = new StreamWriter(fsw, Constants.UNIVERSAL_ENCODING))
                    {
                        // write header
                        fw.Write("#!/bin/bash" + '\xA');
                        fw.Write($"wpa_passphrase \"{network.Name}\" {network.Password} | sudo tee /mnt/sdb/{pwdFileName}.conf" + '\xA');
                    }
                }
            }

            var connectFileName = network.BssID.ReplaceNullSafe(":", "-") + "--2";
            var fn2 = Path.Combine(sdCardPathDCIM, connectFileName + ".sh");
            using (var fsw = new FileStream(fn2, FileMode.Create))
            {
                using (var fw = new StreamWriter(fsw, Constants.UNIVERSAL_ENCODING))
                {
                    // write header
                    fw.Write("#!/bin/bash" + '\xA');
                    fw.Write($"wpa_supplicant -i wlan0 -c /mnt/sdb/{pwdFileName}.conf" + '\xA');
                }
            }
        }
    }
}

