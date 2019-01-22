using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Xamarin.Forms;
using Plugin.Logging;

using WiFiManager.Common.BusinessObjects;



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
                SetProperty(ref _WifiNetworks, value, nameof(WifiNetworks));
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
                SetProperty(ref _selectedNetwork, value, nameof(SelectedNetwork));
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
                SetProperty(ref isBusy, value, nameof(IsBusy));
            }
        }

        bool isConnected;
        public bool IsConnected
        {
            get { return isConnected; }
            set
            {
                SetProperty(ref isConnected, value, nameof (IsConnected));
            }
        }


        public bool UseInternalStorageForCSV
        {
            get { return mgr.UseInternalStorageForCSV; }
            set
            {
                bool usePhoneStorage = value;
                mgr.UseInternalStorageForCSV = usePhoneStorage;
                SetProperty(ref usePhoneStorage, value, nameof(UseInternalStorageForCSV));
            }
        }

        public bool UseCachedNetworkLookup
        {
            get { return mgr.UseCachedNetworkLookup; }
            set
            {
                bool useRAM = value;
                mgr.UseCachedNetworkLookup = useRAM;
                SetProperty(ref useRAM, value, nameof(UseCachedNetworkLookup));
            }
        }

        bool useWEPonly;
        public bool WEPOnly
        {
            get { return useWEPonly; }
            set
            {
                SetProperty(ref useWEPonly, value, nameof (WEPOnly));
            }
        }


        ObservableCollection<WifiNetworkDto> _WifiNetworksHunting = new ObservableCollection<WifiNetworkDto>();
        public ObservableCollection<WifiNetworkDto> WifiNetworksHunting
        {
            get
            {
                return _WifiNetworksHunting;
            }
            set
            {
                SetProperty(ref _WifiNetworksHunting, value, nameof (WifiNetworksHunting));
            }
        }
        #endregion



        public string FirstFailedLineInCSV;





        public MainPageVM(IWifiOperations mgr)
        {
            this.mgr = mgr;

            SaveCommand = new Command(DoSave);
            SaveJsonCommand = new Command(DoSaveJson);
            ConnectCommand = new Command(ExecuteConnect);
            DisconnectCommand = new Command(DoDisconnect);
            RefreshNetworksCommand = new Command(DoRefreshNetworks);
            StopHuntingCommand = new Command(DoStopHunting);

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
                var ts0 = DateTime.Now;

                IsBusy = true;
                FirstFailedLineInCSV = null;

                mgr.DeleteInfoAboutWifiNetworks();
                var allOnAir = mgr.GetActiveWifiNetworks();

                var ts1 = DateTime.Now;
                var elapsed = ts1 - ts0;
                Logging.Info("WiFiManager", $"GetActiveWifiNetworks: finished, elapsed: {elapsed.TotalSeconds} sec");

                // in hunting mode leave only those who's in hunting list
                if (WifiNetworksHunting.Count > 0)
                {
                    // inner join
                    var results = from w1 in allOnAir
                                  join t2 in WifiNetworksHunting on w1.BssID equals t2.BssID
                                  select w1;
                    allOnAir = results.ToList();
                }

                // try to find info in CSV file
                if (mgr.CanLoadFromFile())
                {
                    foreach (var wifiOnAir in allOnAir)
                    {
                        var wifiDtoFromFile = mgr.FindWifiInCSV(wifiOnAir );
                        var isInFileAndOnAir = wifiDtoFromFile != null;
                        if (isInFileAndOnAir)
                        {
                            // update existing Wifi info from file (except for BSSID)
                            wifiOnAir.IsInCSVList = isInFileAndOnAir;
                            var nameOnAir = wifiOnAir.Name;
                            var bssidOnAir = wifiOnAir.BssID;
                            wifiDtoFromFile.CopyTo(wifiOnAir);
                            // only Name & BssID is taken from air
                            wifiOnAir.Name = nameOnAir;
                            wifiOnAir.BssID = bssidOnAir;
                        }
                    }
                }

                if (WEPOnly)
                {
                    allOnAir = allOnAir.Where(x => x.NetworkType.Contains("WEP")).ToList();
                }

                WifiNetworks = new ObservableCollection<WifiNetworkDto>(allOnAir);
                SortListByLevel();

                // clean CSV cache if it was used
                mgr.ClearCachedCSVNetworkList();

                var ts2 = DateTime.Now;
                var elapsed2 = ts2 - ts0;
                Logging.Info("WiFiManager", $"DoRefreshNetworks: finished, elapsed: {elapsed2.TotalSeconds} sec");


                IsConnected = mgr.IsConnected();
                SelectedNetwork = null;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void DoStopHunting()
        {
            WifiNetworksHunting.Clear();
        }

        public async Task DoRefreshCoords()
        {
            try
            {
                var t = mgr.GetCoordsAsync();
                await t.ContinueWith(t2 =>
                {
                    if (!t2.IsFaulted)
                    {
                        foreach (var wifi in WifiNetworks)
                        {
                            wifi.TryUpdateRecentCoords(t2.Result);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Logging.Error("DoRefreshCoords", ex);
            }
        }

        void ExecuteConnect(object parameter)
        {

        }

        void DoDisconnect()
        {
            try
            {
                IsBusy = true;
                Task.Run ( () =>  { mgr.DisConnectAsync(); });
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
        public Command StopHuntingCommand { get; set; }

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
