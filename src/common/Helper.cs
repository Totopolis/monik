using System;
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
			return Encoding.Default.GetString(utf8Bytes);
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
}
