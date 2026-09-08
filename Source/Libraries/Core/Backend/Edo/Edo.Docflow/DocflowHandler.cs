using Core.Infrastructure;
using Edo.Contracts.Messages.Dto;
using Edo.Common;
using Edo.Common.Services;
using Edo.Contracts.Messages.Events;
using Edo.Docflow.Factories;
using MassTransit;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using NHibernate;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaxcomEdo.Contracts.Documents;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Organizations;

namespace Edo.Docflow
{
	public class DocflowHandler : IDisposable
	{
		private readonly ILogger<DocflowHandler> _logger;
		private readonly TransferOrderUpdInfoFactory _transferOrderUpdInfoFactory;
		private readonly OrderUpdInfoFactory _orderUpdInfoFactory;
		private readonly IPaymentRepository _paymentRepository;
		private readonly IEdoRepository _edoRepository;
		private readonly IPublishEndpoint _publishEndpoint;
		private readonly IUnitOfWork _uow;
		private readonly IInformalOrderDocumentHandlerFactory _documentHandlerFactory;
		private readonly ITrueMarkCodesValidator _validator;
		private readonly EdoTaskItemTrueMarkStatusProviderFactory _statusProviderFactory;
		private readonly ITrueMarkWaterCodeService _waterCodeService;
		private readonly IGenericRepository<FormalEdoRequest> _requestRepository;
		private readonly IGenericRepository<TaxcomDocflow> _docflowRepository;

		public DocflowHandler(
			ILogger<DocflowHandler> logger,
			IUnitOfWork uow,
			TransferOrderUpdInfoFactory transferOrderUpdInfoFactory,
			OrderUpdInfoFactory orderUpdInfoFactory,
			IPaymentRepository paymentRepository,
			IEdoRepository edoRepository,
			IInformalOrderDocumentHandlerFactory informalOrderDocumentHandlerFactory,
			IPublishEndpoint publishEndpoint,
			ITrueMarkCodesValidator validator,
			EdoTaskItemTrueMarkStatusProviderFactory statusProviderFactory,
			ITrueMarkWaterCodeService waterCodeService,
			IGenericRepository<FormalEdoRequest> requestRepository,
			IGenericRepository<TaxcomDocflow> docflowRepository
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_transferOrderUpdInfoFactory = transferOrderUpdInfoFactory ?? throw new ArgumentNullException(nameof(transferOrderUpdInfoFactory));
			_orderUpdInfoFactory = orderUpdInfoFactory ?? throw new ArgumentNullException(nameof(orderUpdInfoFactory));
			_paymentRepository = paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
			_documentHandlerFactory = informalOrderDocumentHandlerFactory ?? throw new ArgumentNullException(nameof(informalOrderDocumentHandlerFactory));
			_validator = validator ?? throw new ArgumentNullException(nameof(validator));
			_statusProviderFactory = statusProviderFactory ?? throw new ArgumentNullException(nameof(statusProviderFactory));
			_waterCodeService = waterCodeService ?? throw new ArgumentNullException(nameof(waterCodeService));
			_requestRepository = requestRepository ?? throw new ArgumentNullException(nameof(requestRepository));
			_docflowRepository = docflowRepository ?? throw new ArgumentNullException(nameof(docflowRepository));
		}

		public async Task HandleTransferDocument(int transferDocumentId, CancellationToken cancellationToken)
		{
			var document = await _uow.Session.GetAsync<TransferEdoDocument>(transferDocumentId, cancellationToken);
			if(document is null)
			{
				_logger.LogWarning("Документ {documentId} не найден", transferDocumentId);
				return;
			}

			var isNotValidStatus = _edoRepository.GetInProgressOrCompletedStatuses().Contains(document.Status);
			if(isNotValidStatus)
			{
				_logger.LogError("Документ {documentId} уже в работе, повторно отправить нельзя.");
				return;
			}

			var transferTask = await _uow.Session.GetAsync<TransferEdoTask>(document.TransferTaskId, cancellationToken);
			if(transferTask is null)
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
			await _publishEndpoint.Publish(message);
		}

		public async Task HandleOrderDocument(int orderDocumentId, CancellationToken cancellationToken)
		{
			var document = await _uow.Session.GetAsync<OrderEdoDocument>(orderDocumentId, cancellationToken);
			if(document is null)
			{
				_logger.LogWarning("Документ {documentId} не найден", orderDocumentId);
				return;
			}

			var isNotValidStatus = _edoRepository.GetInProgressOrCompletedStatuses().Contains(document.Status); 
			if(isNotValidStatus)
			{
				_logger.LogError("Документ {documentId} уже в работе, повторно отправить нельзя.");
				return;
			}

			var documentTask = await _uow.Session.GetAsync<DocumentEdoTask>(document.DocumentTaskId, cancellationToken);
			if(documentTask is null)
			{
				_logger.LogWarning("Задача для документа {documentId} не найдена", orderDocumentId);
				return;
			}

			if(!await PrepareOrderDocumentForSendingAsync(document, documentTask, cancellationToken))
			{
				return;
			}

			UniversalTransferDocumentInfo updInfo;
			OrganizationEntity sender;

			switch(documentTask.FormalEdoRequest.Type)
			{
				case CustomerEdoRequestType.Order:
					var order = documentTask.FormalEdoRequest.Order;
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
					throw new InvalidOperationException($"Неизвестный тип заявки {documentTask.FormalEdoRequest.Type}");
			}

			var message = new TaxcomDocflowSendEvent
			{
				EdoAccount = sender.TaxcomEdoSettings.EdoAccount,
				EdoOutgoingDocumentId = document.Id,
				UpdInfo = updInfo
			};
			await _publishEndpoint.Publish(message);
		}


		/// <summary>
		/// Возвращает true, если УПД можно отправить, и false, если отправка остановлена или запущено переформирование.
		/// </summary>
		private async Task<bool> PrepareOrderDocumentForSendingAsync(OrderEdoDocument document, DocumentEdoTask task, CancellationToken cancellationToken)
		{
			if(document == null)
			{
				throw new ArgumentNullException(nameof(document));
			}
			if(task == null)
			{
				throw new ArgumentNullException(nameof(task));
			}
			if(task.DocumentType != EdoDocumentType.UPD)
			{
				return true;
			}

			_uow.OpenTransaction();
			// Блокируем и перечитываем состояние: параллельное событие могло уже создать переотправку.
			await _uow.Session.RefreshAsync(task, LockMode.Upgrade, cancellationToken);
			await _uow.Session.RefreshAsync(document, LockMode.Upgrade, cancellationToken);

			if(task.Status == EdoTaskStatus.Completed || task.Status == EdoTaskStatus.Cancelled
				|| task.Status == EdoTaskStatus.InCancellation || task.Stage != DocumentEdoTaskStage.Sending
				|| document.Status == EdoDocumentStatus.Cancelled || document.Status == EdoDocumentStatus.WaitingForCancellation
				|| document.Status == EdoDocumentStatus.Sent || document.Status == EdoDocumentStatus.InProgress
				|| document.Status == EdoDocumentStatus.Succeed || document.Status == EdoDocumentStatus.CompletedWithDivergences)
			{
				await _uow.CommitAsync(cancellationToken);
				return false;
			}

			if(task.Items.Count == 0)
			{
				await _uow.CommitAsync(cancellationToken);
				return true;
			}

			var result = await _validator.ValidateAsync(task, _statusProviderFactory.Create(task), cancellationToken);
			if(result.IsAllValid)
			{
				await _uow.CommitAsync(cancellationToken);
				return true;
			}

			var order = task.FormalEdoRequest.Order;
			if(order.Client.ReasonForLeaving == ReasonForLeaving.Resale
				|| order.Client.ReasonForLeaving == ReasonForLeaving.Tender)
			{
				throw new InvalidOperationException($"УПД задачи №{task.Id} содержит невалидные коды. "
					+ "Автоматическая замена отсканированных кодов перепродажи или тендера кодами из пула не поддерживается.");
			}

			if(order.IsUndeliveredStatus)
			{
				throw new InvalidOperationException($"Переотправка УПД задачи №{task.Id} недоступна для недоставленного заказа.");
			}

			if(document.Status != EdoDocumentStatus.NotStarted
				|| _docflowRepository.GetCount(_uow, x => x.EdoDocumentId == document.Id) > 0)
			{
				throw new InvalidOperationException($"УПД №{document.Id} уже передавался оператору. "
					+ "Перед переформированием требуется завершить его документооборот.");
			}

			if(_requestRepository.GetCount(_uow, x => x.Order.Id == order.Id
				&& x.Id != task.FormalEdoRequest.Id
				&& x.DocumentType == EdoDocumentType.UPD
				&& !(x is WithdrawalEdoRequest)
				&& (x.Task == null || (x.Task.Status != EdoTaskStatus.Cancelled && !(x.Task is SaveCodesEdoTask)))) > 0)
			{
				throw new InvalidOperationException($"По заказу №{order.Id} уже существует другая активная отправка УПД.");
			}

			// Штатный сценарий переформирования: заявка без старых КМ, подбор выполняет обработчик УПД.
			var request = new ManualEdoRequest
			{
				Type = CustomerEdoRequestType.Order,
				Time = DateTime.Now,
				Source = EdoRequestSource.Manual,
				DocumentType = EdoDocumentType.UPD,
				Order = order
			};
			// Сохраняем сразу новую задачу УПД. Её запуск поддерживает штатный воркер повторной обработки новых задач.
			var resendTask = new DocumentEdoTask
			{
				FormalEdoRequest = request,
				DocumentType = EdoDocumentType.UPD,
				FromOrganization = task.FromOrganization,
				ToCustomer = task.ToCustomer,
				Status = EdoTaskStatus.New,
				Stage = DocumentEdoTaskStage.New
			};
			request.Task = resendTask;

			foreach(var item in task.Items)
			{
				await _waterCodeService.DeleteRelatedGroupAndTransportCodesAsync(
					_uow, item.ProductCode.SourceCode, cancellationToken);
				item.ProductCode.SourceCodeStatus = SourceProductCodeStatus.Rejected;
				item.ProductCode.ResultCode = null;
			}

			await _uow.SaveAsync(request, cancellationToken: cancellationToken);
			await _uow.SaveAsync(resendTask, cancellationToken: cancellationToken);
			document.Status = EdoDocumentStatus.Cancelled;
			task.Status = EdoTaskStatus.Cancelled;
			task.EndTime = DateTime.Now;
			task.CancellationReason = "Автоматическое переформирование УПД: невалидные коды ЧЗ перед отправкой";
			await _uow.SaveAsync(task, cancellationToken: cancellationToken);
			await _uow.SaveAsync(document, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);

			_logger.LogWarning("УПД {DocumentId} отменен до отправки. Создана задача переформирования {TaskId}",
				document.Id, resendTask.Id);
			try
			{
				await _publishEndpoint.Publish(new DocumentTaskCreatedEvent { Id = resendTask.Id }, cancellationToken);
			}
			catch(Exception ex)
			{
				// Задача уже сохранена: её повторно запустит NewEdoTasksResendWorker.
				_logger.LogError(ex, "Не удалось опубликовать запуск задачи УПД {TaskId}. "
					+ "Задача сохранена в статусе Новая для штатного повторного запуска", resendTask.Id);
			}
			return false;
		}

		/// <summary>
		/// Обработка неформализованного документа заказа 
		/// </summary>
		/// <param name="orderDocumentId"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task HandleInformalOrderDocument(int orderDocumentId, OrderDocumentFileData orderDocumentFileData, CancellationToken cancellationToken)
		{
			var document = await _uow.Session.GetAsync<OutgoingInformalEdoDocument>(orderDocumentId, cancellationToken);
			if(document is null)
			{
				_logger.LogWarning($"Документ {orderDocumentId} не найден");
				return;
			}

			var isNotValidStatus = _edoRepository.GetInProgressOrCompletedStatuses().Contains(document.Status);
			if(isNotValidStatus)
			{
				_logger.LogError($"Документ {orderDocumentId} уже в работе, повторно отправить нельзя.");
				return;
			}

			if(orderDocumentFileData.Image is null || orderDocumentFileData.Image.Length == 0)
			{
				_logger.LogWarning($"Файл документа {orderDocumentId} пустой.");
				return;
			}

			var informalEdoRequest = await _uow.Session.Query<InformalEdoRequest>()
				.Where(x => x.Task.Id == document.InformalDocumentTaskId)
				.FirstOrDefaultAsync(cancellationToken: cancellationToken);

			if(informalEdoRequest is null)
			{
				_logger.LogWarning($"Заявка на неформальный ЭДО для задачи с идентификатором {document.InformalDocumentTaskId} не найдена.");
				return;
			}

			var edoTask = informalEdoRequest.Task;
			if(edoTask is null)
			{
				_logger.LogWarning($"Задача ЭДО акта №{document.InformalDocumentTaskId} не найдена");
				return;
			}

			var order = await _uow.Session.GetAsync<OrderEntity>(orderDocumentFileData.OrderId, cancellationToken);
			if(order is null)
			{
				_logger.LogWarning($"Заказ №{orderDocumentFileData.OrderId} не найден");
				return;
			}

			var sender = order.Contract.Organization;
			if(sender.TaxcomEdoSettings is null)
			{
				_logger.LogWarning($"Настройки ЭДО Такском не найдены для организации отправителя {sender.Id}");
				return;
			}

			try
			{
				var informalDocument = order.OrderDocuments
					.FirstOrDefault(x => x.Type == informalEdoRequest.OrderDocumentType);

				if(informalDocument is null)
				{
					_logger.LogWarning($"Документ заказа {informalEdoRequest.OrderDocumentType} для заказа {order.Id} не найден");
					return;
				}

				var handler = _documentHandlerFactory.GetHandler(informalDocument.Type);
				var documentInfo = await handler.ProcessDocument(order, orderDocumentFileData, informalDocument.Id, cancellationToken);

				var message = new TaxcomDocflowInformalDocumentSendEvent
				{
					EdoAccount = sender.TaxcomEdoSettings.EdoAccount,
					EdoOutgoingDocumentId = document.Id,
					DocumentInfo = documentInfo
				};

				await _publishEndpoint.Publish(message, cancellationToken);

				_logger.LogInformation($"Отправка неформализованного документа заказа №{orderDocumentId}");
			}
			catch(NotSupportedException ex)
			{
				_logger.LogWarning(ex, $"Неподдерживаемый тип документа: {informalEdoRequest.OrderDocumentType}");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, $"Ошибка при отправке неформализованного документа заказа №{orderDocumentId}");
				throw;
			}
		}

		public async Task HandleDocflowUpdated(EdoDocflowUpdatedEvent updatedEvent, CancellationToken cancellationToken)
		{
			var documentId = updatedEvent.EdoDocumentId;
			var document = await _uow.Session.GetAsync<OutgoingEdoDocument>(documentId, cancellationToken);
			if(document is null)
			{
				_logger.LogWarning("Документ {documentId} не найден", documentId);
				return;
			}

			var docflowStatus = updatedEvent.DocFlowStatus.TryParseAsEnum<EdoDocFlowStatus>();

			object message = null;

			switch(docflowStatus)
			{
				case EdoDocFlowStatus.Sent:
					document.Status = EdoDocumentStatus.Sent;

					if(updatedEvent.DocFlowId.HasValue)
					{
						message = OrderDocumentSentEvent.Create(document.Id);
					}
					break;
				case EdoDocFlowStatus.InProgress:
					document.Status = EdoDocumentStatus.InProgress;

					if(updatedEvent.DocFlowId.HasValue && updatedEvent.IsReceived)
					{
						message = OrderDocumentSentEvent.Create(document.Id);
					}
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
						case OutgoingEdoDocumentType.InformalOrderDocument:
							message = new InformalOrderDocumentAcceptedEvent { DocumentId = document.Id };
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
						case OutgoingEdoDocumentType.InformalOrderDocument:
							message = new InformalOrderDocumentProblemEvent { DocumentId = document.Id };
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
						case OutgoingEdoDocumentType.InformalOrderDocument:
							message = new InformalOrderDocumentProblemEvent { DocumentId = document.Id };
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
						case OutgoingEdoDocumentType.InformalOrderDocument:
							message = new InformalOrderDocumentCancelledEvent { DocumentId = document.Id };
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
			await _uow.CommitAsync(cancellationToken);

			if(message != null)
			{
				await _publishEndpoint.Publish(message, cancellationToken);
			}
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
