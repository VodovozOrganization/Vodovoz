using Edo.Contracts.Messages.Events;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Docflow
{
	public class DocflowCancellationHandler : IDisposable
	{
		private readonly ILogger<DocflowCancellationHandler> _logger;
		private readonly IPublishEndpoint _publishEndpoint;
		private readonly IUnitOfWork _uow;

		public DocflowCancellationHandler(
			ILogger<DocflowCancellationHandler> logger,
			IUnitOfWork uow,
			IPublishEndpoint publishEndpoint
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
		}

		public async Task HandleDocflowCancellation(int taskId, string reason, CancellationToken cancellationToken)
		{
			var task = await _uow.Session.QueryOver<EdoTask>()
				.Where(x => x.Id == taskId)
				.SingleOrDefaultAsync(cancellationToken);

			switch(task.TaskType)
			{
				case EdoTaskType.Document:
					await CancelOrderDocflow((DocumentEdoTask)task, reason, cancellationToken);
					break;
				case EdoTaskType.Transfer:
					await CancelTransferDocflow((TransferEdoTask)task, reason, cancellationToken);
					break;
				case EdoTaskType.Receipt:
				case EdoTaskType.SaveCode:
				case EdoTaskType.BulkAccounting:
				case EdoTaskType.Withdrawal:
				case EdoTaskType.Tender:
				default:
					_logger.LogWarning("Для задачи типа {EdoTaskType} не может быть документооборота", task.TaskType);
					return;
			}
		}

		private async Task CancelOrderDocflow(DocumentEdoTask documentEdoTask, string reason, CancellationToken cancellationToken)
		{
			var document = await _uow.Session.QueryOver<OrderEdoDocument>()
				.Where(x => x.DocumentTaskId == documentEdoTask.Id)
				.SingleOrDefaultAsync(cancellationToken);

			if(document is null)
			{
				_logger.LogWarning("Документ для задачи №{TaskId} не найден.", documentEdoTask.Id);
				return;
			}

			if(document.Status is EdoDocumentStatus.Cancelled)
			{
				_logger.LogWarning("Документ для задачи №{TaskId} уже отменен.", documentEdoTask.Id);
				return;
			}

			var message = new TaxcomDocflowRequestCancellationEvent
			{
				EdoAccount = documentEdoTask.FormalEdoRequest.Order.Contract.Organization.TaxcomEdoSettings.EdoAccount,
				DocumentId = document.Id,
				CancellationReason = reason
			};

			// Помечаем, что аннулирование запросили мы сами,
			// чтобы ответ Такском не был принят за расхождение
			document.CancellationRequestTime = DateTime.Now;
			await _uow.SaveAsync(document, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);

			await _publishEndpoint.Publish(message, cancellationToken);
		}

		private async Task CancelTransferDocflow(TransferEdoTask transferEdoTask, string reason, CancellationToken cancellationToken)
		{
			var document = await _uow.Session.QueryOver<TransferEdoDocument>()
				.Where(x => x.TransferTaskId == transferEdoTask.Id)
				.SingleOrDefaultAsync(cancellationToken);

			if(document is null)
			{
				_logger.LogWarning("Документ для задачи №{TaskId} не найден.", transferEdoTask.Id);
				return;
			}

			if(document.Status is EdoDocumentStatus.Cancelled)
			{
				_logger.LogWarning("Документ для задачи №{TaskId} уже отменен.", transferEdoTask.Id);
				return;
			}

			var transferOrder = await _uow.Session.QueryOver<TransferOrder>()
				.Where(x => x.Id == transferEdoTask.TransferOrderId)
				.SingleOrDefaultAsync(cancellationToken);

			var message = new TaxcomDocflowRequestCancellationEvent
			{
				EdoAccount = transferOrder.Seller.TaxcomEdoSettings.EdoAccount,
				DocumentId = document.Id,
				CancellationReason = reason
			};

			await _publishEndpoint.Publish(message, cancellationToken);
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
