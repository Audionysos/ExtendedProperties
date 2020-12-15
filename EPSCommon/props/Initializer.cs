using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.audionysos.general.props {

	/// <summary>EPInitializer is designed for use with <see cref="EP{T}"/> but it's independend of it.
	/// Initializer task is trivial - collect child initialzers, run any own code when called and call other intializers to do the same.
	/// Additionaly base initializer can run some custom action spefied in construcor.
	/// Iheritors suppose to override <see cref="initialize(T)"/> method, as base class only action given in constructor and call its children initializers.</summary>
	/// <typeparam name="T"></typeparam>
	public abstract class EPInitializer<T> : IEnumerable {
		/// <summary>List of children initializers.</summary>
		protected List<EPInitializer<T>> list = new List<EPInitializer<T>>();
		/// <summary>Action to be performed when <see cref="initialize(T)"/> is called.</summary>
		protected Action<T> action;

		public EPInitializer(params EPInitializer<T>[] l) {
			list.AddRange(l);
		}

		/// <summary>Create new initializer with some custom action that will be invoked before other children initializers.</summary>
		/// <param name="a">Action to be invoked first</param>
		/// <param name="l">List of child initializers to invoked when <see cref="initialize(T)"/> method is called.</param>
		public EPInitializer(Action<T> a, params EPInitializer<T>[] l) {
			list.AddRange(l); action = a;
		}

		/// <summary>Run this initializer and it's children initializers.</summary>
		public virtual void initialize(T eP) {
			action?.Invoke(eP);
			for (int i = 0; i < list.Count; i++) list[i].initialize(eP);
		}

		/// <summary>Searches for initializer of specified type.
		/// If no initializer is found, null is returned.</summary>
		/// <typeparam name="SI"></typeparam>
		public SI getInitializer<SI>() where SI : EPInitializer<T> {
			if (this is SI) return this as SI;
			for (int i = 0; i < list.Count; i++)
				if (list[i] is SI) return (SI)list[i];
			for (int i = 0; i < list.Count; i++) {
				var r = list[i].getInitializer<SI>();
				if (r) return r;}
			return null;
		}

		public void Add(EPInitializer<T> initializer) => list.Add(initializer);
		public IEnumerator GetEnumerator() => list.GetEnumerator();
		public int Count => list.Count;
		public EPInitializer<T> this[int i] { get => list[i]; }
		public static implicit operator bool(EPInitializer<T> i) => i!=null;
	}

	/// <summary>Do nothin container for grouping other initializers.</summary>
	public class InstanceInitializer: EPInitializer<EP> {
		public InstanceInitializer(params EPInitializer<EP>[] l):base(l) {}
		public InstanceInitializer(Action<EP> a, params EPInitializer<EP>[] l):base(a,l) {}
	}

	/// <summary>Do nothin container for grouping other initializers.</summary>
	public class StaticInitializer : EPInitializer<EPInfo> {
		public StaticInitializer(params EPInitializer<EPInfo>[] l) : base(l) { }
		public StaticInitializer(Action<EPInfo> a, params EPInitializer<EPInfo>[] l) : base(a, l) { }
	}

	/// <summary>This initalizer counts number of instances created with it's parent <see cref="EPInfo"/> object.</summary>
	public class ICounter : InstanceInitializer {
		public int c { private set; get; }
		public override void initialize(EP eP) {
			c++;
			base.initialize(eP);
		}

		public ICounter(params EPInitializer<EP>[] l) : base(l) { }
		public ICounter(Action<EP> a, params EPInitializer<EP>[] l) : base(a, l) { }
	}

}
