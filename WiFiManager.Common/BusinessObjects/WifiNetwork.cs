using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace WiFiManager.Common.BusinessObjects
{
    public class WifiNetwork : BaseObj, INotifyPropertyChanged
    {
        bool _IsEnabled;
        public bool IsEnabled { get
            { return _IsEnabled; }
            set {
                if (_IsEnabled == value)
                    return;

                _IsEnabled = value;
                OnPropertyChanged("IsEnabled");
            }
        }
        public string BssID { get; set; }
        public string NetworkType { get; set; }
        public string Password { get; set; }
        public string WpsPin { get; set; }

        public ObservableCollection<CoordsAndPower> CoordsAndPower { get; set; }

        public WifiNetwork()
        {
            IsEnabled = true;
            CoordsAndPower = new ObservableCollection<CoordsAndPower>();
        }

        public override bool Fit(string filter)
        {
            var r = base.Fit(filter);
            return r || BssID.Contains(filter);
        }

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
    }
}
