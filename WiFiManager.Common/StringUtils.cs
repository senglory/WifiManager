using System;
using System.Collections.Generic;
using System.Text;

namespace WiFiManager.Common
{
	public static class StringUtils
	{
		public static string ReplaceNullSafe(this string src, string whatToFind, string replacement)
		{
			if (string.IsNullOrEmpty(src))
				return src;
			return src.Replace(whatToFind, replacement);
		}

		public static bool StartsWithNullSafe(this string src, string whatToFind)
		{
			if (string.IsNullOrEmpty(src))
				return false;
			return src.StartsWith(whatToFind, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}
