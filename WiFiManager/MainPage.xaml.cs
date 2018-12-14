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
using System.IO;

namespace WiFiManager
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : TabbedPage
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

            var nw = mpv.SelectedNetwork;
            if (nw == null)
                return;
            // get internal IP
            string ipaddress = DependencyService.Get<IIPAddressManager>().GetIPAddress();
            {
                nw.InternalIP = ipaddress;
            }
            // get external IP (for the very first connection to the particular network)
            if (string.IsNullOrEmpty(nw.FirstConnectPublicIP))
            {
                nw.FirstConnectPublicIP = getPublicIp();
            }
            // stop 'please wait'
            mpv.IsBusy = false;
        }

        public void WifiDisConnectNotify()
        {
            var mpv = this.BindingContext as MainPageVM;
            mpv.IsConnected = false ;
            // stop 'please wait'
            mpv.IsBusy =false ;
        }

        void RefreshAvailableNetworks()
        {
            var mpv = this.BindingContext as MainPageVM;

            try
            {
                mpv.IsBusy = true;
                mpv.DoRefreshNetworks();
                if (!string.IsNullOrEmpty(mpv.FirstFailedLineInCSV))
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        DisplayAlert("Alert", mpv.FirstFailedLineInCSV, "OK");
                    });
                }
            }
            catch (InvalidDataException ex)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    DisplayAlert("Alert", ex.Message, "OK");
                });
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    DisplayAlert("Fatal", ex.Message, "OK");
                });
                throw;
            }
            finally
            {
                mpv.IsBusy = false;
            }
        }


        async void MenuItem2_RefreshCoords_Clicked(object sender, EventArgs e)
        {
            var bo = sender as BindableObject;
            var mpv = this.BindingContext as MainPageVM;
            var n = bo.BindingContext as WifiNetworkDto;

            try
            {
                mpv.IsBusy = true;
                await mgr.ActualizeCoordsWifiNetworkAsync(n);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                mpv.IsBusy = false;
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


        async void Conn_Clicked(object sender, EventArgs e)
        {
            var mpv = this.BindingContext as MainPageVM;

            try
            {
                mpv.IsBusy = true;
                mpv.IsConnected = false;
                var nw = mpv.SelectedNetwork;
                var wi = await mgr.ConnectAsync(nw);

                nw.TryUpdateFirstConnectionInfo(wi);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
                mpv.IsBusy = false;
            }
            finally
            {
                mpv.IsBusy = false;
            }
        }

        async void Disconn_Clicked(object sender, EventArgs e)
        {
            var mpv = this.BindingContext as MainPageVM;

            try
            {
                mpv.IsBusy = true;
                mpv.IsConnected = false;
                await mgr.DisConnectAsync();
            }
            catch (Exception ex)
            {
//                Device.BeginInvokeOnMainThread(() => {
                    await DisplayAlert("Error", ex.Message, "OK");
//                });
            }
            finally
            {
                mpv.IsBusy = false;
            }
        }

        void SaveCommand_Clicked(object sender, EventArgs e)
        {
            var mpv = this.BindingContext as MainPageVM;
            try
            {

                Task.Run(() => {
                    mpv.IsBusy = true;
                    mpv.DoSave();
                }
                );
            }
            catch (Exception ex)
            {
                Device.BeginInvokeOnMainThread(() => {
                    DisplayAlert("Error", ex.Message, "OK");
                });
            }
            finally
            {
                mpv.IsBusy = false;
            }
        }

        async void RefreshNetworks_Clicked(object sender, EventArgs e)
        {
            await Task.Run(() => RefreshAvailableNetworks());
        }
 

        private void MenuItem_DeleteNetwork_Clicked(object sender, EventArgs e)
        {
            var bo = sender as BindableObject;
            var mpv = this.BindingContext as MainPageVM;
            var n = bo.BindingContext as WifiNetworkDto;
            mpv.WifiNetworks.Remove(n);
        }

        async void RefreshCoords_Clicked(object sender, EventArgs e)
        {
            var mpv = this.BindingContext as MainPageVM;

            try
            {
                mpv.IsBusy = true;

                await mpv.DoRefreshCoords();
            }
            finally
            {
                mpv.IsBusy = false ;
            }
        }

        private void MenuItem_Hunt_Clicked(object sender, EventArgs e)
        {
            var bo = sender as BindableObject;
            var mpv = this.BindingContext as MainPageVM;
            var n = bo.BindingContext as WifiNetworkDto;

            mpv.WifiNetworksHunting.Add(n);
        }
    }
}
