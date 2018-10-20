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
using System.Net;
using System.Text;

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
            var vm = new MainPageVM(mgr);
            this.BindingContext = vm;
            RefreshAvailableNetworks();
        }

        public void WifiConnectNotify()
        {
            var mpv = this.BindingContext as MainPageVM;
            mpv.IsConnected = true;
        }
        public void WifiDisConnectNotify()
        {
            var mpv = this.BindingContext as MainPageVM;
            mpv.IsConnected = false ;
        }

        void RefreshAvailableNetworks()
        {
            var mpv = this.BindingContext as MainPageVM;
            try
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    //pleaseWait.IsVisible = true;
                    //pleaseWait.IsRunning = true;
                    mpv.IsBusy = true;
                });
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
                    //pleaseWait.IsVisible = false ;
                    //pleaseWait.IsRunning = false ;
                    mpv.IsBusy = false;
                });
            }
        }


        void RefreshCoords_Clicked(object sender, EventArgs e)
        {
            try
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    pleaseWait.IsVisible = true;
                    pleaseWait.IsRunning = true;
                });

                var mpv = this.BindingContext as MainPageVM;
                var coords = mpv.SelectedNetwork.CoordsAndPower;
                //var t1 = mgr.ActualizeCoordsWifiNetworkAsync(mpv.SelectedNetwork);

                //await mgr.ActualizeCoordsWifiNetworkAsync(mpv.SelectedNetwork);
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
                var wi = await mgr.ConnectAsync(nw.BssID,  nw.Name,nw.Password);

                // write data about first connection only if it has not been written yet 
                if (nw.FirstConnectWhen == null)
                {
                    nw.FirstConnectWhen = DateTime.Now;
                }
                if (string.IsNullOrEmpty(nw.FirstConnectMac))
                {
                    nw.FirstConnectMac = wi.MacAddress;
                }
                if (string .IsNullOrEmpty ( nw.FirstConnectPublicIP ))
                {
                    nw.FirstConnectPublicIP = getPublicIp();
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

         string getPublicIp()
        {
            Encoding utf8 =  Encoding.UTF8;

            WebClient webClient = new WebClient();

            String externalIp = utf8.GetString(webClient.DownloadData(

            "http://wtfismyip.com/text"));


            return externalIp;
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

        private void SaveCommand_Clicked(object sender, EventArgs e)
        {
            try
            {
                Device.BeginInvokeOnMainThread(() => {
                    pleaseWait.IsVisible = true;
                    pleaseWait.IsRunning = true;
                });

                var mpv = this.BindingContext as MainPageVM;
                mpv.DoSave();
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

        private void RefreshNetworks_Clicked(object sender, EventArgs e)
        {
            RefreshAvailableNetworks();
        }
    }
}
