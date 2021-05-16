using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetCoro {

	public enum AwaitableType { 
		AsyncTask, WaitHandle, Delay, DoNothing, Interrupt, InterruptAll
	}

	public enum TaskExceptionBehaviour { 
		Default, Nothing, ThrowInCoro
	}

	public class Awaitable {

		public static Awaitable DoNothingAwaitable => new Awaitable(AwaitableType.DoNothing);
		public static Awaitable InterruptCurrentAwaitable => new Awaitable(AwaitableType.Interrupt);
		public static Awaitable InterruptAllAwaitable => new Awaitable(AwaitableType.InterruptAll);

		
		public readonly AwaitableType Type;

		public bool IsAsyncTask => Type == AwaitableType.AsyncTask;
		public bool IsDoNothing => Type == AwaitableType.DoNothing;
		public bool IsInterruptTask => Type == AwaitableType.Interrupt;
		public bool IsInterruptAllTask => Type == AwaitableType.InterruptAll;
		public bool IsDelayTask => Type == AwaitableType.Delay;
		public bool IsWaitHandle => Type == AwaitableType.WaitHandle;

		public virtual bool IsFinished => true;
		public virtual bool IsSuccess => true;

		public Awaitable(AwaitableType type) { 
			this.Type = type;
		}

		public virtual void Assert(TaskExceptionBehaviour defaultBehaviour) { 
			if(!IsSuccess) { 
				switch(defaultBehaviour) { 
					case TaskExceptionBehaviour.Default:
					case TaskExceptionBehaviour.Nothing:
						return;
					case TaskExceptionBehaviour.ThrowInCoro:
						throw new AwaitableExecuteException("UnknownException");
				}
			}
		}

		public virtual void Start() { }

		//Returns true if interrupted immidiately
		public virtual bool OnInterruptStart() => false;


		private bool isCached = false;
		public void Cache<T>(ICollection<T> cacheCollection) { 
			if(isCached)
				return;
			
			AddToCache(cacheCollection);
			isCached = true;
		}

		protected virtual void AddToCache<T>(ICollection<T> cacheCollection) { }

		public void Uncache<T>(ICollection<T> cacheCollection) {
			if(!isCached)
				return;
			
			RemoveFromCache(cacheCollection);
			isCached = false;
		}

		protected virtual void RemoveFromCache<T>(ICollection<T> cacheCollection) { }

	}

	public class AwaitableTask : Awaitable {
		

		public readonly Task Task;
		public readonly TaskExceptionBehaviour TaskExceptionBehaviour;
		public override bool IsFinished => Task.IsCompleted || Task.IsFaulted || Task.IsCanceled;
		public override bool IsSuccess => !Task.IsFaulted;

		public AwaitableTask(Task task, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default) : base(AwaitableType.AsyncTask) { 
			Task = task;
			TaskExceptionBehaviour = taskExceptionBehaviour;
		}

		public override void Start() { 
			if(Task.Status == TaskStatus.Created)
				Task.Start();
		}
		public override void Assert(TaskExceptionBehaviour defaultBehaviour) { 
			TaskExceptionBehaviour exceptionBehaviour = TaskExceptionBehaviour != TaskExceptionBehaviour.Default ? TaskExceptionBehaviour : defaultBehaviour;
			if(!IsSuccess) { 
				switch(exceptionBehaviour) { 
					case TaskExceptionBehaviour.Default:
					case TaskExceptionBehaviour.Nothing:
						return;
					case TaskExceptionBehaviour.ThrowInCoro:
						throw new TaskExecuteException(Task);
				}
			}
		}

		protected override void AddToCache<T>(ICollection<T> cacheCollection) => ((ICollection<Task>)cacheCollection).Add(Task);
		protected override void RemoveFromCache<T>(ICollection<T> cacheCollection) => ((ICollection<Task>)cacheCollection).Remove(Task);
	}

	public class WaitHandleAwaitable : Awaitable { 

		public readonly WaitHandle WaitHandle;

		private bool isInterrupted = false;
		public override bool IsFinished => isInterrupted || taskCompletionSource.Task.IsCompleted; //Doesn't block a thread

		private RegisteredWaitHandle registredWaitHandle;
		private TaskCompletionSource<bool> taskCompletionSource;

		public WaitHandleAwaitable(WaitHandle waitHandle) : base(AwaitableType.WaitHandle) {
			WaitHandle = waitHandle;
			taskCompletionSource = new TaskCompletionSource<bool>();
			taskCompletionSource.Task.ContinueWith(UnregisterWaitHandleDelegate);
			registredWaitHandle = ThreadPool.RegisterWaitForSingleObject(waitHandle, WaitHandleCallback, null, -1, true);
		}

		private void WaitHandleCallback(object state, bool timedOut) { 
			taskCompletionSource.SetResult(true);
		}

		private void UnregisterWaitHandleDelegate(Task _) { 
			registredWaitHandle.Unregister(null);
		}


		public override bool OnInterruptStart() { 
			UnregisterWaitHandleDelegate(taskCompletionSource.Task);
			isInterrupted = true; 
			return true; 
		}

		protected override void AddToCache<T>(ICollection<T> cacheCollection) => ((ICollection<Task>)cacheCollection).Add(taskCompletionSource.Task);
		protected override void RemoveFromCache<T>(ICollection<T> cacheCollection) => ((ICollection<Task>)cacheCollection).Remove(taskCompletionSource.Task);
	}

	public class DelayAwaitable : Awaitable {

		public DateTime CooldownTime { get; private set;}

		private bool isInterrupted = false;
		public override bool IsFinished => isInterrupted || CooldownTime <= DateTime.Now;

		public DelayAwaitable(DateTime cooldownTime) : base(AwaitableType.Delay) { 
			CooldownTime = cooldownTime;
		}

		public static DelayAwaitable MakeSleep(TimeSpan sleepTime) => new DelayAwaitable(DateTime.Now + sleepTime);

		public override bool OnInterruptStart() { isInterrupted = true; return true; }
	}

	public class AwaitableExecuteException : Exception {

		public AwaitableExecuteException(string message) : base(message) { }

	}

	public class TaskExecuteException : AwaitableExecuteException { 
		public readonly Exception CausedByException;
		public readonly Task Task;
		public TaskExecuteException(Task task) : base(task.Exception.Message) { 
			CausedByException = task.Exception;
			Task = task;
		}
	}
}
