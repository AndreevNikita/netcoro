using NetCoro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examples.Examples {


	public class Example3 {

		public enum CoroAction { SuccessCompletion, Interrupt, ThrowException }

		private static IEnumerable<Coro> TestCoro(CoroAction action) { 

			switch(action) { 
				case CoroAction.SuccessCompletion:
					yield break;
				case CoroAction.Interrupt:
					yield return Coro.InterruptCurrent();
					yield break;
				case CoroAction.ThrowException:
					throw new Exception("My exception");
			}
		}

		public static void Start() {
			//Create a CoroExecutionController
			foreach(CoroAction action in typeof(CoroAction).GetEnumValues()) {
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.WriteLine($"{action.ToString()} run");
				Console.ForegroundColor = ConsoleColor.White;
				Console.WriteLine(new string('-', 80));

				//Create a CoroExecutionController for new TestCoro(action)
				CoroManager.Work(TestCoro(action).Control(
					finished: (c) => Console.WriteLine("TestCoro finished!"),
					completed: () => Console.WriteLine("TestCoro completed!"),
					canceled: () => Console.WriteLine("TestCoro canceled!"),
					failtured: (e) => Console.WriteLine($"Exception occured ({e.Message})!")
				));

				Console.WriteLine();
				Console.WriteLine();
				Console.WriteLine();
			}
		}

	}

}
