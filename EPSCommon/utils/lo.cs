using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.audionysos.general.utils {

	/// <summary>Class for displaying logs for debuggin purposes.</summary>
	public class lo {
		/// <summary>Current log buffer. If not null, dogs will be saved in the list instead printed thourgh Debug.WriteLine.</summary>
		public static List<String> logs = null;
		/// <summary>Store last "logs" buffer so it can be restored.</summary>
		public static List<String> prevLogs = null;

		public static bool on = true;

		/// <summary>Turns off the log - any call to lo.g() wont print anything until you call lo.gOn().</summary>
		public static void gOff() { on = false; }
		/// <summary>Turns on the log.</summary>
		public static void gOn() { on = true; }

		/// <summary>Logging series of objects for debugging purposes using Debug.WriteLine().</summary>
		/// <param name="par">List of object to be printed. Each objects is separeted by space.</param>
		public static void g(params object[] par) {
			if (!on) return;
			String s = "";
			foreach (var o in par) s += (o?.ToString() ?? "null") + " ";
			//foreach (var o in par) s += o.ToString() + " ";
			if (logs != null) logs.Add(s);
			else Debug.WriteLine(s);
		}

		/// <summary>Do nothing. This method is just for quick temporar remove of log traces.</summary>
		/// <param name="par"></param>
		public static void gX(params object[] par) { }

		/// <summary>Creates new logs buffer or restore previous, if "logs" field is currently null or do nothing</summary>
		public static List<String> openBuffer() {
			if (logs != null) return logs;
			logs = prevLogs ?? new List<string>();
			return logs;
		}

		internal static void closeBuffer() {
			if (logs != null) prevLogs = logs; logs = null;
		}
	}

}
