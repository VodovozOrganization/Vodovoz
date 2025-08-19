using Edo.Contracts.Messages.Events;
using MassTransit;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Edo.Transfer.Routine
{
	public class StaleTransferSender
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly TransferDispatcher _transferDispatcher;
		private readonly IBus _messageBus;

		public StaleTransferSender(IUnitOfWorkFactory unitOfWorkFactory, TransferDispatcher transferDispatcher, IBus messageBus)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_transferDispatcher = transferDispatcher ?? throw new ArgumentNullException(nameof(transferDispatcher));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));

		}

		public async Task SendStaleTasksAsync(CancellationToken cancellationToken)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var staleTasks = await _transferDispatcher.SendStaleTasksAsync(cancellationToken);
				if(!staleTasks.Any())
				{
					return;
				}
				
				await uow.CommitAsync(cancellationToken);
				
				var events = staleTasks.Select(x => new TransferTaskPrepareToSendEvent { TransferTaskId = x.Id });
				var publishTasks = events.Select(x => _messageBus.Publish(x, cancellationToken));
				await Task.WhenAll(publishTasks);
			}
		}
	}
}
