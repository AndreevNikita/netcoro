using NetCoro;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Test.Tests {
	public class DelayTest {


		private static IEnumerable<Coro> printCoro(char c, int delay) {
			for(int index = 0; index < 30; index++) { 
				yield return delay.Await();
				Console.Write(c);
			}
		}

		public static void Start() { 
			Console.WriteLine("Non equal delay test");
			CoroManager.Work(printCoro('a', 10), printCoro('b', 20), printCoro('c', 40), printCoro('d', 80));

			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
		}

	}
}
