using Core.Infrastructure;
using Edo.Contracts.Messages.Dto;
using Edo.Contracts.Messages.Events;
using Edo.Docflow.Factories;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Organizations;

namespace Edo.Docflow
{
	public class DocflowHandler : IDisposable
	{
		private readonly ILogger<DocflowHandler> _logger;
		private readonly TransferOrderUpdInfoFactory _transferOrderUpdInfoFactory;
		private readonly OrderUpdInfoFactory _orderUpdInfoFactory;
		private readonly IBus _messageBus;
		private readonly IUnitOfWork _uow;

		public DocflowHandler(
			ILogger<DocflowHandler> logger, 
			IUnitOfWorkFactory uowFactory,
			TransferOrderUpdInfoFactory transferOrderUpdInfoFactory,
			OrderUpdInfoFactory orderUpdInfoFactory,
			IBus messageBus
			)
		{
			if(uowFactory is null)
			{
				throw new ArgumentNullException(nameof(uowFactory));
			}

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_transferOrderUpdInfoFactory = transferOrderUpdInfoFactory ?? throw new ArgumentNullException(nameof(transferOrderUpdInfoFactory));
			_orderUpdInfoFactory = orderUpdInfoFactory ?? throw new ArgumentNullException(nameof(orderUpdInfoFactory));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
			_uow = uowFactory.CreateWithoutRoot();
		}

		public async Task HandleTransferDocument(int transferDocumentId, CancellationToken cancellationToken)
		{
			var document = await _uow.Session.GetAsync<TransferEdoDocument>(transferDocumentId, cancellationToken);
			if(document.Status.IsIn(
				EdoDocumentStatus.InProgress,
				EdoDocumentStatus.CompletedWithDivergences,
				EdoDocumentStatus.NotStarted,
				EdoDocumentStatus.Succeed
				))
			{
				_logger.LogError("Документ {documentId} уже в работе, повторно отправить нельзя.");
				return;
			}

			var transferTask = await _uow.Session.GetAsync<TransferEdoTask>(document.TransferTaskId, cancellationToken);
			var transferOrder = await _uow.Session.GetAsync<TransferOrder>(transferTask.TransferOrderId, cancellationToken);
			
			var updInfo = _transferOrderUpdInfoFactory.CreateUniversalTransferDocumentInfo(_uow, transferOrder);

			var message = new TaxcomDocflowSendEvent
			{
				EdoAccount = transferOrder.Seller.TaxcomEdoAccountId,
				EdoOutgoingDocumentId = document.Id,
				UpdInfo = updInfo
			};
			await _messageBus.Publish(message);
		}

		public async Task HandleOrderDocument(int orderDocumentId, CancellationToken cancellationToken)
		{
			var document = await _uow.Session.GetAsync<OrderEdoDocument>(orderDocumentId, cancellationToken);
			
			if(document.Status.IsIn(
				EdoDocumentStatus.InProgress, 
				EdoDocumentStatus.CompletedWithDivergences, 
				EdoDocumentStatus.NotStarted,
				EdoDocumentStatus.Succeed
				))
			{
				_logger.LogError("Документ {documentId} уже в работе, повторно отправить нельзя.");
				return;
			}

			var documentTask = await _uow.Session.GetAsync<DocumentEdoTask>(document.DocumentTaskId, cancellationToken);

			UniversalTransferDocumentInfo updInfo;
			OrganizationEntity sender;

			switch(documentTask.OrderEdoRequest.Type)
			{
				case CustomerEdoRequestType.Order:
					var order = documentTask.OrderEdoRequest.Order;
					sender = order.Contract.Organization;
					updInfo = _orderUpdInfoFactory.CreateUniversalTransferDocumentInfo(documentTask);
					break;
				case CustomerEdoRequestType.OrderWithoutShipmentForAdvancePayment:
				case CustomerEdoRequestType.OrderWithoutShipmentForDebt:
				case CustomerEdoRequestType.OrderWithoutShipmentForPayment:
					throw new NotImplementedException("Не реализована отправка счетов без отгрузки");
				default:
					throw new InvalidOperationException($"Неизвестный тип заявки {documentTask.OrderEdoRequest.Type}");
			}

			var message = new TaxcomDocflowSendEvent
			{
				EdoAccount = sender.TaxcomEdoAccountId,
				EdoOutgoingDocumentId = document.Id,
				UpdInfo = updInfo
			};
			await _messageBus.Publish(message);
		}

		public async Task HandleDocflowUpdated(EdoDocflowUpdatedEvent updatedEvent, CancellationToken cancellationToken)
		{
			var documentId = updatedEvent.EdoDocumentId;
			var document = await _uow.Session.GetAsync<OutgoingEdoDocument>(documentId, cancellationToken);
			var docflowStatus = updatedEvent.DocFlowStatus.TryParseAsEnum<EdoDocFlowStatus>();

			switch(docflowStatus)
			{
				case EdoDocFlowStatus.InProgress:
					// Тут сделать обработку успешной отправки
					// проверять по документам такском а не по статусу InProgress
					break;
				case EdoDocFlowStatus.Succeed:
					var acceptTime = updatedEvent.StatusChangeTime ?? DateTime.Now;
					await AcceptDocument(document, acceptTime, cancellationToken);
					break;
				case EdoDocFlowStatus.NotStarted:

				// уточнение
				// мапим на Problem
				case EdoDocFlowStatus.Warning:
				case EdoDocFlowStatus.CompletedWithDivergences:

				// возникла проблема при проверке на стороне такском
				// мапим на Problem
				case EdoDocFlowStatus.Error:

				// аннулирование
				// мапим на Problem
				case EdoDocFlowStatus.WaitingForCancellation:
				case EdoDocFlowStatus.Cancelled:


				// с остальными ничего не делаем пока

				// неизвестно что и зачем это
				case EdoDocFlowStatus.NotAccepted:
				case EdoDocFlowStatus.Unknown:
				// наш внутренний статус, можно исключить из ЭДО
				case EdoDocFlowStatus.PreparingToSend:
					break;

				default:
					throw new InvalidOperationException($"Неизвестный статус документооборота {updatedEvent.DocFlowStatus}");
			}
		}

		private async Task AcceptDocument(OutgoingEdoDocument document, DateTime acceptTime, CancellationToken cancellationToken)
		{
			document.Status = EdoDocumentStatus.Succeed;
			document.AcceptTime = acceptTime;

			await _uow.SaveAsync(document, cancellationToken: cancellationToken);
			await _uow.CommitAsync();

			switch(document.Type)
			{
				case OutgoingEdoDocumentType.Transfer:
					await NotifyTransferForAcceptDocument(document, cancellationToken);
					break;
				case OutgoingEdoDocumentType.Order:
					await NotifyCustomerForAcceptDocument(document, cancellationToken);
					break;
				default:
					throw new InvalidOperationException($"Неизвестный тип документа {document.Type}");
			}
		}

		private async Task NotifyTransferForAcceptDocument(OutgoingEdoDocument document, CancellationToken cancellationToken)
		{
			var message = new TransferDocumentAcceptedEvent
			{
				DocumentId = document.Id
			};

			await _messageBus.Publish(message, cancellationToken);
		}

		private async Task NotifyCustomerForAcceptDocument(OutgoingEdoDocument document, CancellationToken cancellationToken)
		{
			var message = new OrderDocumentAcceptedEvent
			{
				DocumentId = document.Id
			};

			await _messageBus.Publish(message, cancellationToken);
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
