using NetCoro;
using System;
using System.Collections.Generic;
using System.Text;

namespace Test.Tests {
	public class RoundRobinTest {


		private static IEnumerable<Coro> printCoro(char c) {
			for(int index = 0; index < 30; index++) { 
				yield return Coro.DoNothing();
				Console.Write(c);
			}
		}

		public static void Start() { 
			Console.WriteLine("Round Robin test");
			CoroManager.Work(printCoro('c'), printCoro('a'), printCoro('t'), printCoro(' '));
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
		}

	}
}
