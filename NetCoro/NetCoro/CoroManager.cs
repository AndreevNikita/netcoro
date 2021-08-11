using NetCoro.DataStructs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace NetCoro {
	public partial class CoroManager : ICoroManager {

		private Channel<CoroExecutionController> corosChannel;
		private LinkedContainer<CoroExecutor> corosContainer;
		private AwaitableWaiter waiter = new AwaitableWaiter();

		bool catchExceptions;
		volatile bool receiveNewCorosFlag = true;

		public int CorosCount => addCorosCoroExectunioController.IsFinished ? corosContainer.Count : corosContainer.Count - 1;


		CoroExecutionController addCorosCoroExectunioController;

		public CoroManager(bool catchExceptions = true) {
			this.catchExceptions = catchExceptions;
			Reset();
		}

		private void Reset() { 
			corosChannel = Channel.CreateUnbounded<CoroExecutionController>(new UnboundedChannelOptions() { SingleReader = true });
			corosContainer = new LinkedContainer<CoroExecutor>();
			receiveNewCorosFlag = true;
			waiter.Clear();
		}

		private void R_AddNewCoros() { 
			while(corosChannel.Reader.TryRead(out CoroExecutionController coro)) { 
				corosContainer.Add(coro.StartUse(this));
			}
		}

		CancellationTokenSource corosAddCancellationTokenSource;

		private IEnumerable<Coro> R_AddCorosCoro() { 
			Promise<CoroExecutionController> promise;
			

			while(receiveNewCorosFlag) {
				yield return corosChannel.Reader.ReadAsync(corosAddCancellationTokenSource.Token).AsTask().Await(out promise);
				if(promise.Status == TaskStatus.RanToCompletion) { 
					corosContainer.Add(promise.Result.StartUse(this));
				}
			}

		}

		

		public void Work() {
			corosAddCancellationTokenSource = new CancellationTokenSource();

			corosContainer.Clear();
			R_AddNewCoros();
			addCorosCoroExectunioController = R_AddCorosCoro().Await().Control();
			corosContainer.Add(addCorosCoroExectunioController.StartUse(this));

			while(true) {
				foreach(var coroContainer in corosContainer) { 

					if(coroContainer.Value.CurrentAwaitable.IsFinished) { 
						waiter.Remove(coroContainer.Value.CurrentAwaitable);
						try {
							switch(coroContainer.Value.CompleteNext()) { 
								case CoroStepCallback.End:
									corosContainer.Remove(coroContainer);
									coroContainer.Value.OnComplete();
									continue;
								case CoroStepCallback.InterruptCurrent:
									corosContainer.Remove(coroContainer);
									coroContainer.Value.OnCancel();
									continue;
								case CoroStepCallback.InterruptAll:
									R_StopReceiveNewCoros();
									R_AddNewCoros();
									foreach(var cancelCoroContainer in corosContainer) {
										cancelCoroContainer.Value.Interrupt();
									}
									continue;
								case CoroStepCallback.Next:
									coroContainer.Value.CurrentAwaitable.Start();
									break;
							}
						} catch(ForwardException exception) { 
							throw exception.Exception;
						} catch(Exception exception) when(catchExceptions) {
							corosContainer.Remove(coroContainer);
							coroContainer.Value.OnFailture(exception);
							continue;
						}
					}
					
					waiter.Add(coroContainer.Value.CurrentAwaitable);
				}

				if(corosContainer.IsEmpty)
					break;

				waiter.Wait();
			}

			Reset();
		}


		private void R_StopReceiveNewCoros() { 
			receiveNewCorosFlag = false;
			Interlocked.MemoryBarrier();
			//corosChannel.SkipWait();
			try {
				corosAddCancellationTokenSource.Cancel();
			} catch(Exception e) { }
		}

		
		public void WaitAndStop() {
			R_StopReceiveNewCoros();
		}

		public bool AddCoro(CoroExecutionController coroExecutorProperties) {
			if(receiveNewCorosFlag) {
				corosChannel.Writer.WriteAsync(coroExecutorProperties).AsTask().Wait();
				return true;
			}
			return false;
		}

		public void SkipWaitOnce() => waiter.SkipWait();
	}
}
