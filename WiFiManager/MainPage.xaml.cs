using System;

using Xamarin.Forms;
using Plugin.Geolocator;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms.PlatformConfiguration;

using WiFiManager.Common.BusinessObjects;
using WiFiManager.Common;



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



        private async void RefreshCoords_Clicked(object sender, EventArgs e)
        {
            var hasPermission = await Utils.CheckPermissions(Permission.Location);
            if (!hasPermission)
                return;

            try
            {
                pleaseWait.IsVisible = true;
                pleaseWait.IsRunning = true;

                var mpv = this.BindingContext as MainPageVM;
                var coords = mpv.SelectedNetwork.CoordsAndPower;
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(() => {
                    DisplayAlert("Error", ex.Message, "OK");
                });
            }
            finally
            {
                pleaseWait.IsVisible = false;
                pleaseWait.IsRunning = false;
            }

        }

        private void ConnDisconn_Clicked(object sender, EventArgs e)
        {
            try
            {
                Device.BeginInvokeOnMainThread(() => {
                    pleaseWait.IsVisible = true;
                    pleaseWait.IsRunning = true;
                });

                var mpv = this.BindingContext as MainPageVM;
                var nw = mpv.SelectedNetwork;
                var bres = mgr.Connect(nw.BssID,  nw.Name,nw.Password);
                if (!bres)
                {
                    Device.BeginInvokeOnMainThread(() => {
                        DisplayAlert("Error", "Can't connect", "OK");
                    });
                }
            }
            catch(Exception ex)
            {
                Device.BeginInvokeOnMainThread(() => {
                    DisplayAlert("Error", ex.Message, "OK");
                });
            }
            finally
            {
                Device.BeginInvokeOnMainThread(() => {
                    pleaseWait.IsVisible = false;
                    pleaseWait.IsRunning = false;
                });
            }
        }

        private async void RefreshNetworks_Clicked(object sender, EventArgs e)
        {
            try
            {
                pleaseWait.IsVisible = true;
                pleaseWait.IsRunning = true;

                this.BindingContext = await mgr.GetActiveWifiNetworksAsync(); 
            }
            catch (Exception ex)
            {
                int rr = 0;
            }
            finally
            {
                pleaseWait.IsVisible = false;
                pleaseWait.IsRunning = false;
            }
        }
    }
}
