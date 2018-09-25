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
        ObservableCollection<WifiNetworkDto> _WifiNetworks;
        public ObservableCollection<WifiNetworkDto> WifiNetworks
        {
            get
            {
                return _WifiNetworks;
            }
            set
            {
                _WifiNetworks = value;
                AllRecordsQuickSearch.Clear();
                foreach (var nw in _WifiNetworks)
                {
                    if (string.IsNullOrEmpty(nw.BssID))
                        continue;
                    AllRecordsQuickSearch.Add(nw.BssID, nw);
                }
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
                _selectedNetwork = value;
                SetProperty(ref _selectedNetwork, value, "SelectedNetwork");
            }
        }

        Dictionary<string, WifiNetworkDto> _allrecsQuickSearch = new Dictionary<string, WifiNetworkDto>();
        public Dictionary<string, WifiNetworkDto> AllRecordsQuickSearch
        {
            get { return _allrecsQuickSearch; }
        }


        bool isBusy;
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if (isBusy == value)
                    return;

                isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        bool isConnected;
        public bool IsConnected
        {
            get { return isConnected; }
            set
            {
                if (isConnected == value)
                    return;

                isConnected = value;
                OnPropertyChanged("IsConnected");
            }
        } 
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;



        public MainPageVM(IWifiOperations mgr) {
            this.mgr = mgr;

            SaveCommand = new Command(DoSave);
            ConnectDisconnectCommand = new Command(ExecuteConnectDisconnectCommand);
            RefreshNetworksCommand = new Command(DoRefreshNetworks);
        }


        public void SortList()
        {
            var lst1 = WifiNetworks.OrderByDescending(nw => nw.IsInCSVList);
            var lst2 = lst1.OrderBy(nw => nw.IsEnabled).OrderBy(nw => nw.Name).ToList();
            WifiNetworks = new ObservableCollection<WifiNetworkDto>(lst1);
        }

        public void DoRefreshNetworks()
        {
            var lst1 = mgr.GetActiveWifiNetworks();
            WifiNetworks = new ObservableCollection<WifiNetworkDto>(lst1);

            if (mgr.CanLoadFromFile())
            {
                var lst2 = mgr.GetWifiNetworksFromCSV( );
                foreach (var wifiDtoFromFile in lst2)
                {
                    var existingWifi = GetExistingWifiDto(wifiDtoFromFile);
                    var isInCSVList = existingWifi != null;
                    wifiDtoFromFile.IsInCSVList = isInCSVList;
                    if (isInCSVList 
                        && AllRecordsQuickSearch .ContainsKey(existingWifi.BssID))
                    {
                        // update existing Wifi info from file
                        AllRecordsQuickSearch[existingWifi.BssID].IsInCSVList = wifiDtoFromFile.IsInCSVList;
                        AllRecordsQuickSearch[existingWifi.BssID].IsEnabled = wifiDtoFromFile.IsEnabled;
                        AllRecordsQuickSearch[existingWifi.BssID].Password = wifiDtoFromFile.Password;
                        AllRecordsQuickSearch[existingWifi.BssID].Provider = wifiDtoFromFile.Provider;
                    }
                    else
                    {
                        // wifi not in CSV list - add it
                        WifiNetworks.Add(wifiDtoFromFile);
                        if (!AllRecordsQuickSearch.ContainsKey(wifiDtoFromFile.BssID))
                            AllRecordsQuickSearch.Add(wifiDtoFromFile.BssID, wifiDtoFromFile);
                    }
                }
            }
            SortList();
        }

        void ExecuteConnectDisconnectCommand(object parameter)
        {

        }

        void DoSave(object parameter)
        {
            var lst = new List<WifiNetworkDto>(WifiNetworks);
            mgr.SaveToCSV(lst);
        }

        void DoSaveJson(object parameter)
        {
            var lst = new List<WifiNetworkDto>(WifiNetworks);
            mgr.SaveToJSON(lst);
        }


        WifiNetworkDto GetExistingWifiDto(WifiNetworkDto wifiDtoFromFile)
        {
            if (string.IsNullOrEmpty(wifiDtoFromFile.BssID))
            {
                var foundByName = WifiNetworks.FirstOrDefault(r => r.Name == wifiDtoFromFile.Name);
                return foundByName;
            }
            if (AllRecordsQuickSearch.ContainsKey(wifiDtoFromFile.BssID))
            {
                return AllRecordsQuickSearch[wifiDtoFromFile.BssID];
            }
            else
            {
                return null;
            }
        }




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

        public Command SaveCommand { get; set; }
        public Command RefreshNetworksCommand { get; set; }
        public Command ConnectDisconnectCommand { get; set; }


        //private Command loadTweetsCommand;

        //public Command LoadTweetsCommand
        //{
        //    get
        //    {
        //        return loadTweetsCommand ?? (loadTweetsCommand = new Command(ExecuteLoadTweetsCommand, () =>
        //        {
        //            return !IsBusy;
        //        }));
        //    }
        //}

        //private async void ExecuteLoadTweetsCommand()
        //{
        //    if (IsBusy)
        //        return;

        //    IsBusy = true;
        //    LoadTweetsCommand.ChangeCanExecute();

        //    //DoStuff

        //    IsBusy = false;
        //    LoadTweetsCommand.ChangeCanExecute();
        //}

        //private Command _ConnectDisconnectCommand;
        //public const string ConnectDisconnectCommandPropertyName = "ConnectDisconnectCommand";

        //public Command ConnectDisconnectCommand
        //{
        //    get
        //    {
        //        /*the false returned in second constructor parameter will mean that button bound to this command 
        //        will alwasy be disabled; please change to your logic eg IsBusy view model property*/

        //        return _ConnectDisconnectCommand ?? (_ConnectDisconnectCommand =
        //            new Command(
        //                async () => await ExecuteConnectDisconnectCommand(),
        //                () => false)
        //            );
        //    } 
        //}
    }
}
