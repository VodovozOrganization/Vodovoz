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
		private readonly IUnitOfWork _uow;
		private readonly TransferDispatcher _transferDispatcher;
		private readonly IPublishEndpoint _publishEndpoint;

		public StaleTransferSender(IUnitOfWork uow, TransferDispatcher transferDispatcher, IPublishEndpoint publishEndpoint)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_transferDispatcher = transferDispatcher ?? throw new ArgumentNullException(nameof(transferDispatcher));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));

		}

		public async Task SendStaleTasksAsync(CancellationToken cancellationToken)
		{
			var staleTasks = await _transferDispatcher.SendStaleTasksAsync(cancellationToken);
			if(!staleTasks.Any())
			{
				return;
			}

			// В outbox MassTransit пишем по очереди, не параллельно: строки пишутся через одну сессию NHibernate, а она не потокобезопасна
			foreach(var staleTask in staleTasks)
			{
				await _publishEndpoint.Publish(new TransferTaskPrepareToSendEvent { TransferTaskId = staleTask.Id }, cancellationToken);
			}

			await _uow.CommitAsync(cancellationToken);
		}
	}
}
