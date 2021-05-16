using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetCoro {

	//Coro base

	public abstract partial class Coro : IEnumerable<Awaitable> {
		
		public CoroExecutor CreateExecutor(ICoroManager coroManager) => new CoroExecutor(this, coroManager);

		public abstract IEnumerator<Awaitable> GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	}

	//Middle coro

	public class NestedCoro : Coro { 

		private IEnumerable<Coro> nestedCoroEnumerable;

		public NestedCoro(IEnumerable<Coro> nestedCoro) { 
			this.nestedCoroEnumerable = nestedCoro;
		}

		public override IEnumerator<Awaitable> GetEnumerator() {
			foreach(Awaitable task in nestedCoroEnumerable.SelectMany(coro => coro)) 
				yield return task;
		}
	}

	//End coros

	public class AwaitableCoro : Coro, IEnumerator<Awaitable> { 
		Awaitable task;
		bool isFirst;

		public static AwaitableCoro DoNothingCoro => new AwaitableCoro(Awaitable.DoNothingAwaitable);
		public static AwaitableCoro InterruptCurrentCoro => new AwaitableCoro(Awaitable.InterruptCurrentAwaitable);
		public static AwaitableCoro InterruptAllCoro => new AwaitableCoro(Awaitable.InterruptAllAwaitable);

		public AwaitableCoro(Task task, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default) : this(new AwaitableTask(task, taskExceptionBehaviour)) { }

		public AwaitableCoro(Awaitable task) { 
			this.task = task;
			this.isFirst = true;
		}

		public Awaitable Current { get; private set; }

		object IEnumerator.Current => Current;

		public override IEnumerator<Awaitable> GetEnumerator() => this;

		public bool MoveNext() {
			if(isFirst) { 
				isFirst = false;
				Current = task;
				return true;
			}
			return false;
		}

		public void Dispose() { }

		public void Reset() {
			throw new NotImplementedException();
		}
	}

}
