using System;
using System.Collections.Generic;
using System.Text;

namespace WiFiManager.Common
{
    public static class Constants
    {
        public const int NO_SIGNAL_LEVEL = 1000;
        public const int WIFI_CONFIG_PRIORITY = 10000;
        public const int GPS_TIMEOUT = 5;
        public const int GPS_ACCURACY = 1;
        public static readonly Encoding UNIVERSAL_ENCODING = Encoding.UTF8;
        public const int REFRESH_COORDS_WAIT_MS = 2000;

        public static readonly TimeSpan VIBRATE_DURATION = TimeSpan.FromSeconds(0.5);
    }
}
