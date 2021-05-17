![NetCoro logo](logo.png)

<br/>

### [Ru](README_ru.md)

<br/>  

# NetCoro
NetCoro is a conceptual library, that adds coroutines to c# language

Benefits of coroutines:
- one main thread
- the ability to easily pause the execution of a method while waiting for an event, and executing another method at this time
- no race conditions
- sometimes coroutines make a code with pattern "Chain of Command" simpler

<br/>

# Getting started

## Coroutine

In the NetCoro library coroutine initialy is an object, that implements interface **IEnumerable\<Awaitable\>**, that can return awaitable objects sequentially. But we can get this iterator from **IEnumerable<Coro>** object, sequentially getting coroutines and getting **Awaitable** objects from each of them. Therefore, for convenience, we will call "coroutines" objects of type **IEnumerable\<Coro\>**. We can get **Coro** iterator from methods with `yield return`. For example:
```c#
public static IEnumerable<Coro> myCoro() {
	Console.WriteLine("Executable code before await");
	//...
	yield return Coro.Await(() => SomeLongMethod());
	//...
	Console.WriteLine("Executable code after await");
}
```

For convenience in NetCoro all awaitable objects bringing to **Coro**.Thus, it is possible to create recursive iterator methods.

## Await

For visual convenience preparation of an object for return via `yield return` is carried out through the method `Coro Coro.Await(...)`, overloaded for different awaitable objects types. There is an extension-method in the **CoroExt** class for each overload of `Coro Coro.Await(...)` (only for probabliity to write `myTask.Await()` instead `Coro.Await(myTask)`).

At the moment, waiting for objects of the following types is provided :
- **Task**
- **Action** / **Func\<T\>** - a **Task** is created from the delegate 
- **WaitHandle**
- **DateTime** - pause coroutine before a certain time  
- **int** / **double** / **TimeSpan** (pause the execution of a coroutine for a certain number of milliseconds)

There are also a control **Awaitable** objects:
- `Coro.DoNothing();` - pause the execution of the current coroutine and go to the next
- `Coro.InterruptCurrent();` - interrupt the execution of the current coroutine 
- `Coro.InterruptAll();` - abort execution of all coroutines and exit the method  **Work**


Iterators **IEnumerable\<Coro\>** are nested coroutines. When returning a coroutine based on **IEnumerable\<Coro\>**, the final expected **Awaitable** object is returned recursively.

## AwaitLong

Earlier it was said that delegates are cast and processed as **Task** . The **AwaitLong** method is used for delegates that expect to wait a long time (for example, waiting for user input `yield return Coro.AwaitLong(Console.ReadLine);`). In fact, in **AwaitLong**, tasks are created with the parameter `TaskCreationOptions.LongRunning`.


## Promise

After waiting for a task or delegate, the execution result can be obtained by reference using an object of the **Promise** or the **Promise\<T\>** type in case the expected object returns a result. 

### Properties and methods of **Promise**:
- `TaskStatus Status` - status of finished task
- `Exception Exception` - exception (if thrown) of finished task
- `Asset()` - throws an exception if the task was not completed successfully due to an internal exception 
- `AssertForward()` - throws an exception of type **ForwardException** thrown from CoroManager to the calling method anyway

If a task returns a result, Await returns a **Promise\<TResult\>** object by reference, where **TResult** is a type of return value. This class inherits the **Pormise** class and has all of this properties and methods and:
- `TResult Result` - a result of the task

Example:
```c#
public static IEnumerable<Coro> inputCoro() {

	yield return Coro.AwaitLong(() => Console.ReadLine(), out Promise<string> promise);
	promise.Assert();
	Console.WriteLine($"Task result: {promise.Result}");	
	
}
```

## CoroManager

**CoroManager** - the object, that executing coroutines.

### Constructor 
```c#
CoroManager(bool catchExceptions = true)
```
- `bool catchExceptions` - if true, during execution, all exceptions will be caught, except **ForwardException** and transferred for processing to **CoroExecutionController** of coroutine from which the exception was thrown 

### Methods:
- `bool AddCoro(CoroExecutionController coroExecutorProperties)` - add a coroutine to (execution controler) to **CoroManager**
- `void Work()` - a method that blocks the current thread and executes the coroutines passed using the **AddCoro** method
- `void WaitAndStop()` - waiting for unfinished coroutines and stopping **CoroManager**. After stopping work, the program continues to run after calling the **Work** method 
- `void SkipWaitOnce()` - skips waiting for Awaitable objects returned by coroutines once
-  **AddCoro** extension method
	```c#
	public static void AddCoro(
		this ICoroManager coroManager, 
		IEnumerable<Coro> coro, 
		Action<CoroExecutionController> finished = null,
		Action completed = null, 
		Action canceled = null, 
		Action<Exception> failtured = null,
		TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default
	) => coroManager.AddCoro(coro.Control(finished, completed, canceled, failtured, taskExceptionBehaviour));
	```
	The extension method for the **CoroManager** objects, allowing you to add iterators to execution by creating a **CoroExecutionController** and passing it to  `bool AddCoro(CoroExecutionController coroExecutorProperties)`. This method makes it possible to add to execution without creating a **CoroExecutionController** explicitly `coroManager.AddCoro(myCoroMethod);`


## Static CoroManager

For cases when you just need to wait for the execution of several coroutines, there is a static method Work and its overload
- `public static void Work(params IEnumerable<Coro>[] coros)`
- `public static void Work(params CoroExecutionController[] controlsArray)`
- `public static void Work(IEnumerable<CoroExecutionController> corosExectuionControllers)`
- `public static void Work(bool catchExceptions, params IEnumerable<Coro>[] coros)`
- `public static void Work(bool catchExceptions, params CoroExecutionController[] controlsArray)`
- `public static void Work(bool catchExceptions, IEnumerable<CoroExecutionController> corosExectuionControllers)`

All methods take as one of the arguments an executable coroutine or a execution controller.
The second possible argument `bool catchExceptions` indicates whether to catch exceptions inside the method **Work** and regularly perform other coroutines or should to throw out the exception.

## CoroExecutionController

**CoroExecutionController** - an object for monitoring the execution of one coroutine

### Constructor:
`public CoroExecutionController(Coro coro, Action<CoroExecutionController> finished = null, Action completed = null, Action canceled = null, Action<Exception> failtured = null, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default)`
- `Coro coro` - coroutine, the execution of which will be controlled
- `Action<CoroExecutionController> finished = null` - a delegate to be called when a coroutine finishes executing anyway. The completed coroutine controller passes itself as an argument 
- `Action completed = null` - a delegate to be called when a coroutine successfully completed
- `Action canceled = null` - a delegate to be called when a coroutine canceled (for example, if `Coro.InterruptCurrent();` or `Coro.InterruptAll();` was returned)
- `Action<Exception> failtured = null` - a delegate to be called when an exception is thrown (in case when **catchExceptions** in the **CoroManager** was **true**)
- `TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default` - behaviour when exception in the coroutine was occured (see below for details)

### Properties:
- `event Action<CoroExecutionController> Finished` - a delegate to be called when a coroutine finishes executing anyway. The completed coroutine controller passes itself as an argument 
- `event Action Completed` - a delegate to be called when a coroutine successfully completed
- `event Action Canceled` - a delegate to be called when a coroutine canceled (for example, if `Coro.InterruptCurrent();` or `Coro.InterruptAll();` was returned)
- `event Action<Exception> Failtured` - a delegate to be called when an exception is thrown (in case when **catchExceptions** in the **CoroManager** was **true**)
- `Exception Exception` - an exception, which was occured when the coroutine execution or **null**
- `bool IsFinished` - indicates the coroutine completion status

### Methods:
- `void Interrupt()` - interrupts the execution of the coroutine
- `void Wait()` - blocks calling thread and waits for the coroutine execution finish
- `InterruptAndWait()` - starts to interrupt the coroutine execution and blocks the current thread until it stops

Also there are **Control** extension methods in the **CoroExt** static class with the first parameter of the **Coro** and **IEnumerable\<Coro\>** types. In fact it was created for a convenience. All arguments of the **Control** extension methods are arguments of **CoroExecutionController** constructor.  


## Exceptions handling

Exceptions in coroutines:
- Exceptions thrown from iterator methods are caught and passed to the Failtured delegate of the execution controller
- Exceptions of type **ForwardException** are always thrown from the Work method. Such exceptions are meant to be handled externally 

Handling **Awaitable** exceptions:
- When calling the **Await** method for asynchronous tasks (and delegates), you can specify the **TaskExceptionBehaviour** parameter 
    - **TaskExceptionBehaviour.Nothing** - when an exception occurs in **Task**, nothing happens 
	- **TaskExceptionBehaviour.ThrowInCoro** - when an exception occurs in **Task**, this exception throws in the waiting coroutine in the main thread
	- **TaskExceptionBehaviour.Default** - the behavior is determined by the **taskExceptionBehaviour** parameter passed to the constructor of the execution controller of the current coroutine (if the **TaskExceptionBehaviour.Default** value was also passed during the creation of **CoroExecutionController**, nothing happens)
- If an exception was occured but was not thrown automatically with the **TaskExceptionBehaviour** parameter, it's possible to check the task for an exception and throw it if it was thrown using `promise.Asset()` 
- You can also throw an exception of type **ForwardException** with `promise.AssetForward()`

# Examples

You can find all examples in [Examples](Examples/Examples) project

## Example 1 (Round Robin)

When this code is run, coroutines will be executed, printing the characters "c", "a", "t", "" 30 times, passing control to each other via `yield return Coro.DoNothing ();`. As a result, the console will display
> cat cat cat cat cat ...

```c#
public class Example1 {

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
```

## Example 2 (Long tasks and delays)

In this example, **InputCoro** is waiting for user input, at this time **Worker** coroutines can be executed, sorting the string passed as the **line** argument to . `yield return Task.Delay (...);` simulates waiting for some long operation, while waiting for which other **Worker** methods can be executed.

```c#
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
```

## Example 3 (CoroExecutionController)

This example demonstrates creating a **CoroExectionController** for a coroutine and adding callback methods 

```c#
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
```

## More

You can find more examples in the [Test](NetCoro/Test/Tests) project

<br/>

# Thanks!
Thanks to Anastasia Danilova ([@hakishima_art](https://www.instagram.com/hakishima_art)) for the cool project logo!
