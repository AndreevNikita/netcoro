using NetCoro;
using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Tests {
	public static class NestedCorosTest {

		public static IEnumerable<Coro> coro3_1() {
			yield return ((Action)(() => Console.WriteLine("3.1.1"))).Await();
			yield return ((Action)(() => Console.WriteLine("3.1.2"))).Await();
		}

		public static IEnumerable<Coro> coro3_2() { 
			yield return ((Action)(() => Console.WriteLine("3.2.1"))).Await();

			yield return ((Action)(() => Console.WriteLine("3.2.2"))).Await();

		}

		public static IEnumerable<Coro> coro2() { 
			yield return ((Action)(() => Console.WriteLine("2.1"))).Await();
			yield return coro3_1().Await();
			yield return ((Action)(() => Console.WriteLine("2.2"))).Await();
			yield return coro3_2().Await();
			yield return ((Action)(() => Console.WriteLine("2.3"))).Await();
		}

		public static IEnumerable<Coro> coro1_1() {
			yield return ((Action)(() => Console.WriteLine("1.1.1"))).Await();
			yield return coro2().Await();
			yield return ((Action)(() => Console.WriteLine("1.1.2"))).Await();
		}

		public static IEnumerable<Coro> coro1_2() {
			yield return ((Action)(() => Console.WriteLine("1.2.1"))).Await();
			yield return coro2().Await();
			yield return ((Action)(() => Console.WriteLine("1.2.2"))).Await();
		}

		public static void Start() { 
			Console.WriteLine("Nested coros test");
			CoroManager.Work(coro1_1().Control(completed : () => Console.WriteLine("Coroutine 1.1 completed")), coro1_2().Control(completed : () => Console.WriteLine("Coroutine 1.2 completed")));

			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
		}

	}
}
