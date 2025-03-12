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

			object message = null;

			switch(docflowStatus)
			{
				case EdoDocFlowStatus.InProgress:
					// Тут сделать обработку успешной отправки
					// проверять по документам такском а не по статусу InProgress
					break;
				case EdoDocFlowStatus.Succeed:
					var acceptTime = updatedEvent.StatusChangeTime ?? DateTime.Now;
					document.Status = EdoDocumentStatus.Succeed;
					document.AcceptTime = acceptTime;
					switch(document.Type)
					{
						case OutgoingEdoDocumentType.Transfer:
							message = new TransferDocumentAcceptedEvent { DocumentId = document.Id };
							break;
						case OutgoingEdoDocumentType.Order:
							message = new OrderDocumentAcceptedEvent { DocumentId = document.Id };
							break;
						default:
							throw new InvalidOperationException($"Неизвестный тип документа {document.Type}");
					}
					break;
				case EdoDocFlowStatus.NotStarted:

				// уточнение
				// мапим на Problem
				case EdoDocFlowStatus.Warning:
					document.Status = EdoDocumentStatus.Warning;
					switch(document.Type)
					{
						case OutgoingEdoDocumentType.Transfer:
							message = new TransferDocumentProblemEvent { DocumentId = document.Id };
							break;
						case OutgoingEdoDocumentType.Order:
							message = new OrderDocumentProblemEvent { DocumentId = document.Id };
							break;
						default:
							throw new InvalidOperationException($"Неизвестный тип документа {document.Type}");
					}
					break;

				case EdoDocFlowStatus.CompletedWithDivergences:
					document.Status = EdoDocumentStatus.CompletedWithDivergences;
					switch(document.Type)
					{
						case OutgoingEdoDocumentType.Transfer:
							message = new TransferDocumentProblemEvent { DocumentId = document.Id };
							break;
						case OutgoingEdoDocumentType.Order:
							message = new OrderDocumentProblemEvent { DocumentId = document.Id };
							break;
						default:
							throw new InvalidOperationException($"Неизвестный тип документа {document.Type}");
					}
					break;

				// возникла проблема при проверке на стороне такском
				// мапим на Problem
				case EdoDocFlowStatus.Error:
					document.Status = EdoDocumentStatus.Error;
					break;

				// аннулирование
				// мапим на Problem
				case EdoDocFlowStatus.WaitingForCancellation:
					document.Status = EdoDocumentStatus.WaitingForCancellation;
					break;

				case EdoDocFlowStatus.Cancelled:
					document.Status = EdoDocumentStatus.Cancelled;
					switch(document.Type)
					{
						case OutgoingEdoDocumentType.Transfer:
							message = new TransferDocumentCancelledEvent { DocumentId = document.Id };
							break;
						case OutgoingEdoDocumentType.Order:
							message = new OrderDocumentCancelledEvent { DocumentId = document.Id };
							break;
						default:
							throw new InvalidOperationException($"Неизвестный тип документа {document.Type}");
					}
					break;


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

			await _uow.SaveAsync(document, cancellationToken: cancellationToken);
			await _uow.CommitAsync();

			if(message != null)
			{
				await _messageBus.Publish(message, cancellationToken);
			}
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
