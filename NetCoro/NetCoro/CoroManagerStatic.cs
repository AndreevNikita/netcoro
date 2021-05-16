using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using NetCoro.DataStructs;

namespace NetCoro {


	public partial class CoroManager {
		
		public static void Work(params IEnumerable<Coro>[] coros) => Work(true, coros);

		public static void Work(params CoroExecutionController[] controlsArray) => Work(true, controlsArray);
		public static void Work(IEnumerable<CoroExecutionController> corosExectuionControllers) => Work(true, corosExectuionControllers);


		public static void Work(bool catchExceptions, params IEnumerable<Coro>[] coros) => Work(catchExceptions, coros.Select(coro => new CoroExecutionController(coro.Await())));

		public static void Work(bool catchExceptions, params CoroExecutionController[] controlsArray) => Work(catchExceptions, (IEnumerable<CoroExecutionController>)controlsArray); 

		public static void Work(bool catchExceptions, IEnumerable<CoroExecutionController> corosExectuionControllers) { 
			CoroManager coroManager = new CoroManager(catchExceptions);
			foreach(CoroExecutionController coroExecutionController in corosExectuionControllers) { 
				coroManager.AddCoro(coroExecutionController);
			}
			coroManager.WaitAndStop();
			coroManager.Work();
		}

	}
}
