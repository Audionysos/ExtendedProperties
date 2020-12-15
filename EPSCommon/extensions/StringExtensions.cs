using System;
using System.Collections.Generic;
using System.Text;

namespace com.audionysos.general.extensions {
	public static class StringExtensions {

		//TODO: Make the string builder thread static
		private static StringBuilder sb = new StringBuilder();
		/// <summary>Reapeats string given number of times.</summary>
		/// <param name="pat">pattern string</param>
		/// <param name="times">how many times to repat.</param>
		public static string repeat(this string pat, int times) {
			sb.Clear();
			for (int i = 0; i < times; i++) sb.Append(pat);
			return sb.ToString();
		}

		/// <summary>Append given patter string, specified number of times.</summary>
		public static StringBuilder repeat(this StringBuilder sb, string pattern, int times) {
			for (int i = 0; i < times; i++) sb.Append(pattern);
			return sb;
		}

		/// <summary>
		/// Converts array of bytes to hex string.
		/// By "kgriffs" from stackoverflow.
		/// </summary>
		/// <returns></returns>
		public static string ToHex(this byte[] bytes) {
			char[] c = new char[bytes.Length * 2]; byte b;
			for (int bx = 0, cx = 0; bx < bytes.Length; ++bx, ++cx){
				b = ((byte)(bytes[bx] >> 4));
				c[cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);

				b = ((byte)(bytes[bx] & 0x0F));
				c[++cx] = (char)(b > 9 ? b + 0x37 + 0x20 : b + 0x30);
			}return new string(c);
		}

		/// <summary>
		/// Create bytes are from hex string.
		/// By "kgriffs" from stackoverflow.
		/// </summary>
		/// <returns></returns>
		public static byte[] HexToBytes(this string str) {
			if (str.Length == 0 || str.Length % 2 != 0) return new byte[0];
			byte[] buffer = new byte[str.Length / 2]; char c;
			for (int bx = 0, sx = 0; bx < buffer.Length; ++bx, ++sx) {
				// Convert first half of byte
				c = str[sx];
				buffer[bx] = (byte)((c > '9' ? (c > 'Z' ? (c - 'a' + 10) : (c - 'A' + 10)) : (c - '0')) << 4);
				// Convert second half of byte
				c = str[++sx];
				buffer[bx] |= (byte)(c > '9' ? (c > 'Z' ? (c - 'a' + 10) : (c - 'A' + 10)) : (c - '0'));
			}return buffer;
		}

	}
}
