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

        public ObservableCollection<CoordsAndPower> CoordsAndPower { get; set; }

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
    }
}
