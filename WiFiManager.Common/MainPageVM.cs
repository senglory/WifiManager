using System;
using System.IO;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using WiFiManager.Common.BusinessObjects;

using Xamarin.Forms.PlatformConfiguration;
using Newtonsoft.Json;
using AutoMapper;



namespace WiFiManager.Common
{
    public class MainPageVM : INotifyPropertyChanged
    {
        string filePathJSON;
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

        static readonly MapperConfiguration config;
        static readonly IMapper mapper;

        static MainPageVM()
        {
            config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<WifiNetwork, WifiNetworkDto>();
                cfg.CreateMap<WifiNetworkDto, WifiNetwork>();
                cfg.IgnoreUnmapped();
            });
            config.AssertConfigurationIsValid();
            mapper = config.CreateMapper();
        }

        public MainPageVM(List<WifiNetworkDto> networks, string filePathCSV, string filePathJSON)
        {
            this.filePathCSV = filePathCSV;
            this.filePathJSON = filePathJSON;
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
            SaveToCSV();
        }
        void DoSaveJson(object parameter)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            string str = JsonConvert.SerializeObject(WifiNetworks, settings);
            File.WriteAllText(filePathJSON, str);
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
                        var nw = new WifiNetwork
                        {
                            BssID = arrs[1].ToUpper(),
                            Name = arrs[0],
                            Password = arrs[2],
                            IsEnabled = !Convert.ToBoolean(int.Parse ( arrs[3])),
                            NetworkType = arrs[4],
                            Provider = arrs[5]
                        };
                        var nwdto = mapper.Map<WifiNetwork, WifiNetworkDto>(nw);
                        var isInCSVList = AllRecordsQuickSearch.ContainsKey(nwdto.BssID);
                        nwdto.IsInCSVList = isInCSVList;
                        if (isInCSVList)
                        {
                            AllRecordsQuickSearch[nw.BssID].IsInCSVList = nwdto.IsInCSVList;
                            AllRecordsQuickSearch[nw.BssID].IsEnabled = nwdto.IsEnabled;
                            AllRecordsQuickSearch[nw.BssID].Password = nwdto.Password;
                            AllRecordsQuickSearch[nw.BssID].Provider = nwdto.Provider;
                        }
                        else
                        {
                            WifiNetworks.Add(nwdto);
                        }
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
                    fw.WriteLine("Name;Bssid;Password;IsBanned;NetworkType;Provider");
                    foreach (var nw in WifiNetworks)
                    {
                        var isBanned = nw.IsEnabled ? 0 : 1;
                        fw.WriteLine($"{nw.Name};{nw.BssID};{nw.Password};{isBanned};{nw.NetworkType};{nw.Provider}");
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
