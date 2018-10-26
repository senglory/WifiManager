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
                    SetProperty(ref firstConnectPublicIP, value, "FirstConnectPublicIP");
            }
        }

        string internalIP;
        public string InternalIP
        {
            get { return internalIP; }
            set
            {
                SetProperty(ref internalIP, value, "InternalIP");
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
                SetProperty(ref isInCSVList, value, "IsInCSVList");
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
                SetProperty(ref isSelected, value, "IsSelected");
                //if (isSelected)
                //    DtoNetworkStateForColoring = DtoNetworkState.IsSelected;
                //else
                //    DtoNetworkStateForColoring = DtoNetworkState.Default;
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
                SetProperty(ref isEnabled, value, "IsEnabled");
                //if (isEnabled)
                //    DtoNetworkStateForColoring = DtoNetworkState.Default; 
                //else
                //    DtoNetworkStateForColoring = DtoNetworkState.IsDisabled;
            }
        }


        DtoNetworkState dtoNetworkStateForColoring;
        public DtoNetworkState DtoNetworkStateForColoring
        {
            get { return dtoNetworkStateForColoring; }
            set
            {
                SetProperty(ref dtoNetworkStateForColoring, value, "DtoNetworkStateForColoring");
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
                SetProperty(ref _CoordsAndPower, value, "CoordsAndPower");
            }
        }


        public bool IsVulnerable
        {
            get
            {
                return !string.IsNullOrWhiteSpace(BssID) && 
                    (
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
                    BssID.StartsWith("C8:60:00")
                    ||
                    BssID.StartsWith("C8:3A:35")
                    ||
                    BssID.StartsWith("C0:25:E9")
                    ||
                    BssID.StartsWith("B4:75:0E")
                    ||
                    BssID.StartsWith("B0:B2:DC")
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
                    BssID.StartsWith("5C:F4:AB")
                    ||
                    BssID.StartsWith("50:67:F0")
                    ||
                    BssID.StartsWith("50:67:F0")
                    ||
                    BssID.StartsWith("50:46:5D")
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
                    BssID.StartsWith("28:C6:8E") // Keenetic
                    ||
                    BssID.StartsWith("28:28:5D")
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
                    ||
                    BssID.StartsWith("00:0E:8F")
                    ||
                    BssID.StartsWith("00:1F:CE")
                    ||
                    BssID.StartsWith("00:18:E7")
                    ||
                    BssID.StartsWith("00:0E:8F")
                    );
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
        }
    }
}
