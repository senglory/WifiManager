using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
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

        public bool IsVulnerable
        {
            get
            {
                return !string.IsNullOrWhiteSpace(BssID) 
                    &&
                    NetworkType.Contains("WPS")
                    &&
                    (
                    BssID.StartsWith("00:0E:8F")
                    ||
                    BssID.StartsWith("00:1F:CE")
                    ||
                    BssID.StartsWith("00:18:E7")
                    ||
                    BssID.StartsWith("00:0E:8F")
                    ||
                    BssID.StartsWith("F8:C0:91")
                    ||
                    BssID.StartsWith("EE:43:F6")
                    ||
                    BssID.StartsWith("EC:43:F6")
                    ||
                    BssID.StartsWith("EA:37:7A")
                    ||
                    BssID.StartsWith("E8:CD:2D")
                    ||
                    BssID.StartsWith("E8:37:7A")
                    ||
                    BssID.StartsWith("E4:F4:C6")
                    ||
                    BssID.StartsWith("E4:BE:ED")
                    ||
                    BssID.StartsWith("E0:3F:49")
                    ||
                    BssID.StartsWith("D8:50:E6") // ASUS
                    ||
                    BssID.StartsWith("D8:EB:97")
                    ||
                    BssID.StartsWith("D4:BF:7F")
                    ||
                    BssID.StartsWith("D4:6E:0E")
                    ||
                    BssID.StartsWith("D4:21:22")
                    ||
                    BssID.StartsWith("CC:5D:4E")
                    ||
                    BssID.StartsWith("C8:6C:87")
                    ||
                    BssID.StartsWith("C8:60:00") // ASUS
                    ||
                    BssID.StartsWith("C8:3A:35")
                    ||
                    BssID.StartsWith("C0:25:E9")
                    ||
                    BssID.StartsWith("B4:75:0E")
                    ||
                    BssID.StartsWith("B0:B2:DC") // Keenetic
                    ||
                    BssID.StartsWith("B0:4E:26")
                    ||
                    BssID.StartsWith("94:4A:0C")
                    ||
                    BssID.StartsWith("90:EF:68")
                    ||
                    BssID.StartsWith("90:94:E4")
                    ||
                    BssID.StartsWith("84:C9:B2")
                    ||
                    BssID.StartsWith("78:44:76")
                    ||
                    BssID.StartsWith("74:DA:38")
                    ||
                    BssID.StartsWith("6A:28:5D")
                    ||
                    BssID.StartsWith("60:31:97")
                    ||
                    BssID.StartsWith("60:A4:4C") // ASUS
                    ||
                    BssID.StartsWith("5C:F4:AB")
                    ||
                    BssID.StartsWith("50:67:F0")
                    ||
                    BssID.StartsWith("50:67:F0")
                    ||
                    BssID.StartsWith("50:46:5D") // ASUS
                    ||
                    BssID.StartsWith("4E:5D:4E")
                    ||
                    BssID.StartsWith("4C:60:DE")
                    ||
                    BssID.StartsWith("40:4A:03")
                    ||
                    BssID.StartsWith("40:16:7E")
                    ||
                    BssID.StartsWith("38:2C:4A") // ASUS
                    ||
                    BssID.StartsWith("38:D5:47") // ASUS
                    ||
                    BssID.StartsWith("28:C6:8E") // Keenetic
                    ||
                    BssID.StartsWith("28:28:5D") // Keenetic
                    ||
                    BssID.StartsWith("1C:B7:2C")
                    ||
                    BssID.StartsWith("1C:7E:E5")
                    ||
                    BssID.StartsWith("18:D6:C7")
                    ||
                    BssID.StartsWith("14:A9:E3")
                    ||
                    BssID.StartsWith("10:C3:7B")
                    ||
                    BssID.StartsWith("10:7B:EF")
                    ||
                    BssID.StartsWith("08:C6:B3")
                    ||
                    BssID.StartsWith("04:BF:6D")
                    );
            }
        }

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
                return BssID.StartsWith("38:2C:4A")
                    ||
                       BssID.StartsWith("AC:9E:17")
                    ||
                       BssID.StartsWith("E0:3F:49")
                    ||
                       BssID.StartsWith("10:C3:7B")
                    ||
                       BssID.StartsWith("50:E6:5F")
                    ||
                       BssID.StartsWith("70:4D:7B")
                    ||
                       BssID.StartsWith("88:D7:F6")
;


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
