using System;

using Xamarin.Forms;
using Plugin.Geolocator;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms.PlatformConfiguration;

using WiFiManager.Common.BusinessObjects;
using WiFiManager.Common;
using System.Threading.Tasks;

namespace WiFiManager
{
    public partial class MainPage : ContentPage
    {
        IWifiOperations mgr;

        public MainPage(IWifiOperations mgr)
        {
            this.mgr = mgr;
            this.BindingContext = new MainPageVM(mgr);
            RefreshAvailableNetworks();
            InitializeComponent();
            mgr.ConnectionStateChanged += Mgr_ConnectionStateChanged;
        }

        private void Mgr_ConnectionStateChanged(string connectionState)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                pleaseWait.IsVisible = false;
                pleaseWait.IsRunning = false;
            });
        }

        public void WifiConnectNotify()
        {
            var mpv = this.BindingContext as MainPageVM;
            mpv.IsConnected = true;
        }

        void Refresh_Clicked(object sender, EventArgs e)
        {
            RefreshAvailableNetworks();
        }

        void RefreshAvailableNetworks()
        {
            var mpv = this.BindingContext as MainPageVM;
            mpv.DoRefreshNetworks();
            //this.BindingContext = mgr.GetActiveWifiNetworks();
        }

        async void RefreshCoords_Clicked(object sender, EventArgs e)
        {
            var hasPermission = await Utils.CheckPermissions(Permission.Location);
            if (!hasPermission)
                return;

            try
            {
                Device.BeginInvokeOnMainThread(() => {
                    pleaseWait.IsVisible = true;
                    pleaseWait.IsRunning = true;
                });

                var mpv = this.BindingContext as MainPageVM;
                var coords = mpv.SelectedNetwork.CoordsAndPower;
                var t1 = mgr.ActualizeCoordsWifiNetworkAsync(mpv.SelectedNetwork);
                //t1.Start();
                var t2 = t1.ContinueWith( (www) => {

                    Device.BeginInvokeOnMainThread(() => {
                        pleaseWait.IsVisible = false;
                        pleaseWait.IsRunning = false;
                    });

                });

            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(() => {
                    DisplayAlert("Error", ex.Message, "OK");
                });
            }
            finally
            {

            }

        }

        async void ConnDisconn_Clicked(object sender, EventArgs e)
        {
            try
            {
                Device.BeginInvokeOnMainThread(() => {
                    pleaseWait.IsVisible = true;
                    pleaseWait.IsRunning = true;
                });

                var mpv = this.BindingContext as MainPageVM;
                mpv.IsConnected = false;
                var nw = mpv.SelectedNetwork;
                await mgr.ConnectAsync(nw.BssID,  nw.Name,nw.Password);
                //if (!bres)
                //{
                //    Device.BeginInvokeOnMainThread(() => {
                //        DisplayAlert("Error", "Can't connect", "OK");
                //    });
                //}
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

        async void RefreshNetworks_Clicked(object sender, EventArgs e)
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
