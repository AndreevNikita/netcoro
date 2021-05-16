using NetCoro;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Test.Tests {
	public class WaitHandleTest {

		private static Dictionary<string, ManualResetEvent> manualResetEvents = new Dictionary<string, ManualResetEvent>();

		public static IEnumerable<Coro> InputCoro(CoroManager coroManager) { 

			while(true) { 
				yield return Coro.Await(Console.ReadLine, out Promise<string> promise);
				
				if(promise.Result == "exit") {
					yield return Coro.InterruptAll();
				} else {
					if(manualResetEvents.Remove(promise.Result, out var mre)) {
						mre.Set();
					} else {
						manualResetEvents.Add(promise.Result, new ManualResetEvent(false));
						coroManager.AddCoro(WaitHandleCoro(promise.Result));
					}
				}
			}

		}

		public static IEnumerable<Coro> WaitHandleCoro(string name) { 
			Console.WriteLine($"Start to wait \"{name}\"");
			yield return manualResetEvents[name].Await();
			Console.WriteLine($"\"{name}\" completed");
		}

		public static void Start() { 
			manualResetEvents.Clear();
			Console.WriteLine("WaitHandle await test");
			Console.WriteLine("Enter a name of ManualResetEvent to create coro-waiter or unlock this if exists");
			Console.WriteLine("Enter \"exit\" to interrupt all coros and exit");

			
			CoroManager coroManager = new CoroManager();
			coroManager.AddCoro(InputCoro(coroManager));
			coroManager.Work();

			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
		}

	}
}
