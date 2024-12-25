using Edo.Contracts.Messages.Events;
using MassTransit;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Edo.Transfer.Routine
{
	public class StaleTransferSender : IDisposable
	{
		private readonly TransferDispatcher _transferDispatcher;
		private readonly IBus _messageBus;
		private readonly IUnitOfWork _uow;

		public StaleTransferSender(IUnitOfWorkFactory uowFactory, TransferDispatcher transferDispatcher, IBus messageBus)
		{
			if(uowFactory is null)
			{
				throw new ArgumentNullException(nameof(uowFactory));
			}

			_transferDispatcher = transferDispatcher ?? throw new ArgumentNullException(nameof(transferDispatcher));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));

			_uow = uowFactory.CreateWithoutRoot();
		}

		public async Task SendStaleTasksAsync(CancellationToken cancellationToken)
		{
			_uow.Session.BeginTransaction();

			var staleTasks = await _transferDispatcher.SendStaleTasksAsync(_uow, cancellationToken);
			var events = staleTasks.Select(x => new TransferTaskReadyToSendEvent { Id = x.Id });

			await _uow.CommitAsync();

			var publishTasks = events.Select(x => _messageBus.Publish(x, cancellationToken));
			await Task.WhenAll(publishTasks);
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
