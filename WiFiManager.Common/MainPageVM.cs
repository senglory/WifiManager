using System;
using System.IO;
using System.Collections.Generic;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

using WiFiManager.Common.BusinessObjects;

namespace WiFiManager.Common
{
    public class MainPageVM : INotifyPropertyChanged
    {
        string _filePath;
        ObservableCollection<WifiNetwork> _WifiNetworks;
        public ObservableCollection<WifiNetwork> WifiNetworks
        {
            get
            {
                return _WifiNetworks;
            }
            set
            {
                _WifiNetworks = value;
                SetProperty(ref _WifiNetworks, value, "WifiNetworks");
            }
        }


        private WifiNetwork _selectedNetwork;
        public WifiNetwork SelectedNetwork
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

        public MainPageVM()
        {
            WifiNetworks = new ObservableCollection<WifiNetwork>();
            SaveCommand = new Command(DoSave);
        }



        void DoSave(object parameter)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
            string str = JsonConvert.SerializeObject(WifiNetworks, settings);
            File.WriteAllText(_filePath, str);
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

        public void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Command SaveCommand { get; set; }
    }
}
