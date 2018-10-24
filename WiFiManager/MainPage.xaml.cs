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
using System.Net.Http;
using Newtonsoft.Json;

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

                // get internal IP
                string ipaddress = DependencyService.Get<IIPAddressManager>().GetIPAddress();
                {
                    nw.InternalIP = ipaddress;
                }
                // get external IP (for the very first connection to the particular network)
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


        /// <summary>
        /// Taken from https://forums.xamarin.com/discussion/2260/get-current-ip-address
        /// </summary>
        /// <returns></returns>
        string getPublicIp()
        {
            // from https://www.c-sharpcorner.com/uploadfile/scottlysle/getting-an-external-ip-address-locally/
            // and https://wtfismyip.com/text
            //Encoding utf8 =  Encoding.UTF8;

            //WebClient webClient = new WebClient();

            //var ar = webClient.DownloadData("http://wtfismyip.com/text");
            //String externalIp = utf8.GetString(ar);

            var client = new HttpClient();
            var t1 = Task.Run(() =>  client.GetAsync("https://api.ipify.org/?format=json"));
            var response = t1.Result;
            var t2 = Task.Run(() => response.Content.ReadAsStringAsync());
             var resultString = t2.Result;

            var result = JsonConvert.DeserializeObject<IpResult>(resultString);

            var externalIp = result.Ip;

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
