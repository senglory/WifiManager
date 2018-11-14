using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using WiFiManager.Common.BusinessObjects;

using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;



namespace WiFiManager.Common
{
    public class MainPageVM : INotifyPropertyChanged
    {
        IWifiOperations mgr;

        #region Properties
        ObservableCollection<WifiNetworkDto> _WifiNetworks=new ObservableCollection<WifiNetworkDto> ();
        public ObservableCollection<WifiNetworkDto> WifiNetworks
        {
            get
            {
                return _WifiNetworks;
            }
            set
            {
                SetProperty(ref _WifiNetworks, value, "WifiNetworks");
            }
        }


        WifiNetworkDto _selectedNetwork;
        public WifiNetworkDto SelectedNetwork
        {
            get
            {
                return _selectedNetwork;
            }
            set
            {
                if (_selectedNetwork != null)
                {
                    _selectedNetwork.IsSelected = false;
                }
                SetProperty(ref _selectedNetwork, value, "SelectedNetwork");
                //if (_selectedNetwork != null)
                //    _selectedNetwork.CoordsAndPower.Add(new CoordsAndPower
                //{
                //    Lat = 111,
                //    Long = 222,
                //    Alt = 333,
                //    Power = 3.7,
                //    When = DateTime.Now
                //});
            }
        }


        bool isBusy;
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                SetProperty(ref isBusy, value, "IsBusy");
            }
        }

        bool isConnected;
        public bool IsConnected
        {
            get { return isConnected; }
            set
            {
                SetProperty(ref isConnected, value, "IsConnected");
            }
        }
        #endregion


        public string FirstFailedLineInCSV;





        public MainPageVM(IWifiOperations mgr) {
            this.mgr = mgr;

            SaveCommand = new Command(DoSave);
            SaveJsonCommand = new Command(DoSaveJson);
            ConnectCommand = new Command(ExecuteConnect);
            DisconnectCommand = new Command(DoDisconnect);
            RefreshNetworksCommand = new Command(DoRefreshNetworks);
            RefreshCoordsCommand = new Command(DoRefreshCoords);

            IsConnected = mgr.IsConnected();
        }


        public void SortListByLevel()
        {
            var lst1 = WifiNetworks.OrderBy (nw => nw.IsInCSVList).ThenBy(nw => Math.Abs( nw.Level));
            WifiNetworks = new ObservableCollection<WifiNetworkDto>(lst1);
        }

        public void DoRefreshNetworks()
        {
            try
            {
                IsBusy = true;
                FirstFailedLineInCSV = null;

                var lst1 = mgr.GetActiveWifiNetworks();
                // for quick search
                var allrecsQuickSearch = new Dictionary<string, WifiNetworkDto>();
                foreach (var item in lst1)
                {
                    allrecsQuickSearch.Add (item.BssID, item);
                }


                if (mgr.CanLoadFromFile())
                {
                    var lst2 = mgr.GetWifiNetworksFromCSV(out FirstFailedLineInCSV);
                    foreach (var existingWifiDto in lst1)
                    {
                        var wifiDtoFromFile = lst2.GetExistingWifiDto(existingWifiDto);
                        var isOnAir = wifiDtoFromFile != null;
                        if (isOnAir)
                        {
                            // update existing Wifi info from file (except for BSSID)
                            existingWifiDto.IsInCSVList = isOnAir;
                            wifiDtoFromFile.CopyTo(existingWifiDto);
                        }
                    }
                }
                WifiNetworks = new ObservableCollection<WifiNetworkDto>(lst1);
                SortListByLevel();

                IsConnected = mgr.IsConnected();
                SelectedNetwork = null;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async void DoRefreshCoords()
        {
            try
            {
                IsBusy = true;
                var t = mgr.GetCoordsAsync();
                await t.ContinueWith((r) => {
                    foreach (var wifi in WifiNetworks)
                    {
                        wifi.TryUpdateCoords(r.Result);
                    }
                });
            }
            finally
            {
                IsBusy = false;
            }
        }

        void ExecuteConnect(object parameter)
        {

        }

        async void DoDisconnect()
        {
            try
            {
                IsBusy = true;
                await mgr.DisConnectAsync();
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void DoSave( )
        {
            var lst = new List<WifiNetworkDto>(WifiNetworks);
            mgr.SaveToCSV(lst);
        }

        void DoSaveJson(object parameter)
        {
            var lst = new List<WifiNetworkDto>(WifiNetworks);
            mgr.SaveToJSON(lst);
        }




        public Command SaveCommand { get; set; }
        public Command SaveJsonCommand { get; set; }
        public Command RefreshNetworksCommand { get; set; }
        public Command ConnectCommand { get; set; }
        public Command DisconnectCommand { get; set; }
        public Command RefreshCoordsCommand { get; set; }

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
