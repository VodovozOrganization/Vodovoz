using MassTransit;
using Microsoft.Extensions.Logging;
using NHibernate.Util;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Transfer.Dispatcher
{
	public class TransferEdoHandler : IDisposable
	{
		private readonly IUnitOfWork _uow;
		private readonly ILogger<TransferEdoHandler> _logger;
		private readonly TransferDispatcher _transferDispatcher;
		private readonly IBus _messageBus;

		public TransferEdoHandler(
			ILogger<TransferEdoHandler> logger,
			IUnitOfWorkFactory uowFactory,
			TransferDispatcher transferDispatcher,
			IBus messageBus
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_transferDispatcher = transferDispatcher ?? throw new ArgumentNullException(nameof(transferDispatcher));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
			_uow = uowFactory.CreateWithoutRoot();
		}

		public async Task HandleDocumentTask(int documentEdoTaskId, CancellationToken cancellationToken)
		{
			_uow.Session.BeginTransaction();
			var documentTask = await _uow.Session.GetAsync<DocumentEdoTask>(documentEdoTaskId, cancellationToken);

			var hasAssignedTransferTask = documentTask.TransferEdoRequests.Any(x => x.TransferEdoTask != null);
			if(hasAssignedTransferTask)
			{
				_logger.LogWarning("При первичной обработки на трансфер клиентской задачи №{documentEdoTaskId} " +
					"обнаружены уже обработанные заявки на трансфер. Возможно задача попала на обработку второй раз.",
					documentEdoTaskId);
			}

			var newTransferRequests = documentTask.TransferEdoRequests.Where(x => x.TransferEdoTask == null);
			if(!newTransferRequests.Any())
			{
				_logger.LogError("При первичной обработки на трансфер клиентской задачи №{documentEdoTaskId} " +
					"не обнаружены новые заявки на трансфер.", documentEdoTaskId);
				// зарегистрировать проблему
				return;
			}

			var requestsGroups = newTransferRequests.GroupBy(x => new TransferDirection(x.FromOrganizationId, x.ToOrganizationId));

			var addRequestsTasks = requestsGroups.Select(requestsGroup => _transferDispatcher.AddRequestsToTask(_uow, documentEdoTaskId, requestsGroup, cancellationToken));

			var transferTasks = await Task.WhenAll(addRequestsTasks);
			var sentTasks = transferTasks.Where(x => x.TransferStatus == TransferEdoTaskStatus.InProgress);
			await _uow.CommitAsync();

			var events = sentTasks.Select(x => new TransferTaskReadyToSendEvent { Id = x.Id });
			var publishTasks = events.Select(x => _messageBus.Publish(x, cancellationToken));

			await Task.WhenAll(publishTasks);
		}

		public async Task HandleTransferDocumentAcceptance(int documentId, CancellationToken cancellationToken)
		{
			_uow.Session.BeginTransaction();

			var document = await _uow.Session.GetAsync<TransferEdoDocument>(documentId, cancellationToken);

			if(document == null)
			{
				_logger.LogError("При обработке подтверждения документа №{documentId} не найден документ.", documentId);
			}

			if(document.AcceptTime == null)
			{
				_logger.LogError("Неозможно завершить трансфер, так как документ №{documentId} еще не принят.", documentId);
			}

			var transferTask = await _transferDispatcher.CompleteTransfer(_uow, document, cancellationToken);

			var allComplete = await _transferDispatcher.IsAllTransfersComplete(_uow, transferTask, cancellationToken);

			_uow.Commit();

			if(allComplete)
			{
				var message = new TransferDoneEvent { Id = transferTask.DocumentEdoTaskId };
				await _messageBus.Publish(message, cancellationToken);
			}
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
