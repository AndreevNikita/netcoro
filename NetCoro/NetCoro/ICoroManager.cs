using System;
using System.Collections.Generic;
using System.Text;

namespace NetCoro {
	public interface ICoroManager {

		void Work();

		void WaitAndStop();

		bool AddCoro(CoroExecutionController coro);

		//Skips wait for any Awaitable and starts next iteration
		void SkipWaitOnce();

	}
}
