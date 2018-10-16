using System;

using Xamarin.Forms;
using Plugin.Geolocator;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms.PlatformConfiguration;

using WiFiManager.Common.BusinessObjects;
using WiFiManager.Common;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Xamarin.Forms.Xaml;

namespace WiFiManager
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        IWifiOperations mgr;

        public MainPage(IWifiOperations mgr)
        {
            InitializeComponent();
            this.mgr = mgr;
            this.BindingContext = new MainPageVM(mgr);
            RefreshAvailableNetworks();
        }

        public void WifiConnectNotify()
        {
            var mpv = this.BindingContext as MainPageVM;
            mpv.IsConnected = true;
        }

        void RefreshAvailableNetworks()
        {
            try
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    pleaseWait.IsVisible = true;
                    pleaseWait.IsRunning = true;
                });
                var mpv = this.BindingContext as MainPageVM;
                mpv.DoRefreshNetworks();
            }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    pleaseWait.IsVisible = false;
                    pleaseWait.IsRunning = false;
                });
            }
        }

        async void RefreshCoords_Clicked(object sender, EventArgs e)
        {
            try
            {
                //Device.BeginInvokeOnMainThread(() => {
                //    pleaseWait.IsVisible = true;
                //    pleaseWait.IsRunning = true;
                //});

                var mpv = this.BindingContext as MainPageVM;
                var coords = mpv.SelectedNetwork.CoordsAndPower;
                //var t1 = mgr.ActualizeCoordsWifiNetworkAsync(mpv.SelectedNetwork);

                await mgr.ActualizeCoordsWifiNetworkAsync(mpv.SelectedNetwork);
                //t1.Start();
                //var t2 = t1.ContinueWith( (www) => {

                //});
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(() => {
                    DisplayAlert("Error", ex.Message, "OK");
                });
            }
            finally
            {
                //    Device.BeginInvokeOnMainThread(() => {
                //        pleaseWait.IsVisible = false;
                //        pleaseWait.IsRunning = false;
                //    });
            }

        }

        async void Conn_Clicked(object sender, EventArgs e)
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

        async void Disconn_Clicked(object sender, EventArgs e)
        {
            try
            {
                Device.BeginInvokeOnMainThread(() => {
                    pleaseWait.IsVisible = true;
                    pleaseWait.IsRunning = true;
                });

                var mpv = this.BindingContext as MainPageVM;
                mpv.IsConnected = false;
                await mgr.DisConnectAsync();
            }
            catch (Exception ex)
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
    }
}
