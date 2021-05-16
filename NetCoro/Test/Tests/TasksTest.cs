using NetCoro;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test.Tests {
	public class TasksTest {

		private static Random rand = new Random();

		private static int completedTasks = 0;

		private static CoroExecutionController Control(IEnumerable<Coro> coro) => coro.Control(completed : () => completedTasks++);

		private static IEnumerable<Coro> InputCoro(CoroManager coroManager) { 
			while(true) { 
				yield return Coro.Await(Console.ReadLine, out Promise<string> promise);
				if(int.TryParse(promise.Result, out int intResult)) { 
					for(int index = 0; index < intResult; index++)
						coroManager.AddCoro(Control(TaskCoro($"Task-{index}")));
				} else if(promise.Result == "cc") {
					Console.WriteLine(completedTasks);
				} else if(promise.Result == "exit") { 
					yield return Coro.InterruptAll();
				} else {
					coroManager.AddCoro(Control(TaskCoro(promise.Result)));
				}
			}
		}

		private static IEnumerable<Coro> InnerTaskCoro(string name) {
			yield return Coro.Await(() => Console.WriteLine($"{name} inner"));
		}

		private static IEnumerable<Coro> TaskCoro(string name) { 
			yield return InnerTaskCoro(name).Await();
			//yield return Task.Delay(rand.Next(500, 2000)).Await();
			yield return rand.Next(500, 2000).Await();
			yield return Task.Run(() => Console.WriteLine($"{name} outer")).Await();
		}

		public static void Start() { 
			Console.WriteLine("Tasks test");
			Console.WriteLine("Enter the count of coroutines to add to CoroManager");
			Console.WriteLine("- \"cc\" to show completed coroutines count");
			Console.WriteLine("- \"exit\" to exit from test");

			CoroManager coroManager = new CoroManager();
			coroManager.AddCoro(InputCoro(coroManager));
			coroManager.Work();

			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();

		}
	}
}
