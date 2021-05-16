using NetCoro;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Test.Tests {
	public class ExceptionsTest {

		public static IEnumerable<Coro> behavioursExceptionsCoro(TaskExceptionBehaviour taskExceptionBehaviour) { 
			Console.WriteLine($"Coro start");
			yield return Coro.Await(() => throw new Exception($"In task exception"), taskExceptionBehaviour);
			Console.WriteLine($"Coro finish");
		}
		
		public static IEnumerable<Coro> assertExceptionCoro() { 
			Console.WriteLine($"Assert exception coro start");
			yield return Coro.Await(() => throw new Exception($"Task complete exception"), out Promise promise);
			promise.Assert();
			Console.WriteLine($"Assert exception coro finish"); 
		}

		public static IEnumerable<Coro> assertForwardExceptionCoro() { 
			Console.WriteLine($"Assert forward exception coro start");
			yield return Coro.Await(() => throw new Exception($"Task complete exception"), out Promise promise);
			promise.AssertForward();
			Console.WriteLine($"Assert forward exception coro finish"); 
		}


		private static void CoroFailturedHandler(Exception exception) { 
			Console.WriteLine($"Coro failtured handler called with exception: {exception.Message}");
		}

		private static void StaticWaitMethod(CoroExecutionController coroExecutionControl) { 
			try {
				CoroManager.Work(coroExecutionControl);
			} catch(Exception exception) {
				Console.WriteLine($"Exception catched:\n{exception.Message}");
				Console.WriteLine();
			}
		}

		private static void InstanceWaitMethod(CoroExecutionController coroExecutionControl) { 
			CoroManager coroManager = new CoroManager();
			try {
				coroManager.AddCoro(coroExecutionControl);
				coroManager.WaitAndStop();
				coroManager.Work();
			} catch(Exception exception) {
				Console.WriteLine($"Exception catched:\n{exception.Message}");
				Console.WriteLine();
			}
		}

		private static void behavioursTest(Action<CoroExecutionController> waitMethod, TaskExceptionBehaviour coroExceptionBehaviour, TaskExceptionBehaviour taskExceptionBehaviour) { 
			Console.WriteLine("Test with:");
			Console.WriteLine($"Coro exceptions behaviour: {coroExceptionBehaviour}");
			Console.WriteLine($"Task exceptions behaviour: {taskExceptionBehaviour}");
			Console.WriteLine();
			waitMethod(behavioursExceptionsCoro(taskExceptionBehaviour).Control(
				taskExceptionBehaviour : coroExceptionBehaviour, 
				failtured : CoroFailturedHandler)
			);

			Console.WriteLine("Test finished");
		}

		private static void Test(Action<CoroExecutionController> waitMethod) { 

			foreach(TaskExceptionBehaviour coroExceptionBehaviour in Enum.GetValues(typeof(TaskExceptionBehaviour))) { 
				foreach(TaskExceptionBehaviour taskExceptionBehaviour in Enum.GetValues(typeof(TaskExceptionBehaviour))) { 
					Console.WriteLine(new string('-', 80));
					behavioursTest(waitMethod, coroExceptionBehaviour, taskExceptionBehaviour);
					Console.WriteLine();
					Console.WriteLine();
				}
			}


			Console.WriteLine(new string('-', 80));
			waitMethod(assertExceptionCoro().Control(failtured : CoroFailturedHandler));


			Console.WriteLine(new string('-', 80));
			
			waitMethod(assertForwardExceptionCoro().Control(failtured : CoroFailturedHandler));
			
		}

		public static void Start() { 
			Console.WriteLine("Exceptions test...");
			Console.WriteLine();

			Console.WriteLine("Static wait method");
			Console.WriteLine(new string('-', 80));
			Console.WriteLine(new string('-', 80));
			Console.WriteLine(new string('-', 80));

			Test(StaticWaitMethod);

			Console.WriteLine("Instance wait method");
			Console.WriteLine(new string('-', 80));
			Console.WriteLine(new string('-', 80));
			Console.WriteLine(new string('-', 80));
			Test(InstanceWaitMethod);

		}
	}
}
