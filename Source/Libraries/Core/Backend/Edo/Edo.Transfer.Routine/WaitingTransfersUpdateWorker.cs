using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;

namespace Edo.Transfer.Routine
{
	public class WaitingTransfersUpdateWorker : TimerBackgroundServiceBase
	{
		public WaitingTransfersUpdateWorker()
		{
			
		}
		protected override TimeSpan Interval => throw new NotImplementedException();

		protected override Task DoWork(CancellationToken stoppingToken)
		{
			throw new NotImplementedException();
		}
	}
}
