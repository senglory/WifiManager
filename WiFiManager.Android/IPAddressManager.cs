using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using WiFiManager.Common;
using Xamarin.Forms;

[assembly: Dependency(typeof(WiFiManager.Droid.IPAddressManager))]
namespace WiFiManager.Droid
{
    class IPAddressManager : IIPAddressManager
    {
        public string GetIPAddress()
        {
            IPAddress[] adresses = Dns.GetHostAddresses(Dns.GetHostName());

            if (adresses != null && adresses[0] != null)
            {
                var s0 = "";
                var i = 0;
                do
                {
                    s0 = adresses[i++].ToString();
                } while (s0 == "127.0.0.1" && i < adresses.Length);
                return s0;
            }
            else
            {
                return null;
            }
        }
    }
}