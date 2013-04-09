using System;
using System.Globalization;

namespace SQLServerBackupTool.Web.Lib.Extensions
{
    public static class NumberExtensions
    {
        public static string[] SizeUnits = new[] { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

        public static bool IsBetween(this string val, int down, int up)
        {
            int realVal;

            return int.TryParse(val, out realVal) && IsBetween(realVal, down, up);
        }

        public static bool IsBetween(this int val, int down, int up)
        {
            return val >= down && val <= up;
        }

        public static string BytesToString(this long byteCount, int decimals = 1)
        {
            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num   = Math.Round(bytes / Math.Pow(1024, place), decimals);

            return string.Format(
                "{0} {1}",
                (Math.Sign(byteCount) * num).ToString(CultureInfo.InvariantCulture),
                SizeUnits[place]
            );
        }
    }
}