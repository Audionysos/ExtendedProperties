using System;
using System.Collections.Generic;
using System.Text;

namespace com.audionysos.general.extensions
{
    public static class ObjectExtensions {
		/// <summary>Repeats given action on this object givnen number of times.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="o"></param>
		/// <param name="n">Number of time the action should be called.</param>
		/// <param name="a">Action to be called, taking this object.</param>
		public static void times<T>(this T o, int n, Action<T> a) {
			for (int i = 0; i < n; i++) a(o);
		}

		/// <summary>Repeats given action on this object givnen number of times.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="o"></param>
		/// <param name="n">Number of time the action should be called.</param>
		/// <param name="a">Action to be called, taking this object. Int argument is current iterator value.</param>
		public static void times<T>(this T o, int n, Action<T, int> a) {
			for (int i = 0; i < n; i++) a(o, i);
		}

		/// <summary>Returns true if this object is any type of number (int, double, short etc.).</summary>
		public static bool isNumer<T>(this T n) {
			if (n is int) return true;
			if (n is double) return true;
			if (n is uint) return true;
			if (n is float) return true;
			if (n is long) return true;
			if (n is decimal) return true;
			if (n is short) return true;
			if (n is ushort) return true;
			return false;
		}
	}
}
