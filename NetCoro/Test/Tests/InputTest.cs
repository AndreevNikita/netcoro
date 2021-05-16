using NetCoro;
using SimpleMultithreadQueue;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Test.Tests {
	public class InputTest {


		static MultithreadQueue<string> messageQueue = new MultithreadQueue<string>();

		static IEnumerable<Coro> InputCoro() { 
			while(true) { 
				yield return ((Func<string>)(() => Console.ReadLine())).Await(out Promise<string> promise);
				messageQueue.Enqueue(promise);
				if(promise.Result == "exit") {
					Console.WriteLine("Exit from reader");
					yield break;
				}
			}
		}

		static IEnumerable<Coro> Worker() { 
			int counter = 0;
			while(true) {
				//Wait for input
				yield return CoroExt.Await(() => messageQueue.R_PopAllToNewQueue_Wait(), out Promise<Queue<string>> linePromise);
				foreach(string message in linePromise.Result) {
					counter++;
					if(message == "exit") {
						Console.WriteLine("Exit from worker");
						yield break;
					}

					//Difficult algorithm
					char[] arr = message.ToCharArray();
					Console.WriteLine($"Line {counter}: {new string(arr)}");
					for(int index1 = 0; index1 < arr.Length; index1++)
						for(int index2 = 0; index2 < arr.Length - 1; index2++) { 
							if(arr[index2] > arr[index2 + 1]) { 
								char buffer = arr[index2 + 1];
								arr[index2 + 1] = arr[index2];
								arr[index2] = buffer;
								yield return Task.Delay(500).Await();
								Console.WriteLine($"Line {counter}: {new string(arr)}");
							}
							
						}
				}
			}
		}

		public static void Start() { 
			Console.WriteLine("Input test (enter 'exit' to finish)");
			CoroManager.Work(InputCoro(), Worker());

			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
		}

	}
}
