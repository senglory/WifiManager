using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace WiFiManager.Common.BusinessObjects
{
    public class WifiNetworkDto : BaseObj
    {
        public string BssID { get; set; }
        public string NetworkType { get; set; }
        public string Password { get; set; }
        public string WpsPin { get; set; }
        public int Level { get; set; }
        public bool IsSelected{ get; set; }
        public bool IsEnabled { get; set; }
        public bool IsInCSVList { get; set; }

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
        public override bool Fit(string filter)
        {
            var r = base.Fit(filter);
            return r || BssID.Contains(filter);
        }
    }
}
