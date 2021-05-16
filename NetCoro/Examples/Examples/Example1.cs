using NetCoro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examples.Examples
{
	public class Example1
	{

		private static IEnumerable<Coro> printCoro(char c) {
			for(int index = 0; index < 30; index++) { 
				yield return Coro.DoNothing();
				Console.Write(c);
			}
		}

		public static void Start() { 
			CoroManager.Work(printCoro('c'), printCoro('a'), printCoro('t'), printCoro(' '));
		}

	}
}
