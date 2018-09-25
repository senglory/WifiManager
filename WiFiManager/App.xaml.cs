using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using WiFiManager.Common;
using Plugin.Connectivity;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace WiFiManager
{
	public partial class App : Application
	{
		public App (IWifiOperations mgr)
		{
			InitializeComponent();

			MainPage = new MainPage(mgr);
		}

		protected override void OnStart ()
		{
            CrossConnectivity.Current.ConnectivityChanged += Current_ConnectivityChanged;
        }

        protected void Current_ConnectivityChanged(object sender, Plugin.Connectivity.Abstractions.ConnectivityChangedEventArgs e)
        {
            if (e.IsConnected)
            {
                (MainPage as MainPage).WifiConnectNotify();
            }
        }

        protected override void OnSleep ()
		{
            CrossConnectivity.Current.ConnectivityChanged -= Current_ConnectivityChanged;
        }

        protected override void OnResume ()
		{
            CrossConnectivity.Current.ConnectivityChanged += Current_ConnectivityChanged;
        }
    }
}
