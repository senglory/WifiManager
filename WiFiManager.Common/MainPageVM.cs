using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Plugin.Logging;

using WiFiManager.Common.BusinessObjects;
using System.Windows.Input;

namespace WiFiManager.Common
{
    public class MainPageVM : INotifyPropertyChanged
    {
        IWifiManagerOperations mgr;

        #region Properties
        #region Different lists
        WifiNetworksObservableCollection _wifiNetworks = new WifiNetworksObservableCollection();
        public WifiNetworksObservableCollection WifiNetworks
        {
            get
            {
                return _wifiNetworks;
            }
            set
            {
                SetProperty(ref _wifiNetworks, value, nameof(WifiNetworks));
            }
        }

        WifiNetworksObservableCollection _wifiNetworksInLookup = new WifiNetworksObservableCollection();
        public WifiNetworksObservableCollection WifiNetworksInLookup
        {
            get
            {
                return _wifiNetworksInLookup;
            }
            set
            {
                SetProperty(ref _wifiNetworksInLookup, value, nameof(WifiNetworksInLookup));
            }
        }

        WifiNetworksObservableCollection _wifiNetworksHunting = new WifiNetworksObservableCollection();
        public WifiNetworksObservableCollection WifiNetworksHunting
        {
            get
            {
                return _wifiNetworksHunting;
            }
            set
            {
                SetProperty(ref _wifiNetworksHunting, value, nameof(WifiNetworksHunting));
            }
        }

        WifiNetworksObservableCollection _wifiNetworksSaveList = new WifiNetworksObservableCollection();
        public WifiNetworksObservableCollection WifiNetworksSaveList
        {
            get
            {
                return _wifiNetworksSaveList;
            }
            set
            {
                SetProperty(ref _wifiNetworksSaveList, value, nameof(WifiNetworksSaveList));
            }
        } 
        #endregion

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

        public bool IsNotBusy
        {
            get { return !isBusy; }
            set
            {
                SetProperty(ref isBusy, !value, nameof(IsNotBusy));
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

        public bool UseTAB
        {
            get { return mgr.UseTAB; }
            set
            {
                bool useTAB = value;
                mgr.UseTAB = useTAB;
                SetProperty(ref useTAB, value, nameof(UseTAB));
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

        bool useWithVPNonly;
        public bool WithVPNOnly
        {
            get { return useWithVPNonly; }
            set
            {
                SetProperty(ref useWithVPNonly, value, nameof(WithVPNOnly));
            }
        }

        bool useShakeForRefresh;
        public bool UseShakeForRefresh
        {
            get { return useShakeForRefresh; }
            set
            {
                SetProperty(ref useShakeForRefresh, value, nameof(UseShakeForRefresh));
            }
        }

        bool doDisconnectBeforeRefresh;
        public bool DoDisconnectBeforeRefresh
        {
            get { return doDisconnectBeforeRefresh; }
            set
            {
                SetProperty(ref doDisconnectBeforeRefresh, value, nameof(DoDisconnectBeforeRefresh));
            }
        }

        bool updateOnlyEmptyInfo;
        public bool UpdateOnlyEmptyInfo
        {
            get { return updateOnlyEmptyInfo; }
            set
            {
                SetProperty(ref updateOnlyEmptyInfo, value, nameof(UpdateOnlyEmptyInfo));
            }
        }

        bool isNightTheme = false;
        public bool IsNightTheme
        {
            get { return isNightTheme; }
            set
            {
                SetProperty(ref isNightTheme, value, nameof(IsNightTheme));
            }
        }


        string wiFiNameOrBssIdLookup;
        public string  WiFiNameOrBssIdLookup
        {
            get { return wiFiNameOrBssIdLookup; }
            set
            {
                SetProperty(ref wiFiNameOrBssIdLookup, value, nameof(WiFiNameOrBssIdLookup));
            }
        }

        bool tryCopyFromBluetoothFolder;
        public bool TryCopyFromBluetoothFolder
        {
            get { return tryCopyFromBluetoothFolder; }
            set
            {
                SetProperty(ref tryCopyFromBluetoothFolder, value, nameof(TryCopyFromBluetoothFolder));
            }
        }

        bool dumpRawList;
        public bool DumpRawList
        {
            get { return dumpRawList; }
            set
            {
                SetProperty(ref dumpRawList, value, nameof(DumpRawList));
            }
        }

        bool searchByBssIDOnly;
        public bool SearchByBssIDOnly
        {
            get { return searchByBssIDOnly; }
            set
            {
                SetProperty(ref searchByBssIDOnly, value, nameof(SearchByBssIDOnly));
            }
        }

        bool scanWifiAndGps = true;
        public bool ScanWifiAndGps
        {
            get { return scanWifiAndGps; }
            set
            {
                SetProperty(ref scanWifiAndGps, value, nameof(ScanWifiAndGps));
            }
        }

        bool isFailed;
        public bool IsFailed
        {
            get { return isFailed; }
            set
            {
                SetProperty(ref isFailed, value, nameof(IsFailed));
            }
        }

        #endregion

        public string FirstFailedLineInCSV;


        /// <summary>
        ///    Just FOR xaml
        /// </summary>
        public MainPageVM()
        {

        }

        public void AddMgr(IWifiManagerOperations mgr)
        {
            this.mgr = mgr;

            IsConnected = mgr.IsConnected();
        }

        public async Task DoRefreshNetworks()
        {
            try
            {
                IsFailed = false;

                var ts0 = DateTime.Now;

                FirstFailedLineInCSV = null;
                if (DoDisconnectBeforeRefresh)
                {
                    mgr.DeleteInfoAboutWifiNetworks();
                }
                if (TryCopyFromBluetoothFolder)
                {
                    await mgr.MoveCSVFromBluetoothFolder();
                }
                // clean CSV cache if it was used
                mgr.ClearCachedCSVNetworkList();
                var allOnAir = mgr.GetActiveWifiNetworks();

                //var ts1 = DateTime.Now;
                //var elapsed = ts1 - ts0;
                //Logging.Info("WiFiManager", $"GetActiveWifiNetworks: finished, elapsed: {elapsed.TotalSeconds} sec");

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
                if (mgr.CanLoadFromFile)
                {
                    for (int i = 0; i < allOnAir.Count; i++)
                    {
                        var wifiOnAir = allOnAir[i];
                        var wifiDtoFromFile = await mgr.FindWifiInCSV(wifiOnAir, SearchByBssIDOnly);
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
                    allOnAir = allOnAir.Where(x => x.IsWEP).ToList();
                }

                if (WithVPNOnly)
                {
                    allOnAir = allOnAir.Where(x => x.IsWithVPN).ToList();
                }
                // put currently connected on the top
                var lst1 = allOnAir.OrderByDescending(nw => nw.IsBeingUsed). ThenBy(nw => nw.IsInCSVList).ThenBy(nw => Math.Abs(nw.Level));
                WifiNetworks = new WifiNetworksObservableCollection(lst1);


                //var ts2 = DateTime.Now;
                //var elapsed2 = ts2 - ts0;
                //Logging.Info("WiFiManager", $"DoRefreshNetworks: finished, elapsed: {elapsed2.TotalSeconds} sec");

                IsConnected = mgr.IsConnected();
                SelectedNetwork = null;

                if (ScanWifiAndGps)
                {
                    await DoRefreshCoords();
                }

                //var v = Plugin.Vibrate.CrossVibrate.Current;
                //v.Vibration(TimeSpan.FromSeconds(0.5));
            }
            catch (Exception ex)
            {
                IsFailed = true;
                Logging.Error("DoRefreshNetworks", ex);
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
                    try
                    {
                        if (!t2.IsFaulted)
                        {
                            for (int i = 0; i < WifiNetworks.Count; i++)
                            {
                                var wifi = WifiNetworks[i];
                                if (UpdateOnlyEmptyInfo && !wifi.HasCoords
                                    ||
                                    !UpdateOnlyEmptyInfo
                                    )
                                {
                                    wifi.TryUpdateRecentCoords(t2.Result);
                                }
                            }
                            Thread.Sleep(Constants.REFRESH_COORDS_WAIT_MS);
                        }
                        else
                        {
                            IsFailed = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        IsFailed = true;
                        Logging.Error("DoRefreshCoords", ex);
                        throw;
                    }
                });
            }
            catch (Exception ex)
            {
                IsFailed = true;
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

        public async Task DoSave(WifiNetworkDto theOne=null)
        {
            try
            {
                Device.BeginInvokeOnMainThread(() => {
                    IsBusy = true;
                });
                List<WifiNetworkDto> lst;
                if (DumpRawList)
                {
                    lst = new List<WifiNetworkDto>(WifiNetworks);
                    await mgr.DumpRawListAsync(lst);
                }
                else
                {
                    lst = new List<WifiNetworkDto>();
                    if (null == theOne)
                    {
                        if (WifiNetworksSaveList.Count > 0)
                            lst.AddRange(WifiNetworksSaveList);
                        else
                            lst.AddRange(WifiNetworks);
                    }
                    else
                        lst.Add(theOne);
                    await mgr.SaveToCSVAsync(lst);
                }
            }
            catch
            {
                IsFailed = true;
            }
            finally
            {
                WifiNetworksSaveList.Clear();
                Device.BeginInvokeOnMainThread(() => {
                    IsBusy = false;
                });
            }
        }


        public ICommand SaveCommand => new Command(async x => { await DoSave(null); });
        public ICommand RefreshNetworksCommand => new Command(
            async (x) => {
                try
                {
                    Device.BeginInvokeOnMainThread(() => {
                        IsBusy = true;
                    });

                    await DoRefreshNetworks();
                }
                finally
                {
                    Device.BeginInvokeOnMainThread(() => {
                        IsBusy = false;
                    });
                }
            }
            ,
            (x) =>
            {
                // Return true if there's something to search for.
                return !IsBusy;
            }
            );
        public ICommand ConnectCommand => new Command(ExecuteConnect);
        public ICommand DisconnectCommand => new Command(DoDisconnect);
        public ICommand StopHuntingCommand => new Command(DoStopHunting);

        public ICommand DoLookupCommand => new Command<string>( 
            async(x) => {
                try
                {
                    IsBusy = true;

                    WifiNetworksInLookup.Clear();
                    if (string.IsNullOrWhiteSpace(WiFiNameOrBssIdLookup))
                        return;
                    var lst = await mgr.FindWifiInCSV(WiFiNameOrBssIdLookup);
                    foreach (var wifi in lst)
                    {
                        WifiNetworksInLookup.Add(wifi);
                    }
                }
                catch (Exception ex)
                {
                    IsFailed = true;
                }
                finally
                {
                    IsBusy = false;
                }
            }
            ,
            ( x) =>
            {
                // Return true if there's something to search for.
                return !string.IsNullOrWhiteSpace (  WiFiNameOrBssIdLookup);
            }
            );

        public ICommand CreateUnixFilesCommand => new Command(
            x => {
                mgr.CreateUnixFiles(SelectedNetwork );
            }
            ,
            (x) =>
            {
                // Return true if there's a long-running operation.
                return !IsBusy;
            }
            );
        //DoLookupCommand = new Command((wifiNameOrBssId) => { this.DoLookup(wifiNameOrBssId); });
        //DoLookupCommand = new Command(async x => await DoLookupAsync((string)x));


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
