using NetCoro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Examples.Examples
{
	public class Example2 {
		private static CoroManager coroManager;
		private static int lineNumber;

		private static IEnumerable<Coro> InputCoro() { 
			while(true) { 
				yield return Coro.AwaitLong(() => Console.ReadLine(), out Promise<string> promise);
				coroManager.AddCoro(Worker(promise.Result, ++lineNumber));
			}
		}

		private static IEnumerable<Coro> Worker(string line, int lineNumber) { 
			//Difficult algorithm
			char[] arr = line.ToCharArray();
			Console.WriteLine($"Line {lineNumber}: {new string(arr)}");
			for(int index1 = 0; index1 < arr.Length; index1++) {
				for(int index2 = 0; index2 < arr.Length - 1; index2++) { 
					if(arr[index2] > arr[index2 + 1]) { 
						char buffer = arr[index2 + 1];
						arr[index2 + 1] = arr[index2];
						arr[index2] = buffer;
				
						//Some long operation
						yield return Task.Delay((int)new Random().Next(1000, 2000)).Await();
				
						Console.WriteLine($"Line {lineNumber}: {new string(arr)}");
					}
					
				}
			}
		}

		public static void Start() {
			coroManager = new CoroManager();
			coroManager.AddCoro(InputCoro());
			coroManager.Work();
		}
	}
}
