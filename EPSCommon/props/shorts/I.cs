using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.audionysos.general.props.shorts {
	/// <summary>Provide convinient interface to create new initializers of common types.</summary>
	public class I {

		public static StaticInitializer nitializer(params EPInitializer<EPInfo>[] initializers) {
			return new StaticInitializer(initializers);
		}

		public static StaticInitializer nitializer(Action<EPInfo> a, params EPInitializer<EPInfo>[] initializers) {
			return new StaticInitializer(a, initializers);
		}

		public static InstanceInitializer nstanceInitializer(params EPInitializer<EP>[] initializers) {
			return new InstanceInitializer(initializers);
		}

		public static InstanceInitializer nstanceInitialize(Action<EP> a, params EPInitializer<EP>[] initializers) {
			return new InstanceInitializer(a, initializers);
		}

		public static ICounter nstanceCounter(params EPInitializer<EP>[] initializers) {
			return new ICounter(initializers);
		}

	}
}
