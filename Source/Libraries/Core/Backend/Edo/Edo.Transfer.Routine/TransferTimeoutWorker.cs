using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Infrastructure;
using Vodovoz.Settings.Edo;

namespace Edo.Transfer.Routine
{
	public class TransferTimeoutWorker : TimerBackgroundServiceBase
	{
		private readonly IEdoTransferSettings _edoTransferSettings;
		private readonly StaleTransferSender _staleTransferSender;
		private readonly TimeSpan _transferTaskTimeoutCheckIntervalSecond;

		public TransferTimeoutWorker(IEdoTransferSettings edoTransferSettings, StaleTransferSender staleTransferSender)
		{
			_edoTransferSettings = edoTransferSettings ?? throw new ArgumentNullException(nameof(edoTransferSettings));
			_staleTransferSender = staleTransferSender ?? throw new ArgumentNullException(nameof(staleTransferSender));

			_transferTaskTimeoutCheckIntervalSecond = TimeSpan.FromSeconds(_edoTransferSettings.TransferTaskTimeoutCheckIntervalSecond);
		}

		protected override TimeSpan Interval => _transferTaskTimeoutCheckIntervalSecond;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			await _staleTransferSender.SendStaleTasksAsync(stoppingToken);
		}
	}
}
