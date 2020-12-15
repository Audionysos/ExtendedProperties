using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace com.audionysos.general.extensions {
	public static class CollectionsExtensions {


		/// <summary>Read property value of given object specified by given record</summary>
		/// <returns></returns>
		//public static object readProperty(this object o, FEInterfaceRec r) {
		//	var p = r.pClass.GetProperty(r.address);
		//	return (p != null) ? p.GetValue(o) : r.pClass.GetField(r.address).GetValue(o);
		//}



		/// <summary>Toogle window state between normal and maximized</summary>
		/// <param name="w"></param>
		//public static void toogleState(this Window w) {
		//	if (w.WindowState != WindowState.Normal) w.WindowState = WindowState.Normal;
		//	else w.WindowState = WindowState.Maximized;
		//}

		/// <summary>Create string from given max count of items.</summary>
		/// <param name="arr"></param>
		/// <param name="max">Maximou number of items to create string from (starts from 0)</param>
		/// <returns></returns>
		public static string print(this IList arr, int max = 100) {
			var c = (max < arr.Count) ? max : arr.Count;
			if (c == 0) return "[list is empty]";
			if (c > 10) {
				var sb = new StringBuilder("[" + arr[0]);
				for (int i = 1; i < c; i++) sb.Append(", ").Append(arr[i]);
				if (arr.Count > max) sb.Append("...(max " + max.ToString() + " items printed)");
				return sb.Append("]").ToString();
			}
			var s = "[" + arr[0];
			for (int i = 1; i < c; i++) s += ", " + arr[i]?.ToString() ?? "null";
			if (arr.Count > max) s += "...(max " + max.ToString() + " items printed)";
			return s + "]";
		}

		/// <summary>Make <see cref="List{Object}"/> from this list. Optionaly add elements at beginning and end of new list.</summary>
		/// <param name="arr"></param>
		/// <param name="start">List of elements which will start new list.</param>
		/// <param name="end">List of elements added at end of new list.</param>
		/// <returns>New list of object.</returns>
		public static List<object> toObject(this IList arr, IList start = null, params object[] end) {
			var r = new List<object>(arr.Count + ((start != null) ? start.Count : 0) + ((end != null) ? end.Length : 0));
			if (start != null) {
				for (int i = 0; i < start.Count; i++) r.Add(start[i]);
			}
			for (int i = 0; i < arr.Count; i++) r.Add(arr[i]);
			if (end != null) {
				for (int i = 0; i < end.Length; i++) r.Add(end[i]);
			}
			return r;
		}

		/// <summary>Clear empty array items by adding <paramref name="replace"/> string to previous non-empty element each time an empty string is encountered.
		/// If first string is empty, result array will start with replace string as first item.</summary>
		/// <returns>New array without empty strings.</returns>
		public static string[] clearEmpty(this string[] arr, string replace = "") {
			var chi = new string[arr.Length]; int ci = -1;
			for (int i = 0; i < arr.Length; i++) {
				var s = arr[i];
				if (s.Length == 0) {
					if (i > 0) chi[ci] += replace;
					else chi[++ci] = replace;
				} else chi[++ci] = s;
			}
			Array.Resize(ref chi, ++ci);
			return chi;
		}

		/// <summary>Remove and return last element of the list. If list is empty, null is returned.</summary>
		public static object pop(this IList arr) {
			var i = arr.Count - 1; if (i < 0) return null;
			var e = arr[arr.Count - 1];
			arr.RemoveAt(arr.Count - 1);
			return e;
		}

		/// <summary>Fills list with given item object, starting from 0 and up to givnen count.
		/// If list is to short, new item will be added.</summary>
		public static void fill(this IList arr, int count, object item = null) {
			for (int i = 0; i < arr.Count; i++) arr[i] = item;
			while (arr.Count < count) arr.Add(item);
		}

		/// <summary>Set item at given index. If the list is shorted than given index, item will be add at the end of the list and not placed at given index.</summary>
		public static void setAt(this IList arr, int index, object item) {
			if (index < arr.Count) arr[index] = item;
			else arr.Add(item);
		}

		/// <summary>Set item at given index. If the list is shorted than given index, "empty" will added to missing idices lowe than target index.</summary>
		public static void placeAt(this IList arr, int index, object item, object empty = null) {
			var d = index - arr.Count; //missing items (difference)
			while (d >= 0) { arr.Add(empty); d--; }
			arr[index] = item;
		}
	}
}
