using System;
using com.audionysos.general.props;
using com.audionysos.general.props.shorts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using com.audionysos.general.extensions;
using com.audionysos.console;
using static com.audionysos.console.StaticConsole;

namespace UnitTestProject1 {
	[TestClass]
	public class BasicEPTests {
		[TestMethod]
		public void TestMethod1() {
			StaticConsole.console = new com.audionysos.console.Console();
			var to = new TestClassA();
			Assert.IsTrue(true);
		}
	}

	public class TestClassA {
		private static Type t = typeof(TestClassA);

		public static EPInfo min2Info = new EPInfo(t, nameof(minTwo));
		private static EP<double> minTwo = new EP<double>(10, min2Info);


		public static readonly EPInfo max2Info = new EPInfo(t, nameof(maxTwo));
		private static readonly EP<double> maxTwo = new EP<double>(20, max2Info);

		#region Double constrains
		private EP<double> _tp = new EP<double>(20, tpInfo);
		public readonly static EPInfo tpInfo = new EPInfo(
			t, nameof(testProperty), new StaticInitializer(),
			"Test Property",
			"Short info about this property",
			"Detailed description about property usage.",
			C.onstrains(
				C.FirstValid(
					C.Bounds(-50d, 40, false, false),
					C.Bounds(minTwo, maxTwo),
					new SnapTo<double>(-70, 70, 80, 90)
				)
			),
			I.nstanceCounter()
		);
		public double testProperty {
			get => _tp;
			set => _tp.value = value;
		}
		#endregion

		public static readonly EPInfo stInfo = new EPInfo(
			t, nameof(stringTest), new StaticInitializer(),
			"Test string property",
			null, null);
		public EP<string> stringTest = new EP<string>(null, stInfo);


		public TestClassA() {
			maxTwo.value = 300;

			stringTest.value = "abc";

			testProperty = -100;
			testProperty.times(600, (p, i) => {
				testProperty = i - 100; log("p:", testProperty, " (", i - 100, ")");
			});

			maxTwo.value = 200;
			log("p:", testProperty);
			minTwo.value = 220;
			log("p:", testProperty);
			log("instances count:", _tp.getInstanceIN<ICounter>().c);
		}

	}
}
