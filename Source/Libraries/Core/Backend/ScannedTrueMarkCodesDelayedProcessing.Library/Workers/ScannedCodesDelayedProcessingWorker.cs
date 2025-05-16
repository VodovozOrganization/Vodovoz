using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScannedTrueMarkCodesDelayedProcessing.Library.Option;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;

namespace ScannedTrueMarkCodesDelayedProcessing.Library.Workers
{
	public class ScannedCodesDelayedProcessingWorker : TimerBackgroundServiceBase
	{
		private readonly ILogger<ScannedCodesDelayedProcessingWorker> _logger;
		private readonly IOptions<ScannedCodesDelayedProcessingOptions> _options;

		public ScannedCodesDelayedProcessingWorker(
			ILogger<ScannedCodesDelayedProcessingWorker> logger,
			IOptions<ScannedCodesDelayedProcessingOptions> options)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options;
		}

		protected override TimeSpan Interval => _options.Value.ScanInterval;

		protected override Task DoWork(CancellationToken stoppingToken)
		{
			throw new NotImplementedException();
		}
	}
}
