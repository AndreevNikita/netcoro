using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NetCoro
{
	public class Promise { 
		private Task task;
		public TaskStatus Status => task.Status;
		public Exception Exception => task.Exception;

		public Promise(Task task) {
			this.task = task;
		}

		public void Assert() { 
			if(task.IsFaulted) { 
				throw new TaskExecuteException(task);
			}
		}

		public void AssertForward() { 
			if(task.IsFaulted) { 
				throw new ForwardException(Exception);
			}
		}

	}

	public class Promise<TResult> : Promise { 

		private Task<TResult> resultTask;
		public TResult Result => resultTask.Result;
		
		internal Promise(Task<TResult> task) : base(task) { 
			this.resultTask = task;
		}
		
		public static implicit operator TResult(Promise<TResult> promise) => promise.Result;
	}
}
