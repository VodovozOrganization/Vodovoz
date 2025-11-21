using Core.Infrastructure;
using Edo.Contracts.Messages.Dto;
using Edo.Contracts.Messages.Events;
using Edo.Docflow.Converters;
using Edo.Docflow.Factories;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Orders.Documents;
using Vodovoz.Core.Domain.Organizations;

namespace Edo.Docflow
{
	public class DocflowHandler : IDisposable
	{
		private readonly ILogger<DocflowHandler> _logger;
		private readonly TransferOrderUpdInfoFactory _transferOrderUpdInfoFactory;
		private readonly OrderUpdInfoFactory _orderUpdInfoFactory;
		private readonly IPaymentRepository _paymentRepository;
		private readonly IBus _messageBus;
		private readonly IUnitOfWork _uow;
		private readonly IPrintableDocumentSaver _printableDocumentSaver;
		private readonly IOrderConverter _orderConverter;
		private readonly IInfoForCreatingEdoEquipmentTransferFactory _equipmentTransferInfoFactory;
		private readonly IEquipmentTransferFileDataFactory _equipmentTransferFileDataFactory;

		public DocflowHandler(
			ILogger<DocflowHandler> logger,
			IUnitOfWork uow,
			TransferOrderUpdInfoFactory transferOrderUpdInfoFactory,
			OrderUpdInfoFactory orderUpdInfoFactory,
			IPaymentRepository paymentRepository,
			IBus messageBus,
			IPrintableDocumentSaver printableDocumentSaver,
			IOrderConverter orderConverter,
			IInfoForCreatingEdoEquipmentTransferFactory equipmentTransferInfoFactory,
			IEquipmentTransferFileDataFactory equipmentTransferFileDataFactory
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_transferOrderUpdInfoFactory = transferOrderUpdInfoFactory ?? throw new ArgumentNullException(nameof(transferOrderUpdInfoFactory));
			_orderUpdInfoFactory = orderUpdInfoFactory ?? throw new ArgumentNullException(nameof(orderUpdInfoFactory));
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
			_printableDocumentSaver = printableDocumentSaver ?? throw new ArgumentNullException(nameof(printableDocumentSaver));
			_orderConverter = orderConverter ?? throw new ArgumentNullException(nameof(orderConverter));
			_equipmentTransferInfoFactory = equipmentTransferInfoFactory ?? throw new ArgumentNullException(nameof(equipmentTransferInfoFactory));
			_equipmentTransferFileDataFactory = equipmentTransferFileDataFactory ?? throw new ArgumentNullException(nameof(equipmentTransferFileDataFactory));
		}

		public async Task HandleTransferDocument(int transferDocumentId, CancellationToken cancellationToken)
		{
			var document = await _uow.Session.GetAsync<TransferEdoDocument>(transferDocumentId, cancellationToken);
			if(document == null)
			{
				_logger.LogWarning("Документ {documentId} не найден", transferDocumentId);
				return;
			}

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
			if(transferTask == null)
			{
				_logger.LogWarning("Задача для документа {documentId} не найдена", transferDocumentId);
				return;
			}

			var transferOrder = await _uow.Session.QueryOver<TransferOrder>()
				.Where(x => x.Id == transferTask.TransferOrderId)
				.SingleOrDefaultAsync(cancellationToken);

			var updInfo = await _transferOrderUpdInfoFactory.CreateUniversalTransferDocumentInfo(transferOrder, cancellationToken);

			var message = new TaxcomDocflowSendEvent
			{
				EdoAccount = transferOrder.Seller.TaxcomEdoSettings.EdoAccount,
				EdoOutgoingDocumentId = document.Id,
				UpdInfo = updInfo
			};
			await _messageBus.Publish(message);
		}

		public async Task HandleOrderDocument(int orderDocumentId, CancellationToken cancellationToken)
		{
			var document = await _uow.Session.GetAsync<OrderEdoDocument>(orderDocumentId, cancellationToken);
			if(document == null)
			{
				_logger.LogWarning("Документ {documentId} не найден", orderDocumentId);
				return;
			}

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
			if(documentTask == null)
			{
				_logger.LogWarning("Задача для документа {documentId} не найдена", orderDocumentId);
				return;
			}

			UniversalTransferDocumentInfo updInfo;
			OrganizationEntity sender;

			switch(documentTask.OrderEdoRequest.Type)
			{
				case CustomerEdoRequestType.Order:
					var order = documentTask.OrderEdoRequest.Order;
					var payments = _paymentRepository.GetOrderPayments(_uow, order.Id)
						.Select(payment => payment.GetFirstParentRefundedPaymentOrCurrent())
						.Where(x => order.DeliveryDate.HasValue && x.Date < order.DeliveryDate.Value.AddDays(1))
						.Distinct()
						.ToList();
					
					sender = order.Contract.Organization;
					updInfo = await _orderUpdInfoFactory.CreateUniversalTransferDocumentInfo(documentTask, payments, cancellationToken);
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
				EdoAccount = sender.TaxcomEdoSettings.EdoAccount,
				EdoOutgoingDocumentId = document.Id,
				UpdInfo = updInfo
			};
			await _messageBus.Publish(message);
		}

		/// <summary>
		/// Обработка документа акта приёма-передачи оборудования
		/// </summary>
		/// <param name="equipmentTransferDocumentId"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task HandleEquipmentTransferDocument(int equipmentTransferDocumentId, CancellationToken cancellationToken)
		{
			var document = await _uow.Session.GetAsync<EquipmentTransferEdoDocument>(equipmentTransferDocumentId, cancellationToken);
			if(document == null)
			{
				_logger.LogWarning($"Документ {equipmentTransferDocumentId} не найден");
				return;
			}

			if(document.Status.IsIn(
				EdoDocumentStatus.InProgress,
				EdoDocumentStatus.Succeed
				))
			{
				_logger.LogError($"Документ {equipmentTransferDocumentId} уже в работе, повторно отправить нельзя.");
				return;
			}

			var edoTask = await _uow.Session.GetAsync<EquipmentTransferEdoTask>(document.DocumentTaskId, cancellationToken);
			if(edoTask == null)
			{
				_logger.LogWarning($"Задача ЭДО акта №{document.DocumentTaskId} не найдена");
				return;
			}

			var order = await _uow.Session.GetAsync<OrderEntity>(edoTask.OrderEdoRequest.Order.Id, cancellationToken);
			if(order == null)
			{
				_logger.LogWarning($"Заказ для акта приёма-передачи №{edoTask.Id} не найден");
				return;
			}

			var sender = order.Contract.Organization;
			if(sender.TaxcomEdoSettings == null)
			{
				_logger.LogWarning($"Настройки ЭДО Такском не найдены для организации отправителя {sender.Id}");
				return;
			}

			try
			{
				if(!(order.OrderDocuments
					.FirstOrDefault(x => x.Type == OrderDocumentType.EquipmentTransfer) is EquipmentTransferDocumentEntity equipmentTransferDocument))
				{
					_logger.LogWarning($"Акт приёма-передачи оборудования для заказа {order.Id} не найден");
					return;
				}

				var pdfBytes = _printableDocumentSaver.SaveToPdf(equipmentTransferDocument);

				var documentDate = equipmentTransferDocument.DocumentDate ?? order.DeliveryDate ?? order.CreateDate ?? DateTime.Now;

				var fileData = _equipmentTransferFileDataFactory.CreateEquipmentTransferFileData(
					order.Id.ToString(),
					documentDate,
					pdfBytes);

				var orderInfo = _orderConverter.ConvertOrderToOrderInfoForEdo(order);

				var equipmentTransferInfo = _equipmentTransferInfoFactory.CreateInfoForCreatingEdoEquipmentTransfer(
					orderInfo,
					fileData);

				var equipmentTransferInfoJson = JsonSerializer.Serialize(equipmentTransferInfo);

				var equipmentTransfermessage = new TaxcomDocflowEquipmentTransferSendEvent
				{
					EdoAccount = sender.TaxcomEdoSettings.EdoAccount,
					EdoOutgoingDocumentId = document.Id,
					DocumentType = EdoDocumentType.EquipmentTransfer,
					DocumentInfo = equipmentTransferInfo
				};

				var messageJson = JsonSerializer.Serialize(equipmentTransfermessage);

				await _messageBus.Publish(equipmentTransfermessage, cancellationToken);

				_logger.LogInformation($"Отправка акта приёма-передачи оборудования документа №{equipmentTransferDocumentId}");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, $"Ошибка при отправке акта приёма-передачи оборудования №{equipmentTransferDocumentId}");
				throw;
			}
		}
		
		public async Task HandleDocflowUpdated(EdoDocflowUpdatedEvent updatedEvent, CancellationToken cancellationToken)
		{
			var documentId = updatedEvent.EdoDocumentId;
			var document = await _uow.Session.GetAsync<OutgoingEdoDocument>(documentId, cancellationToken);
			if(document == null)
			{
				_logger.LogWarning("Документ {documentId} не найден", documentId);
				return;
			}

			var docflowStatus = updatedEvent.DocFlowStatus.TryParseAsEnum<EdoDocFlowStatus>();

			object message = null;

			switch(docflowStatus)
			{
				case EdoDocFlowStatus.InProgress:
					// Тут сделать обработку успешной отправки
					// проверять по документам такскома не по статусу InProgress
					document.Status = EdoDocumentStatus.InProgress;
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

		public async Task HandleTransferDocumentCancellation(int taskId, string reason, CancellationToken cancellationToken)
		{
			var document = await _uow.Session.QueryOver<TransferEdoDocument>()
				.Where(x => x.TransferTaskId == taskId)
				.SingleOrDefaultAsync(cancellationToken);

			if(document == null)
			{
				_logger.LogWarning("Документ для задачи трансфера №{TaskId} не найден.", taskId);
				return;
			}

			if(document.Status == EdoDocumentStatus.Cancelled)
			{
				_logger.LogWarning("Документ для задачи трансфера №{TaskId} уже отменен.", taskId);
				return;
			}

			var transferTask = await _uow.Session.GetAsync<TransferEdoTask>(taskId, cancellationToken);

			var transferOrder = await _uow.Session.QueryOver<TransferOrder>()
				.Where(x => x.Id == transferTask.TransferOrderId)
				.SingleOrDefaultAsync(cancellationToken);

			var message = new TaxcomDocflowRequestCancellationEvent
			{
				EdoAccount = transferOrder.Seller.TaxcomEdoSettings.EdoAccount,
				DocumentId = document.Id,
				CancellationReason = reason
			};

			await _messageBus.Publish(message, cancellationToken);
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
