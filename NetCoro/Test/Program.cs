using System;
using System.Threading;
using Test.Tests;

namespace Test
{

	class Program {

		static void Main(string[] args) {

			RoundRobinTest.Start();
			DelayTest.Start();
			
			InputTest.Start();
			
			WaitHandleTest.Start();
			TasksTest.Start();
			
			CoroManagerTest.Start();
			
			
			NestedCorosTest.Start();

			CoroControlTest.Start();
			
			//ExceptionsTest.Start();
			
			//Console.ReadKey();
		}
		

		
	}
}
