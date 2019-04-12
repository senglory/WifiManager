using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using Xamarin.Forms.PlatformConfiguration;
using Plugin.Connectivity;
using Plugin.Geolocator;
using Plugin.Permissions.Abstractions;

using WiFiManager.Common.BusinessObjects;
using WiFiManager.Common;



namespace WiFiManager
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : TabbedPage
    {
        IWifiManagerOperations mgr;

        public MainPage(IWifiManagerOperations mgr)
        {

            InitializeComponent();
            this.mgr = mgr;
            var vm = new MainPageVM(mgr);
            this.BindingContext = vm;

            CrossConnectivity.Current.ConnectivityChanged +=  (sender, args) =>
            {
                if (args.IsConnected)
                {
                    WifiConnectNotify();
                }
                else
                {
                    WifiDisConnectNotify();
                }
            };
        }


        public void WifiConnectNotify()
        {
            var mpv = this.BindingContext as MainPageVM;
            var current = Connectivity.NetworkAccess;

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

        public void RefreshAvailableNetworks(bool doVibrateUponFinish = false)
        {
            System.Diagnostics.Debug.WriteLine("RefreshAvailableNetworks - START");

            var mpv = this.BindingContext as MainPageVM;

            try
            {
                if (mpv.IsBusy)
                    return;
                mpv.DoRefreshNetworks();
                if (!string.IsNullOrEmpty(mpv.FirstFailedLineInCSV))
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        DisplayAlert("Alert", mpv.FirstFailedLineInCSV, "OK");
                    });
                }

                if (doVibrateUponFinish)
                {
                    var v = Plugin.Vibrate.CrossVibrate.Current;
                    v.Vibration(TimeSpan.FromSeconds(0.5));
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
                    lblLastError.Text = ex.Message;
                    DisplayAlert("Fatal", ex.Message, "OK");
                });
                throw;
            }

            System.Diagnostics.Debug.WriteLine("RefreshAvailableNetworks - END");
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
                lblLastError.Text = "";
            }
            catch (Exception ex)
            {
                lblLastError.Text = ex.Message;
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

            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<IpResult>(resultString);

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
                lblLastError.Text = ex.Message;
                mpv.IsBusy = false;
            }
            finally
            {
                mpv.IsBusy = false;
            }
        }

		void WebAdm_Clicked(object sender, EventArgs e){
			var mpv = this.BindingContext as MainPageVM;

			Device.OpenUri(new Uri("http://" + mpv.SelectedNetwork.InternalIP));
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
 

        void MenuItem_DeleteNetwork_Clicked(object sender, EventArgs e)
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

				var t = mpv.DoRefreshCoords();
				await t.ContinueWith(t3 =>
			   {
				  // notify about finishing via vibration
				  var v = Plugin.Vibrate.CrossVibrate.Current;
				   v.Vibration(TimeSpan.FromSeconds(0.5));
			   });
			}
			finally
            {
                mpv.IsBusy = false ;
            }
        }

        void MenuItem_Hunt_Clicked(object sender, EventArgs e)
        {
            try
            {
                var bo = sender as BindableObject;
                var mpv = this.BindingContext as MainPageVM;
                var dto = bo.BindingContext as WifiNetworkDto;

                mpv.WifiNetworksHunting.TryAdd(dto);
            }
            catch (Exception ex)
            {

                var qq = 555;
            }
        }

        private void MenuItem_SaveThis_Clicked(object sender, EventArgs e)
        {
            var bo = sender as BindableObject;
            var mpv = this.BindingContext as MainPageVM;
            var dto = bo.BindingContext as WifiNetworkDto;

            try
            {
                Task.Run(() => {
                    mpv.IsBusy = true;
                    mpv.DoSave(dto);
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

		async void SetDescrForAll_Clicked(object sender, EventArgs e)
		{
			var detailPage = new DescrForAllPage(this.BindingContext as MainPageVM);
			await Navigation.PushModalAsync(detailPage);
		}
	}
}
