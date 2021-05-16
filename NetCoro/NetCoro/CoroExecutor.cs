using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NetCoro {
	public enum CoroStepCallback { Next, End, InterruptCurrent, InterruptAll }

	//Inner coro executor class
	public class CoroExecutor { 
		IEnumerator<Awaitable> execution;
		public Awaitable CurrentAwaitable { get; private set; } = Awaitable.DoNothingAwaitable;

		public CoroExecutionController CoroExecutionController { get; private set; }

		ICoroManager coroManager;

		private bool isInterrupted = false;

		public CoroExecutor(CoroExecutionController coroExecutionControl, ICoroManager coroManager) { 
			this.CoroExecutionController = coroExecutionControl;
			this.execution = coroExecutionControl.Coro.GetEnumerator();
			this.coroManager = coroManager;
		}

		public CoroStepCallback CompleteNext() { 
			if(isInterrupted) { 
				return CoroStepCallback.InterruptCurrent;
			}
			CurrentAwaitable.Assert(CoroExecutionController.TaskExceptionBehaviour);


			if(execution.MoveNext()) {
				CurrentAwaitable = execution.Current;
				if(CurrentAwaitable.IsInterruptTask)
					return CoroStepCallback.InterruptCurrent;
				if(CurrentAwaitable.IsInterruptAllTask)
					return CoroStepCallback.InterruptAll;

				return CoroStepCallback.Next;
			} else {
				return CoroStepCallback.End;
			}
		}

		public void OnComplete() => CoroExecutionController.OnComplete();

		public void OnCancel() => CoroExecutionController.OnCancel(); 
		
		public void OnFailture(Exception exception) => CoroExecutionController.OnFailture(exception);

		/*
		 * 
		 * Here is the main interrupt managment
		 * 
		*/

		internal void Interrupt() { 
			isInterrupted = true;
			if(CurrentAwaitable.OnInterruptStart())
				coroManager.SkipWaitOnce();
		}
	}

	internal class ForwardException : Exception { 
		public readonly Exception Exception;
		public ForwardException(Exception exception) { 
			Exception = exception;
		}
	}
}
