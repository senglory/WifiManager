using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Xamarin.Forms;



namespace WiFiManager.Common.BusinessObjects
{
    public class WifiNetworkDto : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string BssID { get; set; }
        public string NetworkType { get; set; }
        public string Password { get; set; }
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

        string internalIP;
        public string InternalIP
        {
            get { return internalIP; }
            set
            {
                SetProperty(ref internalIP, value, nameof(InternalIP));
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
                       || this.Name == "mycloud"
                       || this.Name == "MT_FREE"
                       || this.Name == "AndroidAP"
                       || this.Name == "TSUM Discount"
                       || this.Name == "CPPK_Free"
                       || this.Name == "Metropolis_FREE"
                       || this.Name == "Mosinter"
                       || this.Name == "Beeline_WiFi_FREE"
                       || this.Name == "Beeline_WiFi_Starbucks_FREE"
                       || this.Name == "Starbucks_Beeline_Free"
                       || this.Name == "Moscow_WiFi_Free"
                       || this.Name == "MetropolisNew-WiFi_FREE"
                       || this.Name == "Aeroexpress_iras"
                       || this.Name == "Shokoladniza-Guest";
            }
        }

		string[] _vulnerableBssIds = new string[] {
			"00:0E:8F",
			"00:1F:CE",
			"00:18:E7",
			"08:C6:B3",
			"04:BF:6D",
			"84:C9:B2",
			"F8:C0:91",
			"EE:43:F6",
			"EC:43:F6",
			"EA:28:5D",
			"EA:37:7A",
			"E8:CD:2D",
			"E4:F4:C6",
			"E4:BE:ED",
			"E0:3F:49",
			"D8:50:E6",
			"D8:EB:97",
			"D4:BF:7F",
			"D4:6E:0E",
			"D4:21:22",
			"CC:5D:4E",
			"C8:6C:87",
			"C8:60:00",
			"C8:3A:35",
			"C0:25:E9",
			"B4:75:0E",
			"B0:B2:DC",
			"B0:4E:26",
			"90:EF:68",
			"90:94:E4",
			"92:7B:EF",
			"94:4A:0C",
			"70:4D:7B",
			"78:44:76",
			"74:DA:38",
			"60:31:97",
			"60:A4:4C", // ASUS
			"64:6E:EA",
			"6A:28:5D",
			"5C:F4:AB",
			"50:67:F0",
			"50:67:F0",
			"50:46:5D", // ASUS
			"4E:5D:4E",
			"4C:60:DE",
			"40:4A:03",
			"40:16:7E",
					
			"30:85:A9", // ASUS
                    
			"38:2C:4A", // ASUS
                    
			"38:D5:47", // ASUS
                    
			"F8:32:E4", // ASUS
			"FE:F5:28", // Keenetic
			"28:28:5D", // Keenetic
			"28:C6:8E", // Keenetic
			"2C:56:DC",
			"1C:B7:2C",
			"1C:7E:E5",
			"18:D6:C7",
			"14:A9:E3",
			"10:C3:7B",
			"10:7B:EF", // Keenetic
			"1C:74:0D", // Keenetic
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

		string[] _bssIdsWithVPN = new string[] {
				"08:60:6E",
				"10:7B:44",
				"10:BF:48",
				"10:C3:7B",
				"14:DD:A9",
				"18:31:BF",
				"1C:B7:2C",
				"20:CF:30",
				"2C:4D:54",
				"2C:56:DC",
				"2C:FD:A1",
				"30:5A:3A",
				"30:85:A9",
				"34:97:F6",
				"38:2C:4A",
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
				"D8:50:E6",
				"E0:3F:49",
				"E0:CB:4E",
				"F0:B4:29"
		};


		public bool IsOpen
        {
            get
            {
                return NetworkType == "[ESS]" || NetworkType == "[WPS][ESS]";
            }
        }

        public bool IsWithVPN
        {
            get
            {
				return _bssIdsWithVPN.Any(w => BssID.StartsWithNullSafe(w));
            }
        }

        public WifiNetworkDto()
        {
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
        }

        public override string ToString()
        {
            return BssID + " " + Name + " " +  Math.Abs(Level).ToString ();
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
