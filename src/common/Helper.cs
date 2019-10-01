using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Monik.Common
{
    public class Helper
    {
        public static string Utf16ToUtf8(string utf16String)
        {
            // Get UTF16 bytes and convert UTF16 bytes to UTF8 bytes
            byte[] utf16Bytes = Encoding.Unicode.GetBytes(utf16String);
            byte[] utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, utf16Bytes);

            // Return UTF8 bytes as ANSI string
            return Encoding.UTF8.GetString(utf8Bytes);
        }

        public static string Utf8ToUtf16(string utf8String)
        {
            //UTF8 bytes
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(utf8String);

            //Converting to Unicode from UTF8 bytes
            byte[] unicodeBytes = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, utf8Bytes);

            return Encoding.Unicode.GetString(unicodeBytes);
        }

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0,
            DateTimeKind.Utc);

        public static DateTime FromMillisecondsSinceUnixEpoch(long milliseconds)
        {
            return UnixEpoch.AddMilliseconds(milliseconds);
        }
    } //end of class

    public static class DatetimeHelpers
    {
        public static DateTime RoundUp(this DateTime dt, TimeSpan d)
        {
            var modTicks = dt.Ticks % d.Ticks;
            var delta = modTicks != 0 ? d.Ticks - modTicks : 0;
            return new DateTime(dt.Ticks + delta, dt.Kind);
        }

        public static DateTime RoundDown(this DateTime dt, TimeSpan d)
        {
            var delta = dt.Ticks % d.Ticks;
            return new DateTime(dt.Ticks - delta, dt.Kind);
        }

        public static DateTime RoundToNearest(this DateTime dt, TimeSpan d)
        {
            var delta = dt.Ticks % d.Ticks;
            bool roundUp = delta > d.Ticks / 2;
            var offset = roundUp ? d.Ticks : 0;

            return new DateTime(dt.Ticks + offset - delta, dt.Kind);
        }
    }//end of class

    public class TimingHelper
    {
        private DateTime _from;
        private readonly IMonik _monik;

        private TimingHelper(IMonik aControl)
        {
            _from = DateTime.Now;
            _monik = aControl;
        }

        public static TimingHelper Create(IMonik monik)
        {
            return new TimingHelper(monik);
        }

        public void Begin()
        {
            _from = DateTime.Now;
        }

        public void EndAndLog([CallerMemberName] string sourceName = "")
        {
            var delta = DateTime.Now - _from;
            _monik.ApplicationInfo("{0} execution time: {1}ms", sourceName, delta.TotalMilliseconds);
        }

        public void EndAndMeasure(string metricName)
        {
            var delta = DateTime.Now - _from;
            _monik.Measure(metricName, AggregationType.Gauge, delta.TotalMilliseconds);
        }
    }//end of class
}
