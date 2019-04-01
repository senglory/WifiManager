﻿using System;
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
        const string SEMICOLON_REPLACEMENT_IN_CSV = "\x3B";

        static readonly string TAG = "WiFiManager";
        static readonly string[] PERMISSIONS_TO_REQUEST = { Manifest.Permission.WriteExternalStorage };

        delegate bool FindDelegate(WifiNetworkDto nw, WifiNetworkDto nw2);

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

        string _filePathTemplateJSON
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
            _cultUS = new CultureInfo("en-us");
            _cultRU = new CultureInfo("ru-ru");
            _config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<WifiNetwork, WifiNetworkDto>();
                cfg.CreateMap<WifiNetworkDto, WifiNetwork>();
                cfg.IgnoreUnmapped();
            });
            _config.AssertConfigurationIsValid();

            Log.Info("WiFiManager", "MainActivity - OnCreate - before  CreateMapper");

            var mi = typeof(AutoMapper.Mappers.EnumToEnumMapper).GetRuntimeMethods().First(m => m.IsStatic);
            _mapper = _config.CreateMapper();

            Log.Info("WiFiManager", "MainActivity - OnCreate - after  CreateMapper");

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
                Log.Error("WiFiManager", "MainActivity - OnCreate - failed to work with SensorManager " + ex.Message );
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
				Log.Error("WiFiManager", "MainActivity - OnCreate - failed to work with RequestLocationUpdates " + ex.Message);
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
                //Android.Util.Log.Info("WiFiManager", $"GetCoordsAsync: got GPS coords, elapsed: {elapsed.TotalSeconds} sec" );

                return new Tuple<double, double, double>(_location.Latitude, _location.Longitude, _location.Altitude);
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
            foreach (ScanResult nw in results)
            {
                var netw = new WifiNetworkDto()
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


        WifiNetworkDto FindInternal(WifiNetworkDto nw, FindDelegate findMethod)
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
                                wifiDtoFromFile = GetWifiDtoFromString(lineFromCSV);
                                _CachedCSVNetworkList.Add(wifiDtoFromFile);
                            }
                        }
                    } 
                    #endregion
                }
                // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                // Refactor lookup code, also do the same for FindWifiInCSV()
                //var wifiDtoFromFileKVP = _CachedCSVNetworkList.FirstOrDefault(
                //    (nwtmp) => {
                //    return nwtmp.BssID.ToUpper() == nw.BssID.ToUpper() && nwtmp.Name == nw.Name ;
                //});
                var wifiDtoFromFileKVP = _CachedCSVNetworkList.FirstOrDefault(
                    (nwtmp) => {
                        if ( string.IsNullOrEmpty (  nw.BssID))
                            return nwtmp.Name == nw.Name;
                        else
                            return nwtmp.BssID.ToUpper() == nw.BssID.ToUpper();
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
                            wifiDtoFromFile = GetWifiDtoFromString(lineFromCSV);

                            if (findMethod(nw, wifiDtoFromFile))
                                return wifiDtoFromFile;
                        }
                    }
                }
                wifiDtoFromFile = null;
            }

            return wifiDtoFromFile;
        }

        public WifiNetworkDto FindWifiInCSV(WifiNetworkDto nw)
        {
            WifiNetworkDto res=null;

            try
            {
                if (!System.IO.File.Exists(_filePathCSV))
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


        static readonly Regex _rxForBssId1 = new Regex(@"[0-9a-f]{2}:[0-9a-f]{2}:[0-9a-f]{2}:[0-9a-f]{2}:[0-9a-f]{2}:[0-9a-f]{2}", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
        static readonly Regex _rxForBssId2 = new Regex(@"[0-9a-f]{2}-[0-9a-f]{2}-[0-9a-f]{2}-[0-9a-f]{2}-[0-9a-f]{2}-[0-9a-f]{2}", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static WifiNetworkDto GetWifiDtoFromString(string s)
        {
            var arrs = s.Split(new char[] { ';' }, StringSplitOptions .None );
            // parse potential BSSID value
            var bssidRaw = arrs[1].ToUpper().Trim();
            var bssid = bssidRaw;
            var sb = new StringBuilder();
            if (!string .IsNullOrEmpty (bssidRaw)) 
            {
                if (_rxForBssId1.IsMatch(bssidRaw) || _rxForBssId2.IsMatch(bssidRaw))
                    bssid = sb.AppendFormat("{0}:{1}:{2}:{3}:{4}:{5}",
                        bssidRaw.Substring(0, 2),
                        bssidRaw.Substring(3, 2),
                        bssidRaw.Substring(6, 2),
                        bssidRaw.Substring(9, 2),
                        bssidRaw.Substring(12, 2),
                        bssidRaw.Substring(15, 2)).ToString();
                else
                    bssid = sb.AppendFormat("{0}:{1}:{2}:{3}:{4}:{5}",
                        bssidRaw.Substring(0, 2),
                        bssidRaw.Substring(2, 2),
                        bssidRaw.Substring(4, 2),
                        bssidRaw.Substring(6, 2),
                        bssidRaw.Substring(8, 2),
                        bssidRaw.Substring(10, 2)).ToString();
            }
            WifiNetwork nw = null;
            string isEna = string.IsNullOrWhiteSpace(arrs[3]) ? "0" : arrs[3];

            // legacy
            if (arrs.Length == 6)
            {
                nw = new WifiNetwork
                {
                    BssID = bssid,
                    Name = arrs[0],
                    Password = arrs[2],
                    IsEnabled = !Convert.ToBoolean(int.Parse(isEna)),
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
                    Name = arrs[0],
                    Password = arrs[2],
                    IsEnabled = !Convert.ToBoolean(int.Parse(isEna)),
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

                // prevent from further breaking of list load because of ';' in name or pwd or comments
                var nameAdj = nw.Name.Replace(SEMICOLON_REPLACEMENT_IN_CSV, ";");
                var passwordAdj = nw.Password.Replace(SEMICOLON_REPLACEMENT_IN_CSV, ";");
                var commentsAdj = nw.Provider.Replace(SEMICOLON_REPLACEMENT_IN_CSV, ";");
                nw.Name = nameAdj;
                nw.Password = passwordAdj;
                nw.Provider = commentsAdj;
            }

            var wifiDtoFromFile = _mapper.Map<WifiNetwork, WifiNetworkDto>(nw);
            return wifiDtoFromFile;
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

                Thread.CurrentThread.CurrentCulture = _cultUS;
                Thread.CurrentThread.CurrentUICulture = _cultUS;
                Android.Util.Log.Info("SaveToCSV", "Saving CSV in BAK as " + _filePathCSVBAK);
                var csvAlreadyExists = System.IO. File.Exists(_filePathCSV);
                if (csvAlreadyExists)
                {
                    System.IO.File.Copy(_filePathCSV, _filePathCSVBAK, true);
                }
                var alreadySaved = new List<WifiNetworkDto>();
                if (csvAlreadyExists)
                {
                    fsBAK = new FileStream(_filePathCSVBAK, FileMode.Open);
                }
                using (var fsw = new FileStream(_filePathCSV, FileMode.Create))
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
                                    if (wifiOnAir == null)
                                    {
                                        fw.WriteLine(s);
                                    }
                                    else
                                    {
                                        var isBanned = wifiOnAir.IsEnabled ? "" : "1";
                                        var dummy = "";
                                        var firstCOnnectWhen = wifiOnAir.FirstConnectWhen.HasValue ? wifiOnAir.FirstConnectWhen.Value.ToString(_cultUS) : "";
                                        // prevent from further breaking of list load because of ';' in name or pwd or comments
                                        var nameAdj = wifiOnAir.Name.ReplaceNullSafe(";", SEMICOLON_REPLACEMENT_IN_CSV);
                                        var passwordAdj = wifiOnAir.Password.ReplaceNullSafe(";", SEMICOLON_REPLACEMENT_IN_CSV);
                                        var commentsAdj = wifiOnAir.Provider.ReplaceNullSafe(";", SEMICOLON_REPLACEMENT_IN_CSV);
                                        fw.WriteLine($"{nameAdj};{wifiOnAir.BssID};{passwordAdj};{isBanned};{dummy};{commentsAdj};{wifiOnAir.WpsPin};{firstCOnnectWhen};{wifiOnAir.FirstConnectPublicIP};{wifiOnAir.FirstConnectMac};{wifiOnAir.FirstCoordLat};{wifiOnAir.FirstCoordLong};{wifiOnAir.FirstCoordAlt};{wifiOnAir.LastCoordLat};{wifiOnAir.LastCoordLong};{wifiOnAir.LastCoordAlt}");
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
                                var isBanned = wifiOnAir.IsEnabled ? "" : "1";
                                var dummy = "";
                                // prevent from further breaking of list load because of ';' in name or pwd or comments
                                var nameAdj = wifiOnAir.Name.ReplaceNullSafe(";", SEMICOLON_REPLACEMENT_IN_CSV);
                                var passwordAdj = wifiOnAir.Password.ReplaceNullSafe(";", SEMICOLON_REPLACEMENT_IN_CSV);
                                var commentsAdj = wifiOnAir.Provider.ReplaceNullSafe(";", SEMICOLON_REPLACEMENT_IN_CSV);
                                fw.WriteLine($"{nameAdj};{wifiOnAir.BssID};{passwordAdj};{isBanned};{dummy};{commentsAdj};{wifiOnAir.WpsPin};{wifiOnAir.FirstConnectWhen};{wifiOnAir.FirstConnectPublicIP};{wifiOnAir.FirstConnectMac};{wifiOnAir.FirstCoordLat};{wifiOnAir.FirstCoordLong};{wifiOnAir.FirstCoordAlt};{wifiOnAir.LastCoordLat};{wifiOnAir.LastCoordLong};{wifiOnAir.LastCoordAlt}");
                            }
                        }
                    }
                }

                System.IO.File.Delete(_filePathCSVBAK);
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

        //public void SaveToJSON(List<WifiNetworkDto> wifiNetworks)
        //{
        //    JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        //    string str = JsonConvert.SerializeObject(wifiNetworks, settings);
        //    var filePathJSON = string.Format(filePathTemplateJSON, DateTime.Now.ToString("yyyyMMdd-HHmm"));
        //    System.IO.File.WriteAllText(filePathJSON, str);
        //}

        public async Task<WifiConnectionInfo> ConnectAsync(WifiNetworkDto dto)
        {
            string bssid = dto.BssID;
            string ssid = dto.Name;
            string password = dto.Password;

            WifiConnectionInfo info2;

            var formattedSsid = $"\"{ssid}\"";
            var formattedPassword = $"\"{password}\"";



            var wifiConfig = new WifiConfiguration
            {
                Bssid = bssid,
                Ssid = formattedSsid,
                Priority = Constants.WIFI_CONFIG_PRIORITY
            };

            if (!dto.NetworkType.Contains("[WPA"))
            {
                wifiConfig.AllowedProtocols.Set( (int)WifiConfiguration.Protocol.Rsn);
                wifiConfig.AllowedKeyManagement.Set((int)KeyManagementType.None);
            }
            else
            if (dto.NetworkType.Contains("[WPA"))
            {
                //wifiConfig.AllowKeyManagement.Set((int)KeyManagementType.WpaPsk);
                wifiConfig.PreSharedKey = formattedPassword;
            }
            else
            if (dto.NetworkType.Contains("[WEP]"))
            {
                // for WEP
                wifiConfig.WepKeys[0] = formattedPassword;

                wifiConfig.WepTxKeyIndex = 0;
                wifiConfig.AllowedGroupCiphers.Set((int)WifiConfiguration.GroupCipher.Wep40);
                wifiConfig.AllowedGroupCiphers.Set((int)WifiConfiguration.GroupCipher.Wep104);
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
                info2 = new WifiConnectionInfo
                {
                    FirstConnectMac = wifiManager.ConnectionInfo.MacAddress,
					// https://theconfuzedsourcecode.wordpress.com/2015/05/16/how-to-easily-get-device-ip-address-in-xamarin-forms-using-dependencyservice/
					InternalIP = DependencyService.Get<IIPAddressManager>().GetIPAddress()
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
                    Log.Error("WiFiManager", "CanLoadFromFile " + ex.Message);
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
	}
}

