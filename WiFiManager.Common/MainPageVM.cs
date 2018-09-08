using System;
using System.IO;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Linq;
using Xamarin.Forms.PlatformConfiguration;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using WiFiManager.Common.BusinessObjects;

using Newtonsoft.Json;



namespace WiFiManager.Common
{
    public class MainPageVM : INotifyPropertyChanged
    {
        //string _filePath;
        string filePathCSV;


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
                _allrecsQuickSearch.Clear();
                foreach (var nw in _WifiNetworks)
                {
                    _allrecsQuickSearch.Add(nw.BssID, nw);
                }
                SetProperty(ref _WifiNetworks, value, "WifiNetworks");
            }
        }


        private WifiNetworkDto _selectedNetwork;
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

        public event PropertyChangedEventHandler PropertyChanged;

        public MainPageVM(List<WifiNetworkDto> networks, string filePathCSV)
        {
            this.filePathCSV = filePathCSV;
            WifiNetworks = new ObservableCollection<WifiNetworkDto>(networks);
            SaveCommand = new Command(DoSave);
            ConnectDisconnectCommand = new Command(ExecuteConnectDisconnectCommand);
            RefreshNetworksCommand= new Command(DoRefreshNetworks);
        }

        public void SortList()
        {
            WifiNetworks = new ObservableCollection<WifiNetworkDto>(WifiNetworks.OrderByDescending(nw => nw.IsInCSVList).OrderBy(nw => nw.IsEnabled).OrderBy(nw => nw.Name));
        }

        void DoRefreshNetworks()
        {


        }


        void DoSave(object parameter)
        {
            //JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            //string str = JsonConvert.SerializeObject(WifiNetworks, settings);
            //File.WriteAllText(_filePath, str);
            SaveToCSV();
        }

        public void AppendNetworksFromCSV( string csvFilePath)
        {
            if (!File.Exists(csvFilePath))
                return;
            using (var fs = new FileStream(csvFilePath, FileMode.Open))
            {
                using (var fr = new StreamReader(fs))
                {
                    fr.ReadLine();
                    while (!fr.EndOfStream)
                    {
                        var s = fr.ReadLine();
                        var arrs = s.Split(new char[] { ';' });
                        var nw = new WifiNetworkDto
                        {
                            BssID = arrs[1].ToUpper(),
                            Name = arrs[0],
                            Password = arrs[2],
                            IsEnabled = !Convert.ToBoolean(int.Parse ( arrs[3])),
                            NetworkType = arrs[4]
                        };
                        var detected = AllRecordsQuickSearch.ContainsKey(nw.BssID);
                        nw.IsInCSVList = detected;
                        if (detected)
                            AllRecordsQuickSearch[nw.BssID].IsEnabled = nw.IsEnabled;
                        WifiNetworks.Add(nw);
                    }
                }
            }
        }
        void SaveToCSV()
        {
            using (var fs = new FileStream(filePathCSV, FileMode.Create))
            {
                using (var fw = new StreamWriter(fs))
                {
                    fw.WriteLine("Name;Bssid;Password;IsBanned;NetworkType");
                    foreach (var nw in WifiNetworks)
                    {
                        var isBanned = nw.IsEnabled ? 0 : 1;
                        fw.WriteLine($"{nw.Name};{nw.BssID};{nw.Password};{isBanned};{nw.NetworkType}");
                    }
                }
            }
        }

        void ExecuteConnectDisconnectCommand(object parameter)
        {

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
