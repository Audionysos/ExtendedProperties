using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using com.audionysos.general.extensions;

namespace com.audionysos.general.props {

	/// <summary>Base class for property constrains sets. This simply collects any propery constrains and execute them on by one when <see cref="apply{T}(T, ref T)"/> is called.</summary>
	public class PConstrains : PConstrain, IEnumerable {

		/// <summary>List of all specfied constrians.</summary>
		protected List<PConstrain> list = new List<PConstrain>();
		/// <summary>List of pending constrian waithing to be tested again.</summary>
		internal readonly List<PConstrain> pending = new List<PConstrain>();

		public void Add(PConstrain c) {
			list.Add(c);
			c.CHANGED += onConstrianChanged;
		}

		private void onConstrianChanged(PConstrainEvent e) => fireChangeEvent();

		public int Count => list.Count;

		/// <summary>Apply the value to given property considering all spefied constrains.</summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="value"></param>
		/// <param name="property"></param>
		/// <returns></returns>
		public virtual PConstrianResults apply<T>(T value, ref T property) {
			var r = new PConstrianResults(list.Count);
			for (int i = 0; i < list.Count; i++) { var c = list[i];
				var tr = c.test(value);
				if (tr.status == PCostrainStatus.PASSED ||
					tr.status == PCostrainStatus.CORRECTED)
					c.correct(value, ref property);
				else pending.Add(c);
			}
			var pc = 1;
			do {
				pc = pending.Count;
				for (int i = 0; i < pending.Count; i++) { var c = pending[i];
					var tr = c.test(value);
					if (tr.status == PCostrainStatus.REFUSED) continue;
					c.correct(value, ref property);
					pending.RemoveAt(i); i--;
				}
			} while (pc > 0 && pc != pending.Count);

			return r;
		}

		public override PConstrianResult test(object newValue) {
			var r = new PConstrianResults(list.Count);
			for (int i = 0; i < list.Count; i++) { var c = list[i];
				r.Add(c.test(newValue));
			} return r;
		}

		public override PConstrianResult correct<T>(T value, ref T current) {
			return apply(value, ref current);
		}

		public IEnumerator GetEnumerator() {
			return list.GetEnumerator();
		}

		public PConstrain this[int i] { get => list[i]; }
	}

	/// <summary>Applies first valid constrian. All other constrains are ignored.
	/// If none of constrains is valid but input value can be corrected, the one with greates correctnes value will be choosed.</summary>
	public class FirstValid : PConstrains {

		public override PConstrianResults apply<T>(T value, ref T property) {
			var r = new PConstrianResults(list.Count);
			var bc = int.MinValue; var bi = 0; //best correctnes and it's index
			for (int i = 0; i < list.Count; i++) { var c = list[i];
				var tr = c.test(value); r.Add(tr);
				if (tr.status == PCostrainStatus.PASSED) { bi = i; break; }
				if (tr.status == PCostrainStatus.CORRECTED &&
					tr.correctnes > bc) { bc = tr.correctnes; bi = i; }
			}
			list[bi].correct(value, ref property);
			return r;
		}
	}

	public abstract class PConstrain {
		public event PConstrainEventHandler CHANGED;

		public PConstrain() {}

		/// <summary>Dispatches <see cref="CHANGED"/> event so that extended propery can reegzamine constrains.</summary>
		protected void fireChangeEvent() => CHANGED?.Invoke(new PConstrainEvent(this));

		/// <summary>Correct and set new value for given current value.</summary>
		/// <param name="value">New value to be set.</param>
		/// <param name="current">Current value.</param>
		/// <returns>Result of correction.</returns>
		public abstract PConstrianResult correct<T>(T value, ref T current);

		/// <summary>Test if given value can be set to the property.</summary>
		/// <param name="newValue"></param>
		/// <returns></returns>
		public abstract PConstrianResult test(object newValue);

		public static implicit operator bool(PConstrain c) => c != null;
	}

	public delegate PConstrianResult ConstrainCorector<CT>(CT value, ref CT current);
	public delegate PConstrianResult ConstrainTester<CT>(object newValue);

	/// <summary>Custom constrian class.</summary>
	public class CConstrian<T> : PConstrain {

		ConstrainTester<T> tester;
		ConstrainCorector<T> corrector;

		public CConstrian(ConstrainTester<T> tester, ConstrainCorector<T> corrector) {
			this.tester = tester;
			this.corrector = corrector;
		}

		public override PConstrianResult correct<IT>(IT value, ref IT current) {
			var c = (T)(object)current;
			var r = corrector?.Invoke((T)(object)value, ref c);
			current = (IT)(object)c; return r;
		}

		public override PConstrianResult test(object newValue) {
			return tester.Invoke(newValue);
		}
	}

	public delegate void PConstrainEventHandler(PConstrainEvent e);
	public class PConstrainEvent{
		public readonly PConstrain c;
		public PConstrainEvent(PConstrain c) => this.c = c;
	}


	/// <summary>Constrain the value to specified bounds.
	/// Any type which contains (less than, greater than, equal and minus operator) can be used as generic type.
	/// If type is wrong, runtime error will be trown.</summary>
	/// <typeparam name="T"></typeparam>
	public class Bounds<T> : PConstrain {
		public readonly T min;
		public readonly T max;
		public readonly bool includeMin;
		public readonly bool includeMax;
		public int sub = 1;
		public int add = 1;

		/// <summary>Creates new bounds constrain</summary>
		/// <param name="min">Minmial value that can be set.</param>
		/// <param name="max">Maximal value that can be set.</param>
		/// <param name="includeMin">Specifies that a value can take given minimal value. If false, only smaller values are allowed.</param>
		/// <param name="includeMax">Specifies that a value can take given maximal value. If false, only greater values are allowed.</param>
		public Bounds(T min, T max, bool includeMin = true, bool includeMax = true) {
			var t = typeof(T);
			if (t.IsSubclassOf(typeof(EP))) {
				var d = (min as EP).createEventHandler(this, nameof(onBoundsChanged));

				//var pType = t.GenericTypeArguments[0];
				//var mi = this.GetType().GetMethod(nameof(onBoundsChanged));
				//mi = mi.MakeGenericMethod(pType);
				//var d = Delegate.CreateDelegate((min as EP).eventType, this, mi);
				t = (min as EP).type;
				(min as EP).listenEvent(d, EPEvents.CHANGED);
				(max as EP).listenEvent(d, EPEvents.CHANGED);
			}
			if (!t.hasOperators("<", ">", "==")) throw new Exception("Type of value don't support reguired opertors (<,>,==).");
			if ((!includeMax || !includeMin) && !min.isNumer()) throw new Exception("min and max values can be only excluded from range if value type is numeric type.");
			this.min = min;
			this.max = max;
			this.includeMin = includeMin;
			this.includeMax = includeMax;
		}

		public void onBoundsChanged<ET>(EPEvent<ET> e){
			fireChangeEvent();
		}

		private PConstrianResult inRange = new PConstrianResult(
			PCostrainStatus.PASSED, null, null);
		private PConstrianResult toBig = new PConstrianResult(
			PCostrainStatus.CORRECTED, null, "Given value is to big.");
		private PConstrianResult toSmall = new PConstrianResult(
			PCostrainStatus.CORRECTED, null, "Given value is to small.");
		private PConstrianResult result;

		public override PConstrianResult test(object nv) {
			var xd = (dynamic)max - (dynamic)nv;
			var maxOk = xd > 0 || (includeMax && xd == 0);
			if (!maxOk) { result = toBig.setCore((int)xd); return toBig; }

			var nd = (dynamic)nv - (dynamic)min;
			var minOk = nd > 0 || (includeMin && nd == 0) ;
			if (!minOk) { result = toSmall.setCore((int)nd); return toSmall; }

			result = inRange; return inRange;
		}

		//public override PConstrianResult test(object nv) {
		//	var md = (dynamic)max - (dynamic)nv;
		//	var maxOk = ((dynamic)nv < (dynamic)max) ||
		//		(includeMax && ((dynamic)nv == (dynamic)max));
		//	if (!maxOk) { result = toBig; return toBig; }

		//	var minOk = ((dynamic)nv > (dynamic)min) ||
		//		(includeMin && ((dynamic)nv == (dynamic)min));
		//	if (!minOk) { result = toSmall; return toSmall; }

		//	result = inRange; return inRange;
		//}

		public override PConstrianResult correct<M>(M value, ref M c) {
			if (!result) test(value);
			if (result == inRange) c = value;
			else if (result == toBig) c = (includeMax) ? max : Math.Round((dynamic)max-sub);
			else if (result == toSmall) c = (includeMin) ? min : Math.Round((dynamic)min+add);
			var r = result; result = null; return r;
		}

		private double nextSmaller(double v) {
			var c = double.Epsilon;
			var r = v - c;
			while(r == v) {
				c += double.Epsilon;
				r = v - c;
			}return r;
		}

		public override string ToString() {
			var s = "Bounds";
			s += ((includeMin) ? "<" : "(") + min.ToString() + ", "; 
			s += max.ToString() + ((includeMax) ? ">" : ")");
			return s;
		}

	}

	/// <summary>Snaps input closest value form specified list</summary>
	/// <typeparam name="T"></typeparam>
	public class SnapTo<T> : PConstrain {
		private T[] list;

		public SnapTo(params T[] list) {
			this.list = list;
		}

		
		public override PConstrianResult correct<T1>(T1 value, ref T1 current) {
			if (!result) test(value);
			current = (T1)(object)list[ci];
			var r = result; result = null;
			return r;
		}

		/// <summary>Closest index.</summary>
		private int ci = -1;
		private PConstrianResult result;
		public override PConstrianResult test(object newValue) {
			ci = -1; var bc = int.MinValue; result = null;
			for (int i = 0; i < list.Length; i++) { var av = list[i];
				if (newValue.Equals(av)) {
					result = new PConstrianResult(PCostrainStatus.PASSED);
					bc = int.MaxValue;
					ci = i; break;
				} else {
					var c = -Math.Abs((double)newValue - (double)(object)av);
					if (bc < c) {bc = (int)c; ci = i;}
				}
			}if (!result) result = new PConstrianResult(PCostrainStatus.CORRECTED);
			result.setCore(bc);
			return result;
		}
	}

	public class PConstrianResult {
		internal int _c;
		public int correctnes => _c;
		public PConstrianResult setCore(int c) { _c = c; return this; }

		protected PCostrainStatus _status;
		/// <summary>Status of constains</summary>
		public PCostrainStatus status => _status;

		protected object _cause;
		/// <summary>The object or property which coused correction of refuse of input value.</summary>
		public object cause => _cause;

		protected string _info;
		public string info => _info;

		public PConstrianResult(PCostrainStatus status, PConstrianResult cause = null, string info = null) {
			this._status = status;
			this._cause = cause;
			this._info = info;
		}

		public override string ToString() {
			return $@"{_status} ({_c}) {_info}";
		}

		public static implicit operator bool(PConstrianResult r) => r!=null;
	}

	/// <summary>Contains a list of nested constrains results.
	/// Status is set to reflect most restricted status of inner result i.e. if any of results is <see cref="PCostrainStatus.REFUSED"/> status of this collection will also be refused.</summary>
	public class PConstrianResults : PConstrianResult, IEnumerable{
		private List<PConstrianResult> all;

		public PConstrianResults(int capcity)
			:base(PCostrainStatus.PASSED, null, "Initalized") {
			all = new List<PConstrianResult>(capcity);
		}

		public void Add(PConstrianResult r) {
			all.Add(r);
			if (//_status != PCostrainStatus.REFUSED &&
				r.status == PCostrainStatus.CORRECTED)
				{ _status = r.status; _info = "Corrected by inner constrian."; }
			else if (r.status == PCostrainStatus.REFUSED)
				{ _status = r.status; _info = "Refused by inner constrian."; } 
		}
		public IEnumerator GetEnumerator() => all.GetEnumerator();

		public static implicit operator bool(PConstrianResults r) => r != null;
	}

	public enum PCostrainStatus {
		/// <summary>Constrain test has passed successfully and given input value is correct.</summary>
		PASSED,
		/// <summary>Constain not allow given input but it can be corrected to allowed value.</summary>
		CORRECTED,
		/// <summary>Constrain not allow for given input and cannot correct it.</summary>
		REFUSED
	}
}
