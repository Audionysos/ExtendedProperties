using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.audionysos.general.props.shorts {
	/// <summary>Provides convient way to create new constrains</summary>
	public class C {
		/// <summary>Creates standard list of constrains where each constrain is applied one by one.
		/// Constrains can be tested multiple times while aleready applied costrains make input value applicable for other constrians.</summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static PConstrains onstrains(params PConstrain[] list) {
			var c = new PConstrains();
			for (int i = 0; i < list.Length; i++) c.Add(list[i]);
			return c;
		}

		public static CConstrian<T> onstrain<T>(ConstrainTester<T> t, ConstrainCorector<T> c) {
			return new CConstrian<T>(t, c);
		}

		/// <summary>Creates constrains list where first valid constains (which allows for setting input value) is used and all other are skipped.
		/// If no constrain can allow for given input and there are constrains which can correct it, costrain with hihest correctnes will be used.</summary>
		/// <param name="list"></param>
		/// <returns></returns>
		public static PConstrains FirstValid(params PConstrain[] list) {
			var c = new FirstValid();
			for (int i = 0; i < list.Length; i++) c.Add(list[i]);
			return c;
		}

		/// <summary>Creates bounds constrian which limits imput value to specified upper and lower limit.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="min">Mimimal value a property can take.</param>
		/// <param name="max">Maxmimal value a property can take.</param>
		/// <param name="includeMin">Specifies that a value can take given minimal value. If false, only smaller values are allowed.</param>
		/// <param name="includeMax">Specifies that a value can take given maximal value. If false, only greater values are allowed.</param>
		/// <returns></returns>
		public static PConstrain Bounds<T>(T min, T max, bool includeMin = true, bool includeMax = true) {
			return new Bounds<T>(min, max, includeMin, includeMax);
		}

	}
}
