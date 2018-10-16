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
                SetProperty(ref _selectedNetwork, value, "SelectedNetwork");
            }
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
            SaveJsonCommand = new Command(DoSaveJson);
            ConnectCommand = new Command(ExecuteConnect);
            DisconnectCommand = new Command(DoDisconnect);
            RefreshNetworksCommand = new Command(DoRefreshNetworks);
            DeleteNetworkCommand = new Command(DoDeleteNetwork);

            IsConnected = mgr.IsConnected();
        }


        public void SortList()
        {
            var lst1 = WifiNetworks.OrderBy(nw => Math.Abs( nw.Level));
            //var lst1 = WifiNetworks.OrderBy(nw => nw.Name).ThenByDescending(nw => Math.Abs( nw.Level));
            WifiNetworks = new ObservableCollection<WifiNetworkDto>(lst1);
        }

        public void DoRefreshNetworks()
        {
            try
            {
                IsBusy = true;

                var lst1 = mgr.GetActiveWifiNetworks();
                // for quick search
                var allrecsQuickSearch = new Dictionary<string, WifiNetworkDto>();
                foreach (var item in lst1)
                {
                    allrecsQuickSearch.Add (item.BssID, item);
                }


                if (mgr.CanLoadFromFile())
                {
                    var lst2 = mgr.GetWifiNetworksFromCSV();
                    foreach (var existingWifi in lst1)
                    {
                        var wifiDtoFromFile = lst2.GetExistingWifiDto(existingWifi);
                        var isOnAir = wifiDtoFromFile != null;
                        if (isOnAir)
                        {
                            // update existing Wifi info from file (except for BSSID)
                            existingWifi.IsInCSVList = isOnAir;
                            existingWifi.IsEnabled = wifiDtoFromFile.IsEnabled;
                            existingWifi.Password = wifiDtoFromFile.Password;
                            existingWifi.Provider = wifiDtoFromFile.Provider;
                        }
                    }
                }
                WifiNetworks = new ObservableCollection<WifiNetworkDto>(lst1);
                SortList();
            }
            finally
            {
                IsBusy = false;
            }
        }

        void DoDeleteNetwork(object networkToDelete)
        {
            var dto = networkToDelete as WifiNetworkDto;
            WifiNetworks.Remove(dto);
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
        public Command SaveJsonCommand { get; set; }
        public Command RefreshNetworksCommand { get; set; }
        public Command ConnectCommand { get; set; }
        public Command DisconnectCommand { get; set; }
        public Command DeleteNetworkCommand { get; set; }


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
