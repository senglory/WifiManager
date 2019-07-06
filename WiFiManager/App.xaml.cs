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
        public App (IWifiManagerOperations mgr)
        {
            InitializeComponent();

            //DisplayMetrics displayMetrics = new DisplayMetrics();
            //WindowManager.DefaultDisplay.GetRealMetrics(displayMetrics);
            // displayMetrics.WidthPixels
            // displayMetrics.HeightPixels
            var vm = this.Resources["MainPageVM"] as MainPageVM;
            MainPage = new MainPage(mgr, vm);
        }

        protected override void OnStart()
        {
            base.OnStart();

            CrossConnectivity.Current.ConnectivityChanged += (sender, args) => {
                if (!CrossConnectivity.Current.IsConnected)
                { }
//                    ShowFailureTimeOutAltert(ConstantMessages.INTERNET_CONNECTION_FAILURE);
            };
        }
    }
}
