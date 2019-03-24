using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using WiFiManager.Common;



[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace WiFiManager
{
	public partial class App : Application
	{
		public App (IWifiManagerOperations mgr)
		{
			InitializeComponent();

			MainPage = new MainPage(mgr);
		}
    }
}
