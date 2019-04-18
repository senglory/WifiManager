using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using WiFiManager.Common.BusinessObjects;
using WiFiManager.Common;



namespace WiFiManager
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DescrForAllPage : ContentPage
	{
		MainPageVM mpv;

		public DescrForAllPage (MainPageVM mpv)
		{
			InitializeComponent ();
			this.mpv = mpv;
		}

		async void OnOkButtonClicked(object sender, EventArgs args)
		{
			var descrTextForAll = descrText.Text;
			for (int i = 0; i < mpv.WifiNetworks.Count; i++)
			{
				if (mpv.UpdateOnlyEmptyInfo && string.IsNullOrWhiteSpace(mpv.WifiNetworks[i].Provider)
				||
					!mpv.UpdateOnlyEmptyInfo
					)
				{
					mpv.WifiNetworks[i].Provider = descrTextForAll;
				}
			}


			await Navigation.PopModalAsync();
		}

		async void OnDismissButtonClicked(object sender, EventArgs args)
		{

			await Navigation.PopModalAsync();
		}
	}
}