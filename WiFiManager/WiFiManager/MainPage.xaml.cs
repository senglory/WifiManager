using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using WiFiManager.Common.BusinessObjects;
using WiFiManager.Common;


using Xamarin.Forms;
using Xamarin.Forms.Xaml;
//using Android.Net.Wifi;
//using Android.Locations;
using Plugin.Geolocator;

namespace WiFiManager
{
    public partial class MainPage : ContentPage
    {
        IWifiOperations mgr;

        public MainPage(IWifiOperations mgr)
        {
            this.mgr = mgr;
            InitializeComponent();
            this.BindingContext = mgr.GetActiveWifiNetworks();
        }

        private void Refresh_Clicked(object sender, EventArgs e)
        {
            this.BindingContext = mgr.GetActiveWifiNetworks();
        }



        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var netw = e.SelectedItem as WifiNetwork;
            netw.CoordsAndPower.Clear();
            DetailsArea.BindingContext = e.SelectedItem;
        }

        private async void RefreshCoords_Clicked(object sender, EventArgs e)
        {
            var mpv = this.BindingContext as MainPageVM;
            var netw = mpv.SelectedNetwork.CoordsAndPower;
            //LocationManager LocMgr = Android.App.Application.Context.GetSystemService("location") as LocationManager;
            //var locationCriteria = new Criteria();
            //locationCriteria.Accuracy = Accuracy.High;
            //locationCriteria.PowerRequirement = Power.High;
            //var locationProvider = LocMgr.GetBestProvider(locationCriteria, true);
            //var lastLocation = LocMgr.GetLastKnownLocation(locationProvider);
            //netw.Add(new CoordsAndPower {
            //    Lat = lastLocation.Latitude,
            //    Long=lastLocation.Longitude,
            //    Alt=lastLocation.Altitude
            //});

            var locator = CrossGeolocator.Current;
            locator.DesiredAccuracy = 1;

            var position = await locator.GetPositionAsync(new TimeSpan(0, 0, 0, 0, 10000));
            netw.Add(new CoordsAndPower
            {
                Lat = position.Latitude,
                Long = position.Longitude,
                Alt = position.Altitude
            });
        }
    }
}
