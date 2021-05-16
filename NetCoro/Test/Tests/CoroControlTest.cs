using NetCoro;
using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Tests {
	public class CoroControlTest {

		private static IEnumerable<Coro> coro1() { 
			yield return Coro.Await(() => Console.WriteLine("Coro 1 start"));
			yield return 2000.Await();
			yield return Coro.Await(() => Console.WriteLine("Coro 1 finish"));
		}

		private static IEnumerable<Coro> coro2() { 
			yield return Coro.Await(() => Console.WriteLine("Coro 2 start"));
			yield return 5000.Await();
			yield return Coro.Await(() => Console.WriteLine("Coro 2 finish"));
		}

		private static IEnumerable<Coro> exceptionCoro() { 
			yield return 3000.Await();
			throw new Exception("Exception in exception coro");
		}


		public static void Start() { 
			CoroManager coroManager = new CoroManager();
			coroManager.AddCoro(coro1().Control(completed : () => Console.WriteLine("Coro 1 completed")));
			coroManager.AddCoro(coro2());
			coroManager.AddCoro(exceptionCoro().Control(
				failtured : (Exception e) => Console.WriteLine($"Exception occured:\n{e.Message}"), 
				finished : (coroController) => Console.WriteLine($"Exception coro finished")
			));
			coroManager.WaitAndStop();
			coroManager.Work();

			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
		}

	}
}
