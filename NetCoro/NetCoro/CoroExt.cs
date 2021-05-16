using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoro {

	public partial class Coro { 

		public static NestedCoro Await(IEnumerable<Coro> coro) => new NestedCoro(coro);

		//Tasks await

		public static Coro Await<T>(Func<T> func, out Promise<T> promise, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None) => Await(new Task<T>(func, taskCreationOptions), out promise, taskExceptionBehaviour);
		public static Coro AwaitLong<T>(Func<T> func, out Promise<T> promise, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default) => Await(func, out promise, taskExceptionBehaviour, TaskCreationOptions.LongRunning);



		public static Coro Await(Action action, out Promise promise, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None) => Await(new Task(action, taskCreationOptions), out promise, taskExceptionBehaviour);
		public static Coro AwaitLong(Action action, out Promise promise, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default) => Await(action, out promise, taskExceptionBehaviour, TaskCreationOptions.LongRunning);



		public static Coro Await<T>(Func<T> func, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None) => Await(new Task<T>(func, taskCreationOptions), taskExceptionBehaviour);
		public static Coro AwaitLong<T>(Func<T> func, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default) => Await(func, taskExceptionBehaviour, TaskCreationOptions.LongRunning);



		public static Coro Await(Action action, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None) => Await(new Task(action, taskCreationOptions), taskExceptionBehaviour);
		public static Coro AwaitLong(Action action, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default) => Await(action, taskExceptionBehaviour, TaskCreationOptions.LongRunning);



		public static Coro Await<T>(Task<T> task, out Promise<T> promise, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default) { 
			promise = new Promise<T>(task);
			return Await(task, taskExceptionBehaviour);
		}

		public static Coro Await(Task task, out Promise promise, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default) { 
			promise = new Promise(task);
			return Await(task, taskExceptionBehaviour);
		}


		public static Coro Await(Task task, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default) => new AwaitableCoro(task, taskExceptionBehaviour);

		//WaitHandle await

		public static Coro Await(WaitHandle waitHandle) => new AwaitableCoro(new WaitHandleAwaitable(waitHandle));

		//Delay await

		public static Coro Await(DateTime timeToWait) => new AwaitableCoro(new DelayAwaitable(timeToWait));

		public static Coro Await(TimeSpan sleepTime) => new AwaitableCoro(DelayAwaitable.MakeSleep(sleepTime));

		public static Coro Await(double sleepMillis) => new AwaitableCoro(DelayAwaitable.MakeSleep(TimeSpan.FromMilliseconds(sleepMillis)));

		//Management calls

		public static Coro DoNothing() => AwaitableCoro.DoNothingCoro;
		public static Coro InterruptCurrent() => AwaitableCoro.InterruptCurrentCoro;
		public static Coro InterruptAll() => AwaitableCoro.InterruptAllCoro;

	}

	public static class CoroExt {


		public static NestedCoro Await(this IEnumerable<Coro> coro) => Coro.Await(coro);


		//Tasks await

		public static Coro Await<T>(this Func<T> func, out Promise<T> promise, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None) => Coro.Await(func, out promise, taskExceptionBehaviour, taskCreationOptions);
		public static Coro AwaitLong<T>(this Func<T> func, out Promise<T> promise, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default) => Await(func, out promise, taskExceptionBehaviour, TaskCreationOptions.LongRunning);

		public static Coro Await(this Action action, out Promise promise, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None) => Coro.Await(action, out promise, taskExceptionBehaviour, taskCreationOptions);
		public static Coro AwaitLong(this Action action, out Promise promise, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default) => Await(action, out promise, taskExceptionBehaviour, TaskCreationOptions.LongRunning);

		public static Coro Await<T>(this Func<T> func, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None) => Coro.Await(func, taskExceptionBehaviour, taskCreationOptions);
		public static Coro AwaitLong<T>(this Func<T> func, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default) => Await(func, taskExceptionBehaviour, TaskCreationOptions.LongRunning);

		public static Coro Await(this Action action, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default, TaskCreationOptions taskCreationOptions = TaskCreationOptions.None) => Coro.Await(action, taskExceptionBehaviour, taskCreationOptions);
		public static Coro AwaitLong(this Action action, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default) => Await(action, taskExceptionBehaviour, TaskCreationOptions.LongRunning);



		public static Coro Await<T>(this Task<T> task, out Promise<T> promise, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default) => Coro.Await(task, out promise, taskExceptionBehaviour);

		public static Coro Await(this Task task, out Promise promise, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default) => Coro.Await(task, out promise, taskExceptionBehaviour);

		public static Coro Await(this Task task, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default) => Coro.Await(task, taskExceptionBehaviour);

		//Delay await
		public static Coro Await(this DateTime timeToWait) => Coro.Await(timeToWait);

		public static Coro Await(this TimeSpan sleepTime) => Coro.Await(sleepTime);

		public static Coro Await(this double sleepMillis) => Coro.Await(sleepMillis);
		public static Coro Await(this int sleepMillis) => Coro.Await(sleepMillis);
		public static Coro Await(this long sleepMillis) => Coro.Await(sleepMillis);

		//WaitHandle await

		public static Coro Await(this WaitHandle waitHandle) => Coro.Await(waitHandle);

		//CoroExecutionProperties addition

		public static CoroExecutionController Control(
			this Coro coro, 
			Action<CoroExecutionController> finished = null,
			Action completed = null, 
			Action canceled = null, 
			Action<Exception> failtured = null,
			TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default
		) => new CoroExecutionController(coro, finished, completed, canceled, failtured, taskExceptionBehaviour);

		public static CoroExecutionController Control(
			this IEnumerable<Coro> coro, 
			Action<CoroExecutionController> finished = null,
			Action completed = null, 
			Action canceled = null,
			Action<Exception> failtured = null,
			TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default
		) => coro.Await().Control(finished, completed, canceled, failtured, taskExceptionBehaviour);


		//ICoroManager extension

		public static void AddCoro(
			this ICoroManager coroManager, 
			IEnumerable<Coro> coro, 
			Action<CoroExecutionController> finished = null,
			Action completed = null, 
			Action canceled = null, 
			Action<Exception> failtured = null,
			TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default
		) => coroManager.AddCoro(coro.Control(finished, completed, canceled, failtured, taskExceptionBehaviour));


	}
}
