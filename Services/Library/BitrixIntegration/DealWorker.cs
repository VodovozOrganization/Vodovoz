using System;
using System.Threading;
using System.Threading.Tasks;

namespace BitrixIntegration
{
	public class DealWorker 
	{
        private readonly DealProcessor _dealProcessor;
		private readonly int _interval;
		private CancellationTokenSource _cts;
		private DateTime? _lastDay;

		public DealWorker(DealProcessor dealProcessor, int interval = 60000)
        {
            _dealProcessor = dealProcessor ?? throw new ArgumentNullException(nameof(dealProcessor));

			if(interval < 1)
			{
				throw new ArgumentException("Значение интервала должно быть более нуля");
			}
			_interval = interval;
		}

		public void Start()
		{
			_cts?.Dispose();
			_cts = new CancellationTokenSource();
			Iteration(_cts.Token);
		}

		public void Stop()
		{
			_cts?.Cancel();
		}

		private void Iteration(CancellationToken cancellationToken)
		{
			Task.Run(Work, cancellationToken).ContinueWith((delayTask) => {
				Task.Delay(_interval, cancellationToken).ContinueWith((worTask) => Iteration(cancellationToken), cancellationToken);
			}, cancellationToken);			
		}

		private void Work()
		{
			var today = DateTime.Today;
			if(_lastDay.HasValue && _lastDay.Value.Day != today.Day)
			{
				_dealProcessor.ProcessDeals(_lastDay.Value);
			}
			else
			{
				_dealProcessor.ProcessDeals(today);
			}
			_lastDay = today;
		}
    }
}