using System;
using WiFiManager.Common.BusinessObjects;
using WiFiManager.Common;


using Xamarin.Forms;
using Plugin.Geolocator;
using Plugin.Permissions.Abstractions;



namespace WiFiManager
{
    public partial class MainPage : ContentPage
    {
        IWifiOperations mgr;

        public MainPage(IWifiOperations mgr)
        {
            this.mgr = mgr;
            InitializeComponent();
            this.BindingContext =  mgr.GetActiveWifiNetworks();
        }

        private void Refresh_Clicked(object sender, EventArgs e)
        {
            this.BindingContext = mgr.GetActiveWifiNetworks();
        }



        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var netw = e.SelectedItem as WifiNetwork;
            //netw.CoordsAndPower.Clear();
            DetailsArea.BindingContext = netw;
            //lstCoords.ItemsSource = netw.CoordsAndPower;
        }

        private async void RefreshCoords_Clicked(object sender, EventArgs e)
        {
            var hasPermission = await Utils.CheckPermissions(Permission.Location);
            if (!hasPermission)
                return;

            try
            {
                pleaseWait.IsVisible = true;
                pleaseWait.IsRunning = true;// my activity indicator

                //await long operation here, i.e.:
                var mpv = this.BindingContext as MainPageVM;
                var coords = mpv.SelectedNetwork.CoordsAndPower;
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
                var includeHeading = true;

                var position = await locator.GetPositionAsync(TimeSpan.FromSeconds(10), null, includeHeading);
                if (position == null)
                {
                    return;
                }
                coords.Add(new CoordsAndPower
                {
                    Lat = position.Latitude,
                    Long = position.Longitude,
                    Alt = position.Altitude
                });
                //lstCoords.ItemsSource = coords;
            }
            finally
            {
                pleaseWait.IsVisible = false;
                pleaseWait.IsRunning = false;// my activity indicator
            }

        }

        private void ConnDisconn_Clicked(object sender, EventArgs e)
        {
            try
            {
                pleaseWait.IsVisible = true;
                pleaseWait.IsRunning = true;
            }
            finally
            {
                pleaseWait.IsVisible = false;
                pleaseWait.IsRunning = false;
            }
        }
    }
}
