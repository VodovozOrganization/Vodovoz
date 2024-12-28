using Edo.Contracts.Messages.Events;
using MassTransit;
using NLog;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Edo.Transfer.Routine
{
	public class StaleTransferSender
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly TransferDispatcher _transferDispatcher;
		private readonly IBus _messageBus;

		public StaleTransferSender(IUnitOfWorkFactory uowFactory, TransferDispatcher transferDispatcher, IBus messageBus)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_transferDispatcher = transferDispatcher ?? throw new ArgumentNullException(nameof(transferDispatcher));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));

		}

		public async Task SendStaleTasksAsync(CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{

				uow.OpenTransaction();

				var staleTasks = await _transferDispatcher.SendStaleTasksAsync(uow, cancellationToken);
				if(!staleTasks.Any())
				{
					return;
				}

				await uow.CommitAsync();

				var events = staleTasks.Select(x => new TransferTaskReadyToSendEvent { Id = x.Id });
				var publishTasks = events.Select(x => _messageBus.Publish(x, cancellationToken));
				await Task.WhenAll(publishTasks);
			}
		}
	}
}
