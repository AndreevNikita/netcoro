using NetCoro;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test.Tests {
	public class CoroManagerTest {

		private static CoroManager coroManager;
		private static int counter = 0;

		const int MIN_SORT_OPERATION_DELAY = 50;
		const int MAX_SORT_OPERATION_DELAY = 100;

		const int RANDOM_STRING_MIN_SIZE = 8;
		const int RANDOM_STRING_MAX_sIZE = 24;
		const string RANDOM_STRING_CHARACTERS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

		const bool MIX_ENABLED = false;

		private static long delayDifferenceCounter = 0;
		private static long iterationsCounter = 0;

		private static CoroExecutionController lastCoroExecutionController;

		static IEnumerable<Coro> InputCoro() { 
			while(true) { 
				yield return Coro.AwaitLong(() => Console.ReadLine(), out Promise<string> promise);
				
				//messageQueue.Enqueue(promise);
				if(promise.Result == "exit") {
					coroManager.WaitAndStop();
					yield break;
				} else if(promise.Result == "e") { 
					coroManager.AddCoro(ExceptionCoro().Control(failtured : (Exception e) => Console.WriteLine($"Exception occured:\n{e.Message}")));
				} else if(promise.Result == "i") { 
					yield return Coro.InterruptAll();
				} else if(promise.Result == "il") { 
					lastCoroExecutionController?.Interrupt();
				//Stress test with n of random strings
				} else if(int.TryParse(promise.Result, out int count)) { 
					//Generate a random string
					Random rand = new Random();
					for(int index = 0; index < count; index++) { 
						int length = rand.Next(RANDOM_STRING_MIN_SIZE, RANDOM_STRING_MAX_sIZE);
						char[] resultStringSymbols = new char[length];
						for(int charIndex = 0; charIndex < resultStringSymbols.Length; charIndex++)
							resultStringSymbols[charIndex] = RANDOM_STRING_CHARACTERS[rand.Next(0, RANDOM_STRING_CHARACTERS.Length)];
						coroManager.AddCoro(lastCoroExecutionController = Worker(new string(resultStringSymbols), ++counter).Control(canceled : OnCoroCancel));
						
					}
				} else {
					coroManager.AddCoro(lastCoroExecutionController = Worker(promise.Result, ++counter).Control(canceled : OnCoroCancel));
				}
			}
		}

		static void OnCoroCancel() => Console.WriteLine("Coro canceled");

		static IEnumerable<Coro> Worker(string line, int lineNumber) { 
			Stopwatch stopWatch = new Stopwatch();

			//Difficult algorithm
			char[] arr = line.ToCharArray();
			Console.WriteLine($"Line {lineNumber}: {new string(arr)}");
			for(int index1 = 0; index1 < arr.Length; index1++) {
				for(int index2 = 0; index2 < arr.Length - 1; index2++) { 
					if(arr[index2] > arr[index2 + 1]) { 
						char buffer = arr[index2 + 1];
						arr[index2 + 1] = arr[index2];
						arr[index2] = buffer;
						//Mixed coros and tasks test
						long currentDelay = new Random().Next(MIN_SORT_OPERATION_DELAY, MAX_SORT_OPERATION_DELAY);
						stopWatch.Restart();
						char delayType;
						if(MIX_ENABLED && lineNumber % 2 == 0) {
							yield return Task.Delay((int)currentDelay).Await();
							delayType = 'T';
						} else {
							yield return currentDelay.Await();
							delayType = 'D';
						}
						stopWatch.Stop();
						long elapsed = stopWatch.ElapsedMilliseconds;
						long delayDifference = elapsed - currentDelay;

						delayDifferenceCounter += delayDifference;
						iterationsCounter++;

						string messageLine = $"Line {lineNumber:D7} ({delayType}; exc delay: {currentDelay:D4}; in fact delay: {elapsed:D4}; ddelay: {delayDifference:D4}; workers count: {coroManager.CorosCount:D7}): {new string(arr)}";
						yield return Coro.Await(() => Console.WriteLine(messageLine));
					}
							
				}
			}
		}

		static IEnumerable<Coro> ExceptionCoro() { 
			throw new Exception("Exception in exception coro!");
			yield break;
		}

		public static void Start() { 
			delayDifferenceCounter = 0;
			iterationsCounter = 0;
			Console.WriteLine(
				"Enter the text to sort or a number n to generate n random texts to sort;\n" +
				"\"e\" to throw an exception from coroutine;\n" +
				"\"i\" to interrupt all now;\n" +
				"\"il\" to interrupt last\n" +
				"\"exit\" to exit"
			);
			coroManager = new CoroManager();
			coroManager.AddCoro(InputCoro());
			coroManager.Work();
			Console.WriteLine($"Awerage delay difference: {(double)delayDifferenceCounter / (double)iterationsCounter}");

			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
		}


	}
}
