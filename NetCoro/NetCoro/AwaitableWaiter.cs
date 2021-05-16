using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoro {
	public class AwaitableWaiter {

		public HashSet<Task> tasksToWait = new HashSet<Task>();

		DateTime ? waitDateTime;
		private bool HasDelayAwaitable => waitDateTime != null;
		CancellationTokenSource skipWaitCancellationTokenSource;
		bool hasFinished;

		public AwaitableWaiter() {
			Clear();
		}


		public void Add(Awaitable awaitable) { 
			if(awaitable.IsFinished)
				hasFinished = true;

			switch(awaitable.Type) { 
				case AwaitableType.WaitHandle:
				case AwaitableType.AsyncTask:
					awaitable.Cache(tasksToWait);
					break;
				case AwaitableType.Delay:
					DateTime awaitableCooldownTime = ((DelayAwaitable)awaitable).CooldownTime;
					if((waitDateTime == null) || (awaitableCooldownTime < waitDateTime))
						waitDateTime = awaitableCooldownTime;
					break;
			}
				
		}

		public void Remove(Awaitable awaitable) { 
			switch(awaitable.Type) { 
				case AwaitableType.WaitHandle:
				case AwaitableType.AsyncTask:
					awaitable.Uncache(tasksToWait);
					break;
			}
		}


		private readonly TimeSpan MINUS_ONE = TimeSpan.FromMilliseconds(-1);

		public void Wait() {
			//Console.WriteLine("Start wait");
			if(hasFinished || !skipWaitCancellationTokenSource.IsCancellationRequested) {
				TimeSpan waitTime = MINUS_ONE;

				bool waitTasks = tasksToWait.Count != 0;

				if(HasDelayAwaitable) { 
					TimeSpan testWaitTime = waitDateTime.Value - DateTime.Now;

					if(testWaitTime > TimeSpan.Zero) {
						waitTime = testWaitTime;
					} else
						waitTasks = false;
				}

				if(waitTasks) {
					try {
					
						Task.WaitAny(tasksToWait.ToArray(), (int)waitTime.TotalMilliseconds, skipWaitCancellationTokenSource.Token);
					} catch {
						//Console.WriteLine($"Exception: {exception.Message}");
					} 
				} else if(waitTime != MINUS_ONE) {
					Thread.Sleep(waitTime);
				}
			}
			Clear();
		}

		public void Clear() { 
			//tasksToWait = new HashSet<Task>();
			hasFinished = false;
			waitDateTime = null;
			skipWaitCancellationTokenSource = new CancellationTokenSource();
		}

		public void SkipWait() => skipWaitCancellationTokenSource.Cancel();
	}
}
