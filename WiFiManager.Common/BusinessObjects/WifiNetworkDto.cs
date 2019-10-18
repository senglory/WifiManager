using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Xamarin.Forms;
using System.Text;
using System.Text.RegularExpressions;
using AutoMapper;
using System.Globalization;
using System.Reflection;

namespace WiFiManager.Common.BusinessObjects
{
    public class WifiNetworkDto : INotifyPropertyChanged
    {

        public string Name { get; set; }
        public string BssID { get; set; }
        public string NetworkType { get; set; }
        public string Password { get; set; }
        public int Channel { get; set; }
        public string WpsPin { get; set; }
        public DateTime? FirstConnectWhen { get; set; }
        public string FirstConnectMac { get; set; }

        string firstConnectPublicIP;
        public string FirstConnectPublicIP
        {
            get { return firstConnectPublicIP; }
            set {
                    SetProperty(ref firstConnectPublicIP, value, nameof(FirstConnectPublicIP));
            }
        }

        string routerWebUIIP;
        public string RouterWebUIIP
        {
            get { return routerWebUIIP; }
            set
            {
                SetProperty(ref routerWebUIIP, value, nameof(RouterWebUIIP));
            }
        }

        public int Level { get; set; }
        public string Provider { get; set; }

        // for coloring
        bool isInCSVList;
        public bool IsInCSVList
        {
            get
            {
                return isInCSVList;
            }
            set
            {
                SetProperty(ref isInCSVList, value, nameof(IsInCSVList));
                if (isInCSVList)
                    DtoNetworkStateForColoring = DtoNetworkState.IsInCSVList;
                else
                    DtoNetworkStateForColoring = DtoNetworkState.Default;
            }
        }

        bool isSelected;
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                SetProperty(ref isSelected, value, nameof(IsSelected));
            }
        }

        bool isEnabled;
        public bool IsEnabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                SetProperty(ref isEnabled, value, nameof(IsEnabled));
            }
        }

        #region Coords
        double? firstCoordLat;
        public double? FirstCoordLat
        {
            get
            {
                return firstCoordLat;
            }
            set
            {
                SetProperty(ref firstCoordLat, value, "FirstCoordLat");
            }
        }

        double? firstCoordLong;
        public double? FirstCoordLong
        {
            get
            {
                return firstCoordLong;
            }
            set
            {
                SetProperty(ref firstCoordLong, value, "FirstCoordLong");
            }
        }

        double? firstCoordAlt;
        public double? FirstCoordAlt
        {
            get
            {
                return firstCoordAlt;
            }
            set
            {
                SetProperty(ref firstCoordAlt, value, "FirstCoordAlt");
            }
        }

        double? lastCoordLat;
        public double? LastCoordLat
        {
            get
            {
                return lastCoordLat;
            }
            set
            {
                SetProperty(ref lastCoordLat, value, "LastCoordLat");
            }
        }

        double? lastCoordLong;
        public double? LastCoordLong
        {
            get
            {
                return lastCoordLong;
            }
            set
            {
                SetProperty(ref lastCoordLong, value, "LastCoordLong");
            }
        }

        double? lastCoordAlt;
        public double? LastCoordAlt
        {
            get
            {
                return lastCoordAlt;
            }
            set
            {
                SetProperty(ref lastCoordAlt, value, "LastCoordAlt");
            }
        } 
        #endregion



        DtoNetworkState dtoNetworkStateForColoring;
        public DtoNetworkState DtoNetworkStateForColoring
        {
            get { return dtoNetworkStateForColoring; }
            set
            {
                SetProperty(ref dtoNetworkStateForColoring, value, nameof(DtoNetworkStateForColoring));
            }
        }

        ObservableCollection<CoordsAndPower> _CoordsAndPower=new ObservableCollection<CoordsAndPower>();
        public ObservableCollection<CoordsAndPower> CoordsAndPower {
            get
            {
                return _CoordsAndPower;
            }
            set
            {
                SetProperty(ref _CoordsAndPower, value, nameof(CoordsAndPower));
            }
        }

        public bool IsIgnoredNetwork
        {
            get
            {
                return this.Name == "Telekom_FON"
                       || this.Name == "Unitymedia WifiSpot"
                       || this.Name == "Globus_Gast"
                       || this.Name == "MT_FREE"
                       || this.Name == "TSUM Discount"
                       || this.Name == "CPPK_Free"
                       || this.Name == "VTB_WiFi_Free"
                       || this.Name == "RTK_SBRF_WIFI"
                       || this.Name == "TANUKI_WiFi_FREE"
                       || this.Name == "TANUKI_WiFi_FREE_TURBO"
                       || this.Name == "free_wi - fi_raiffeisenbank"
                       || this.Name == "Moscow_WiFi_Free"
                       || this.Name == "Metropolis_FREE"
                       || this.Name == "McDonalds_WiFi_Free"
                       || this.Name == "Mosinter"
                       || this.Name == "Beeline_WiFi_FREE"
                       || this.Name == "Beeline_WiFi_Starbucks_FREE"
                       || this.Name == "Tele2_WiFi_Free"
                       || this.Name == "Starbucks_Beeline_Free"
                       || this.Name == "Moscow_WiFi_Free"
                       || this.Name == "MetropolisNew-WiFi_FREE"
                       || this.Name == "Aeroexpress_iras"
                       || this.Name == "Shokoladniza-Guest"
                       || this.Name.StartsWithNullSafe("RTK_SBRF_WIFI")
                       || this.Name.StartsWithNullSafe("((lovit))")
                       || this.Name.StartsWithNullSafe("YICarCam_");
            }
        }

        readonly string[] _vulnerableBssIds = new string[] {
            "00:0E:8F", // Beeline Smart_box
            "00:1F:CE",
            "00:18:E7", // Rostelecom DIR-655 A4
            "04:BF:6D",
            "04:D4:C4",
            "08:C6:B3",
            "0C:80:63", // TP-Link
            "10:7B:EF", // Keenetic
            "10:BF:48",
            "10:C3:7B",
            "14:A9:E3",
            "14:DD:A9",
            "18:A6:F7",
            "18:D6:C7", // TP-Link Archer C20
            "1C:B7:2C", // ASUS RT-N11P
            "1C:7E:E5",
            "1C:74:0D", // Keenetic
            "28:28:5D", // Keenetic
            "28:C6:8E", // Keenetic
            "2C:56:DC",
            "2C:4D:54",
            "2C:FD:A1",
            "30:5A:3A",
            "30:85:A9", // ASUS
            "30:B5:C2", // TP-Link
            "38:2C:4A", // ASUS RT-N11P
            "38:D5:47", // ASUS
            "40:16:7E", // ASUS RT-AC52U
            "40:4A:03", // Keenetic
            "44:32:C8",
            "44:94:FC",
            "4E:5D:4E",
            "4C:60:DE", // NETGEAR JWNR2000v2
            "5C:F4:AB",
            "50:67:F0",
            "50:46:5D", // ASUS
            "58:23:8C",
            "58:8B:F3",
            "60:31:97", // Keenetic
            "60:A4:4C", // ASUS
            "64:6E:EA",
            "64:70:02", // TP-Link TL-WR1042N
            "6A:28:5D",
            "70:4D:7B",
            "74:DA:38",
            "78:44:76",
            "7C:8B:CA", // TP-Link
            "84:C9:B2", // D-Link
            "88:D7:F6", // ASUS
            "90:94:E4",
            "90:EF:68", // Keenetic
            "92:7B:EF",
            "94:4A:0C",
            "AA:28:5D",
            "AC:22:0B",
            "AC:84:C6",
            "AC:F1:DF",
            "B0:B2:DC", // Keenetic
            "B0:4E:26", // TP-LINK Archer C20
            "b0:be:76", // TP-LINK
            "B4:75:0E",
            "CC:03:FA",
            "C0:25:E9", // TP-LINK Archer C50
            "C4:27:95",
            "c4:6e:1f",
            "C8:6C:87", // Keenetic
            "C8:60:00", // ASUS RT-N12
            "C8:3A:35",
            "CC:5D:4E",
            "CE:5D:4E",
            "D4:BF:7F",
            "D4:6E:0E",
            "D4:21:22", // Beeline Smart_box
            "D8:50:E6",
            "D8:EB:97",
            "D8:FE:E3", // D-Link DIR-620
            "E0:3F:49",
            "E4:F4:C6",
            "E4:BE:ED",
            "E8:37:7A", // Keenetic
            "E8:CD:2D", // MGTS
            "EA:28:5D", // Keenetic
            "EA:37:7A",
            "EC:08:6B", // TP-LINK
            "EC:43:F6", // Keenetic
            "EE:43:F6", // Keenetic
            "F4:F2:6D",
            "F4:EC:38",
            "F8:32:E4", // ASUS
            "F8:C0:91", // Upvel OnLime NetByNet
            "FC:F5:28", // Keenetic
            "FE:F5:28", // Keenetic
        };  

        public bool IsVulnerable
        {
            get
            {
                return !string.IsNullOrWhiteSpace(BssID)
                    &&
                    NetworkType.Contains("WPS")
                    &&
                    _vulnerableBssIds.Any(w => BssID.StartsWithNullSafe(w));
            }
        }

        readonly string[] _bssIdsWithVPN = new string[] {
                "08:60:6E",
                "10:7B:44",
                "10:BF:48",
                "10:C3:7B",
                "14:DD:A9",
                "18:31:BF",
                "1C:B7:2C", // !!!!!!!!!!! ASUS RT-N11P
                "20:CF:30",
                "2C:4D:54",
                "2C:56:DC",
                "2C:FD:A1",
                "30:5A:3A",
                "30:85:A9",
                "34:97:F6",
                "38:2C:4A", // !!!!!!!!!!! ASUS RT-N11P
                "38:D5:47",
                "40:16:7E",
                "50:46:5D",
                "50:E6:5F",
                "54:04:A6",
                "54:A0:50",
                "60:45:CB",
                "60:A4:4C",
                "70:4D:7B",
                "72:4D:7B",
                "74:D0:2B",
                "78:24:AF",
                "88:D7:F6",
                "90:57:84",
                "90:E6:BA",
                "9C:5C:8E",
                "AC:9E:17",
                "AC:22:0B",
                "BC:AE:C5",
                "BC:EE:7B",
                "C8:60:00",
                "CC:5D:4E",
                "D8:50:E6",
                "E0:3F:49",
                "E0:CB:4E",
                "F0:B4:29"
        };

        public bool IsWithVPN
        {
            get
            {
                return _bssIdsWithVPN.Any(w => BssID.StartsWithNullSafe(w));
            }
        }

        readonly string[] _bssIdsWithVPNEasy = new string[] {
                "1C:B7:2C", // !!!!!!!!!!! ASUS RT-N11P
                "38:2C:4A"  // !!!!!!!!!!! ASUS RT-N11P
        };

        public bool IsWithVPNEasy
        {
            get
            {
                return _bssIdsWithVPNEasy.Any(w => BssID.StartsWithNullSafe(w));
            }
        }


        public bool IsOpen
        {
            get
            {
                return NetworkType == "[ESS]" || NetworkType == "[WPS][ESS]";
            }
        }

        public bool IsWEP
        {
            get
            {
                return NetworkType.Contains("[WEP]");
            }
        }

        public bool IsWPS
        {
            get
            {
                return NetworkType.Contains("[WPS]");
            }
        }

        public bool IsWithPassword
        {
            get
            {
                return !string.IsNullOrEmpty(Password);
            }
        }

        public bool IsWithWPSPIN
        {
            get
            {
                return !string.IsNullOrEmpty(WpsPin);
            }
        }

        string _currentlyActiveWifiSSID;

        public bool IsBeingUsed
        {
            get
            {
                return !string.IsNullOrWhiteSpace(_currentlyActiveWifiSSID) && BssID.ToUpper() == _currentlyActiveWifiSSID.ToUpper();
            }
        }

        public WifiNetworkDto(string currentlyActiveWifiSSID = null)
        {
            _currentlyActiveWifiSSID = currentlyActiveWifiSSID;
            CoordsAndPower = new ObservableCollection<CoordsAndPower>();
            ConnectDisconnectCommand = new Command(ExecuteConnectDisconnectCommand);
            RefeshCoordsCommand = new Command(DoRefeshCoordsCommand);
        }

        public Command ConnectDisconnectCommand { get; set; }


        void ExecuteConnectDisconnectCommand(object parameter)
        {

        }
        public Command RefeshCoordsCommand { get; set; }
        void DoRefeshCoordsCommand(object parameter)
        {

        }

        public bool HasCoords{
            get{
                return LastCoordLat.HasValue && LastCoordLong.HasValue && LastCoordAlt.HasValue;
            }
        }

        public void TryUpdateRecentCoords(Tuple<double, double, double> coords2)
        {
            CoordsAndPower.Add(new CoordsAndPower
            {
                Lat = coords2.Item1,
                Long = coords2.Item2,
                Alt = coords2.Item3,
                Level = Level,
                When = DateTime.Now
            });

            LastCoordLat = coords2.Item1;
            LastCoordLong = coords2.Item2;
            LastCoordAlt = coords2.Item3;
        }

        public void TryUpdateFirstConnectionInfo(WifiConnectionInfo info2)
        {
            // write data about first connection only if it has not been written yet 
            if (FirstConnectWhen == null)
            {
                FirstConnectWhen = info2.FirstConnectWhen;
            }
            if (string.IsNullOrEmpty(FirstConnectMac))
            {
                FirstConnectMac = info2.FirstConnectMac;
            }
            if (string.IsNullOrEmpty(FirstConnectPublicIP))
            {

            }

            if (!FirstCoordLat.HasValue)
            {
                FirstCoordLat = info2.FirstCoordLat;
                FirstCoordLong = info2.FirstCoordLong;
                FirstCoordAlt = info2.FirstCoordAlt;
            }
            // always actualize info about Web UI
            RouterWebUIIP = info2.RouterWebUIIP;
        }

        public override string ToString()
        {
            return BssID + " " + Name + " " +  Math.Abs(Level).ToString ();
        }

        static MapperConfiguration _config;
        static IMapper _mapper;
        static readonly CultureInfo _cultUS = new CultureInfo("en-us");
        static readonly CultureInfo _cultRU = new CultureInfo("ru-ru");

        static readonly Regex _rxForBssId1 = new Regex(@"[0-9a-f]{2}:[0-9a-f]{2}:[0-9a-f]{2}:[0-9a-f]{2}:[0-9a-f]{2}:[0-9a-f]{2}", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
        static readonly Regex _rxForBssId2 = new Regex(@"[0-9a-f]{2}-[0-9a-f]{2}-[0-9a-f]{2}-[0-9a-f]{2}-[0-9a-f]{2}-[0-9a-f]{2}", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
        const string SEMICOLON_REPLACEMENT_IN_CSV = @"\x3B";

        static WifiNetworkDto()
        {
            _config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<WifiNetwork, WifiNetworkDto>();
                cfg.CreateMap<WifiNetworkDto, WifiNetwork>();
                cfg.IgnoreUnmapped();
            });
            _config.AssertConfigurationIsValid();
            var mi = typeof(AutoMapper.Mappers.EnumToEnumMapper).GetRuntimeMethods().First(m => m.IsStatic);
            _mapper = _config.CreateMapper();
        }

        public static WifiNetworkDto GetWifiDtoFromString(string s)
        {
            var arrs = s.Split(new char[] { ';' }, StringSplitOptions.None);
            // parse potential BSSID value
            var bssidRaw = arrs[1].ToUpper().Trim();
            var bssid = bssidRaw;
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(bssidRaw))
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
            WifiNetwork nw;
            string isEna = string.IsNullOrWhiteSpace(arrs[3]) ? "0" : arrs[3];

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
                RouterWebUIIP = arrs[9],
                FirstConnectMac = arrs[10],
                Level = -1 * Constants.NO_SIGNAL_LEVEL
            };
            if (!string.IsNullOrEmpty(arrs[11]))
                nw.FirstCoordLat = Convert.ToDouble(arrs[11], _cultUS);
            if (!string.IsNullOrEmpty(arrs[12]))
                nw.FirstCoordLong = Convert.ToDouble(arrs[12], _cultUS);
            if (!string.IsNullOrEmpty(arrs[13]))
                nw.FirstCoordAlt = Convert.ToDouble(arrs[13], _cultUS);

            if (!string.IsNullOrEmpty(arrs[14]))
                nw.LastCoordLat = Convert.ToDouble(arrs[14], _cultUS);
            if (!string.IsNullOrEmpty(arrs[15]))
                nw.LastCoordLong = Convert.ToDouble(arrs[15], _cultUS);
            if (!string.IsNullOrEmpty(arrs[16]))
                nw.LastCoordAlt = Convert.ToDouble(arrs[16], _cultUS);

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

            var wifiDtoFromFile = _mapper.Map<WifiNetwork, WifiNetworkDto>(nw);
            return wifiDtoFromFile;
        }

        public static string NetworkToStringInCSV(WifiNetworkDto wifiOnAir)
        {
            var isBanned = wifiOnAir.IsEnabled ? "" : "1";
            var dummy = "";
            var firstCOnnectWhen = wifiOnAir.FirstConnectWhen.HasValue ? wifiOnAir.FirstConnectWhen.Value.ToString(_cultUS) : "";
            // prevent from further breaking of list load because of ';' in name or pwd or comments
            var nameAdj = wifiOnAir.Name.ReplaceNullSafe(";", SEMICOLON_REPLACEMENT_IN_CSV);
            var passwordAdj = wifiOnAir.Password.ReplaceNullSafe(";", SEMICOLON_REPLACEMENT_IN_CSV);
            var commentsAdj = wifiOnAir.Provider.ReplaceNullSafe(";", SEMICOLON_REPLACEMENT_IN_CSV);
            return $"{nameAdj};{wifiOnAir.BssID};{passwordAdj};{isBanned};{dummy};{commentsAdj};{wifiOnAir.WpsPin};{firstCOnnectWhen};{wifiOnAir.FirstConnectPublicIP};{wifiOnAir.RouterWebUIIP};{wifiOnAir.FirstConnectMac};{wifiOnAir.FirstCoordLat};{wifiOnAir.FirstCoordLong};{wifiOnAir.FirstCoordAlt};{wifiOnAir.LastCoordLat};{wifiOnAir.LastCoordLong};{wifiOnAir.LastCoordAlt}";
        }

        public static string ToStringInCSV(string s)
        {
            return s.ReplaceNullSafe(";", SEMICOLON_REPLACEMENT_IN_CSV);
        }


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName]string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
            //if (Equals(backingStore, value)) return false;
            //backingStore = value;
            //OnPropertyChanged(propertyName);
            //return true;
        }

        public void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public void CopyTo(WifiNetworkDto dtoDst)
        {
            dtoDst.IsEnabled = IsEnabled;
            dtoDst.Password = Password;
            dtoDst.Provider = Provider;
            dtoDst.WpsPin = WpsPin;
            dtoDst.FirstConnectWhen = FirstConnectWhen;
            dtoDst.FirstConnectPublicIP = FirstConnectPublicIP;
            dtoDst.RouterWebUIIP = RouterWebUIIP;
            dtoDst.FirstConnectMac = FirstConnectMac;
            dtoDst.FirstCoordLat = FirstCoordLat;
            dtoDst.FirstCoordLong = FirstCoordLong;
            dtoDst.FirstCoordAlt = FirstCoordAlt;
            dtoDst.LastCoordLat = LastCoordLat;
            dtoDst.LastCoordLong = LastCoordLong;
            dtoDst.LastCoordAlt = LastCoordAlt;
        }
    }
}
