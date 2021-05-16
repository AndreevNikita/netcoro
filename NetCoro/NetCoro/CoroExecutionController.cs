using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NetCoro {
	//Outer interface to control coroutine execution
	public class CoroExecutionController { 

		internal readonly Coro Coro;
		public event Action<CoroExecutionController> Finished;
		public event Action Completed;
		public event Action Canceled;
		public event Action<Exception> Failtured;
		public TaskExceptionBehaviour TaskExceptionBehaviour { get; set; }

		public Exception Exception { get; private set; }

		private ManualResetEvent interruptWaiter;
		CoroExecutor controlledExecutor = null;

		public bool IsFinished { get; private set; } = false;

		public CoroExecutionController(Coro coro, Action<CoroExecutionController> finished = null, Action completed = null, Action canceled = null, Action<Exception> failtured = null, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default) { 
			this.Coro = coro;
			this.Finished = finished;
			this.Completed = completed;
			this.Canceled = canceled;
			this.Failtured = failtured;
			this.TaskExceptionBehaviour = taskExceptionBehaviour;
			this.interruptWaiter = new ManualResetEvent(false);
		}


		int usageCounter = 0;
		public CoroExecutor StartUse(CoroManager coroManager)  {
			if(Interlocked.Increment(ref usageCounter) != 1)
				throw new Exception("Execution controller can be used only once");

			return controlledExecutor = new CoroExecutor(this, coroManager); 
		}

		public static implicit operator CoroExecutionController(Coro coro) => new CoroExecutionController(coro);


		public CoroExecutionController Clone() => new CoroExecutionController(Coro, Finished, Completed, Canceled, Failtured, TaskExceptionBehaviour);

		//Interrupt from other thread
		public void Interrupt() {
			controlledExecutor.Interrupt();
		}

		public void Wait() { 
			interruptWaiter.WaitOne();
		}

		public void InterruptAndWait() { 
			Interrupt();
			Wait();
		}

		private void OnFinish() {
			IsFinished = true;
			interruptWaiter.Set();
			Finished?.Invoke(this);
		}

		internal void OnComplete() {
			Completed?.Invoke(); 

			OnFinish();
		}
		
		internal void OnCancel() {
			Canceled?.Invoke();

			OnFinish();
		}

		internal void OnFailture(Exception exception) {
			Exception = exception;
			Failtured?.Invoke(exception);

			OnFinish();
		}

	}
}
