using Edo.Admin;
using Edo.Contracts.Messages.Events;
using Edo.Problems;
using Edo.Transport;
using EdoService.Library.Factories;
using MassTransit;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using QS.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Controllers;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Extensions;
using VodovozBusiness.Errors.Edo;
using VodovozBusiness.Nodes;
using VodovozBusiness.Services.Edo;
using DocumentContainerType = Vodovoz.Core.Domain.Documents.DocumentContainerType;
using EdoContainer = Vodovoz.Domain.Orders.Documents.EdoContainer;
using IOrderRepository = Vodovoz.EntityRepositories.Orders.IOrderRepository;
using Order = Vodovoz.Domain.Orders.Order;

namespace EdoService.Library
{
	public class EdoService : IEdoService
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IOrderRepository _orderRepository;
		private readonly IEdoRepository _edoRepository;
		private readonly IGenericRepository<ReceiptEdoTask> _receiptRepository;
		private readonly MessageService _messageService;
		private readonly IUserService _userService;
		private readonly EdoCancellationService _edoCancellationService;
		private readonly IGenericRepository<FormalEdoRequest> _edoRequestRepository;
		private readonly ICounterpartyEdoAccountEntityController _counterpartyEdoAccountEntityController;
		private readonly IEdoRequestCreatedEventPublisher _edoRequestCreatedEventPublisher;
		private readonly IBus _bus;
		private readonly IEnumerable<IInformalEdoRequestFactory> _requestFactories;

		private static EdoDocFlowStatus[] _successfulEdoStatuses => new[]
		{
			EdoDocFlowStatus.Succeed,
			EdoDocFlowStatus.InProgress
		};

		private static EdoDocumentStatus[] _resendableEdoDocumentStatuses => new[]
		{
			EdoDocumentStatus.Cancelled,
			EdoDocumentStatus.Error
		};

		public EdoService(
			IUnitOfWorkFactory uowFactory,
			IOrderRepository orderRepository,
			IGenericRepository<ReceiptEdoTask> receiptRepository,
			IEdoRepository edoRepository,
			MessageService messageService,
			IUserService userService,
			EdoCancellationService edoCancellationService,
			IGenericRepository<FormalEdoRequest> edoRequestRepository,
			ICounterpartyEdoAccountEntityController counterpartyEdoAccountEntityController,
			IEdoRequestCreatedEventPublisher edoRequestCreatedEventPublisher,
			IBus bus,
			IEnumerable<IInformalEdoRequestFactory> requestFactories
			)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_receiptRepository = receiptRepository ?? throw new ArgumentNullException(nameof(receiptRepository));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
			_userService = userService ?? throw new ArgumentNullException(nameof(userService));
			_edoCancellationService = edoCancellationService ?? throw new ArgumentNullException(nameof(edoCancellationService));
			_edoRequestRepository = edoRequestRepository ?? throw new ArgumentNullException(nameof(edoRequestRepository));
			_counterpartyEdoAccountEntityController =
				counterpartyEdoAccountEntityController ?? throw new ArgumentNullException(nameof(counterpartyEdoAccountEntityController));
			_edoRequestCreatedEventPublisher = edoRequestCreatedEventPublisher
				?? throw new ArgumentNullException(nameof(edoRequestCreatedEventPublisher));
			_bus = bus ?? throw new ArgumentNullException(nameof(bus));
			_requestFactories = requestFactories ?? throw new ArgumentNullException(nameof(requestFactories));
		}

		public Result ResendEdoDocumentForOrder(OrderEntity order)
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Ставим документ в очередь на переотправку в ЭДО"))
			{
				return ResendEdoDocument(uow, order);
			}
		}

		public Result ResendEdoDocumentForOrder(int taskId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Ставим документ в очередь на переотправку в ЭДО"))
			{
				var order = GetOrderByTaskId(uow, taskId);
				if(order is null)
				{
					return Result.Failure(EdoErrors.HasProblem);
				}

				return ResendEdoDocument(uow, order);
			}
		}

		private Result ResendEdoDocument(IUnitOfWork uow, OrderEntity order)
		{
			if(order.IsUndeliveredStatus)
			{
				return Result.Failure(EdoErrors.IsUndeliveredOrder);
			}

			var edoTask = GetCancelledEdoTaskForResend(uow, order);
			if(edoTask is null)
			{
				return Result.Failure(EdoErrors.NoCancelledEdoTaskForResend);
			}

			bool hasDocflow = HasDocflow(uow, edoTask);
			bool hasCancelledDocflow = HasCancelledDocflow(uow, edoTask);

			if(hasCancelledDocflow) // Есть ДО
			{
				if(EdoTaskHasBeenCancelled(uow, edoTask))
				{
					ResendDocumentForCancelledEdoTask(uow, order, edoTask);
				}
				else // 3.2 Если задачу можем отменить на нашей стороне и ДО аннулирован
				{
					CancelEdoTaskWithReason(uow, edoTask);
					ResendDocumentForCancelledEdoTask(uow, order, edoTask);
				}
			}
			else if(!hasDocflow) // Нет ДО
			{
				if(edoTask.Status is EdoTaskStatus.Problem)
				{
					ResendNewTaskDocument(edoTask.Id); // 4.
				}
				else if(EdoTaskHasBeenCancelled(uow, edoTask))
				{
					ResendDocumentForCancelledEdoTask(uow, order, edoTask); // 1. Задача уже отменена, можно переотправлять 
				}
				else
				{
					// 2. Задача есть, но она не отменена, нужно отменить и переотправить
					CancelEdoTaskWithReason(uow, edoTask);
					ResendDocumentForCancelledEdoTask(uow, order, edoTask);
				}
			}
			else
			{
				CreateEventForEdoTaskCancellation(edoTask); // 3.1 Отправляем запрос на аннулирование, нужно отобразить пользователю, что задача поставлена на отмену, и после отмены можно будет переотправить

				return Result.Failure(EdoErrors.CreateTaskPendingCancellation(edoTask.Id));
			}

			uow.Commit();

			return Result.Success();
		}

		private bool HasCancelledDocflow(IUnitOfWork uow, OrderEdoTask edoTask)
		{
			var orderDocument = uow.Session.QueryOver<OrderEdoDocument>()
				.Where(x => x.DocumentTaskId == edoTask.Id)
				.SingleOrDefault();

			if(CanResendEdoDocument(orderDocument.Status))
			{
				return true;
			}

			return false;
		}

		private bool HasDocflow(IUnitOfWork uow, OrderEdoTask edoTask)
		{
			var orderDocument = uow.Session.QueryOver<OrderEdoDocument>()
				.Where(x => x.DocumentTaskId == edoTask.Id)
				.SingleOrDefault();

			if(orderDocument is null)
			{
				return true;
			}

			return false;
		}

		private void ResendDocumentForCancelledEdoTask(IUnitOfWork uow, OrderEntity order, OrderEdoTask edoTask)
		{
			var productCodes = TrueMarkProductCodeFactory.CreateAutoCodesFromCancelledTask(edoTask);

			var request = ManualEdoRequestFactory.Create(order, productCodes);

			uow.Save(request);
			uow.Save(edoTask);

			_edoRequestCreatedEventPublisher.Publish(request.Id, "Ручная переотправка документов ЭДО")
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();
		}

		private OrderEntity GetOrderByTaskId(IUnitOfWork uow, int taskId)
		{
			var edoTask = uow.Session.Get<DocumentEdoTask>(taskId);
			return edoTask?.FormalEdoRequest?.Order;
		}

		public bool CanResendEdoDocument(EdoDocumentStatus? status) => status.HasValue
			&& _resendableEdoDocumentStatuses.Contains(status.Value);

		/// <summary>
		/// Получает отмененную ЭДО задачу для переотправки документа
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="order">Заказ</param>
		/// <returns>Отмененная ЭДО задача с маркировонной продукцией, или заказ без КМ, или null, если нет подходящей</returns>
		private OrderEdoTask GetCancelledEdoTaskForResend(IUnitOfWork uow, OrderEntity order)
		{
			if(order is null)
			{
				return null;
			}

			var edoTasks = _edoRepository.GetEdoTaskByOrder(uow, order.Id);
			if(!edoTasks.Any())
			{
				return null;
			}

			var orderItems = _orderRepository.GetOrderItems(uow, order.Id);
			var hasMarkedProducts = orderItems.Any(x => x.Nomenclature.IsAccountableInTrueMark);
			if(!hasMarkedProducts)
			{
				return edoTasks.FirstOrDefault();
			}

			var cancelledEdoTaskWithRejectedCodes = edoTasks
				.Where(x => x.Status is EdoTaskStatus.Cancelled)
				.Where(x => x.FormalEdoRequest.ProductCodes.Any(c =>
					c.SourceCodeStatus is SourceProductCodeStatus.Rejected 
					&& c.ResultCode != null))
				.ToList(); 

			return cancelledEdoTaskWithRejectedCodes.FirstOrDefault();
		}

		/// <summary>
		/// Проверяет, была ли отменена ЭДО задача для переотправки документа (при наличии маркированной продукции в заказе)
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="edoTask">ЭДО задача</param>
		/// <returns>True, если задача была отменена или в заказе нет КМ, False в противном случае</returns>
		private bool EdoTaskHasBeenCancelled(IUnitOfWork uow, OrderEdoTask edoTask)
		{
			var order = edoTask.FormalEdoRequest?.Order;
			var orderItems = _orderRepository.GetOrderItems(uow, order.Id);
			var hasMarkedProducts = orderItems.Any(x => x.Nomenclature.IsAccountableInTrueMark);

			if(!hasMarkedProducts)
			{
				return true;
			}

			var cancelledEdoTaskWithRejectedCodes = edoTask.Status is EdoTaskStatus.Cancelled 
				&& edoTask.FormalEdoRequest.ProductCodes.Any(c =>
				c.SourceCodeStatus is SourceProductCodeStatus.Rejected
				&& c.ResultCode != null);

			return cancelledEdoTaskWithRejectedCodes;
		}

		public virtual void SetNeedToResendEdoDocumentForOrder<T>(T entity, DocumentContainerType type) where T : IDomainObject
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Ставим документ в очередь на переотправку в ЭДО"))
			{
				var edoDocumentsActions = UpdateEdoDocumentAction(uow, entity, type);

				if(type is DocumentContainerType.Upd)
				{
					var orderLastTrueMarkDocument = uow.GetAll<TrueMarkDocument>()
						.Where(x => x.Order.Id == entity.GetId())
						.OrderByDescending(x => x.CreationDate)
						.FirstOrDefault();

					if(orderLastTrueMarkDocument != null
						&& orderLastTrueMarkDocument.Type != TrueMarkDocument.TrueMarkDocumentType.WithdrawalCancellation)
					{
						edoDocumentsActions.IsNeedToCancelTrueMarkDocument = true;
					}

					var edoTask =
						uow
							.GetAll<BulkAccountingEdoTask>()
							.FirstOrDefault(x => x.FormalEdoRequest.Order.Id == entity.Id);
					
					if(edoTask != null)
					{
						edoTask.Status = EdoTaskStatus.New;
						uow.Save(edoTask);
					}
				}

				uow.Save(edoDocumentsActions);
				uow.Commit();
			}
		}

		private OrderEdoTrueMarkDocumentsActions UpdateEdoDocumentAction(IUnitOfWork uow, IDomainObject entity, DocumentContainerType type)
		{
			var restriction = GetRestrictionByType(entity, type);

			var edoDocumentsAction = uow.GetAll<OrderEdoTrueMarkDocumentsActions>()
					.Where(restriction)
					.FirstOrDefault() ?? new OrderEdoTrueMarkDocumentsActions();

			FillEdoDocumentsActionByType(edoDocumentsAction, entity, type);

			if(type is DocumentContainerType.Upd)
			{
				edoDocumentsAction.IsNeedToResendEdoUpd = true;
			}
			else
			{
				edoDocumentsAction.IsNeedToResendEdoBill = true;
			}

			edoDocumentsAction.Created = DateTime.Now;

			return edoDocumentsAction;
		}

		private void FillEdoDocumentsActionByType(OrderEdoTrueMarkDocumentsActions edoDocumentsAction, IDomainObject entity, DocumentContainerType type)
		{
			switch(type)
			{
				case DocumentContainerType.Bill:
				case DocumentContainerType.Upd:
					edoDocumentsAction.Order = (Order)entity;
					break;
				case DocumentContainerType.BillWSForDebt:
					edoDocumentsAction.OrderWithoutShipmentForDebt = (OrderWithoutShipmentForDebt)entity;
					break;
				case DocumentContainerType.BillWSForPayment:
					edoDocumentsAction.OrderWithoutShipmentForPayment = (OrderWithoutShipmentForPayment)entity;
					break;
				case DocumentContainerType.BillWSForAdvancePayment:
					edoDocumentsAction.OrderWithoutShipmentForAdvancePayment = (OrderWithoutShipmentForAdvancePayment)entity;
					break;
				default:
					throw new NotImplementedException($"Не поддерживаемый тип {type.GetEnumDisplayName()}");
			}
		}

		private Expression<Func<OrderEdoTrueMarkDocumentsActions, bool>> GetRestrictionByType(IDomainObject entity, DocumentContainerType type)
		{
			switch(type)
			{
				case DocumentContainerType.Bill:
				case DocumentContainerType.Upd:
					return x => x.Order.Id == entity.GetId();
				case DocumentContainerType.BillWSForDebt:
					return x => x.OrderWithoutShipmentForDebt.Id == entity.GetId();
				case DocumentContainerType.BillWSForPayment:
					return x => x.OrderWithoutShipmentForPayment.Id == entity.GetId();
				case DocumentContainerType.BillWSForAdvancePayment:
					return x => x.OrderWithoutShipmentForAdvancePayment.Id == entity.GetId();
				default:
					throw new NotImplementedException($"Не поддерживаемый тип {type.GetEnumDisplayName()}");
			}
		}

		public Result ValidateEdoContainers(IList<EdoContainer> edoContainers)
		{
			var errors = new List<Error>();

			foreach(var edoContainer in edoContainers)
			{
				if(_successfulEdoStatuses.Contains(edoContainer.EdoDocFlowStatus))
				{
					errors.Add(EdoErrors.CreateAlreadySuccefullSended(edoContainer));
				}
			}

			if(errors.Any())
			{
				return Result.Failure(errors);
			}

			return Result.Success();
		}

		public Result ValidateEdoOrderDocument(IUnitOfWork uow, OrderEdoDocument document)
		{
			if(document is null)
			{
				return Result.Failure(EdoErrors.HasProblem);
			}

			var order = _orderRepository.GetOrderByOrderEdoDocumentId(uow, document.Id);

			if(order is null)
			{
				return Result.Failure(EdoErrors.HasProblem);
			}

			var errors = new List<Error>();

			if(!_resendableEdoDocumentStatuses.Contains(document.Status))
			{
				errors.Add(EdoErrors.CreateResendableEdoDocumentStatuses(order.Id, _resendableEdoDocumentStatuses));
			}

			return errors.Any() ? Result.Failure(errors) : Result.Success();
		}

		public Result ValidateOrderForDocument(OrderEntity order, DocumentContainerType type)
		{
			var errors = new List<Error>();

			if(order.OrderPaymentStatus is OrderPaymentStatus.Paid)
			{
				errors.Add(EdoErrors.CreateAlreadyPaidUpd(order.Id, type));
			}

			if(errors.Any())
			{
				return Result.Failure(errors);
			}

			return Result.Success();
		}

		public Result ValidateOrderForDocumentType(OrderEntity order, EdoDocumentType type)
		{
			var errors = new List<Error>();

			if(order.OrderPaymentStatus is OrderPaymentStatus.Paid)
			{
				errors.Add(EdoErrors.CreateAlreadyPaidUpd(order.Id, type));
			}

			if(errors.Any())
			{
				return Result.Failure(errors);
			}

			return Result.Success();
		}

		public Result ValidateOutgoingDocument(IUnitOfWork uow, EdoDockflowData dockflowData)
		{
			var type = dockflowData.EdoDocumentType.Value;

			if(dockflowData.OrderId.HasValue == false)
			{
				return Result.Failure(EdoErrors.HasProblem);
			}

			if(dockflowData.DocFlowId.HasValue == false)
			{
				return Result.Failure(EdoErrors.HasProblem);
			}

			var order = _orderRepository.GetOrder(uow, dockflowData.OrderId.Value);

			var ValidateOrderForDocumentTypeResult = ValidateOrderForDocumentType(order, type);

			if(ValidateOrderForDocumentTypeResult.IsFailure)
			{
				return ValidateOrderForDocumentTypeResult;
			}

			return Result.Success();
		}

		public Result SendDocumentTaskCreatedEvent(EdoTask edoTask)
		{
			_messageService.PublishSendDocumentTaskCreatedEvent(edoTask.Id)
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();
			
			return Result.Success();
		}

		public void CancelOldEdoOffers(IUnitOfWork unitOfWork, Order order)
		{
			var containersToRevokeStatuses = new EdoDocFlowStatus[]
			{
				EdoDocFlowStatus.Succeed
			};

			var orderEdoContainers = _orderRepository
				.GetEdoContainersByOrderId(unitOfWork, order.Id)
				.Where(ooec => containersToRevokeStatuses.Contains(ooec.EdoDocFlowStatus));

			var restriction = GetRestrictionByType(order, DocumentContainerType.Bill);

			var edoDocumentsAction = unitOfWork.GetAll<OrderEdoTrueMarkDocumentsActions>()
				.Where(restriction)
				.FirstOrDefault() ?? new OrderEdoTrueMarkDocumentsActions();

			FillEdoDocumentsActionByType(edoDocumentsAction, order, DocumentContainerType.Bill);

			edoDocumentsAction.IsNeedOfferCancellation = true;

			unitOfWork.Save(edoDocumentsAction);

			unitOfWork.Commit();
		}

		public Result ValidateOrderForOrderDocument(EdoDocFlowStatus status)
		{
			var errors = new List<Error>();

			if(status is EdoDocFlowStatus.InProgress 
				|| status is EdoDocFlowStatus.Succeed)
			{
				errors.Add(EdoErrors.AlreadySuccefullSended);
			}

			if(errors.Any())
			{
				return Result.Failure(errors);
			}

			return Result.Success();
		}

		public void ResendEdoOrderDocumentForOrder(Order order, OrderDocumentType type)
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Публикация неформализованной заявки ЭДО"))
			{
				var informalRequest = uow.GetAll<InformalEdoRequest>()
					.FirstOrDefault(r => r.Order.Id == order.Id && r.OrderDocumentType == type);
				
				if(informalRequest is null)
				{
					var factory = _requestFactories.FirstOrDefault(f => f.CanCreateFor(type))
						?? throw new NotSupportedException($"Не найден фабричный метод для типа документа {type}");

					informalRequest = factory.Create(order);
					uow.Save(informalRequest);
				}

				uow.Commit();

                _messageService.PublishInformalEdoRequestCreatedEvent(informalRequest.Id)
                    .GetAwaiter().GetResult();
			}
		}

		public async Task<Result> ResendReceiptDocument(
			int receiptEdoTaskId,
			CancellationToken cancellationToken = default)
		{	
			using(var uow = _uowFactory.CreateWithoutRoot("Переотправка чека"))
			{
				var receiptTask = uow.Session.Get<ReceiptEdoTask>(receiptEdoTaskId);
				if(receiptTask is null)
				{
					return Result.Failure(EdoErrors.HasProblem);
				}

				var order = receiptTask.FormalEdoRequest?.Order;
				if(order is null)
				{
					return Result.Failure(EdoErrors.HasProblem);
				}

				var canResendResult = CanResendReceipt(receiptTask);
				if(canResendResult.IsFailure)
				{
					return canResendResult;
				}

				receiptTask.ReceiptStatus = EdoReceiptStatus.New;

				var productCodes = new ObservableList<TrueMarkProductCode>(
					receiptTask.Items.Select(x => x.ProductCode)
				);

				var request = ManualEdoRequestFactory.Create(order, productCodes);

				CancelEdoTaskWithReason(uow, receiptTask);

				await uow.SaveAsync(request, cancellationToken: cancellationToken);
				await uow.SaveAsync(receiptTask, cancellationToken: cancellationToken);
				await uow.CommitAsync(cancellationToken);

				await _edoRequestCreatedEventPublisher.Publish(request.Id, "Ручная переотправка чека", cancellationToken);

				return Result.Success();
			}
		}

		private void CreateEventForEdoTaskCancellation(EdoTask edoTask)
		{
			var message = new RequestDocflowCancellationEvent
			{
				TaskId = edoTask.Id,
				Reason = $"Новая ручная переотправка пользователем {_userService.GetCurrentUser().Name}"
			};

			_bus.Publish(message)
				.GetAwaiter()
				.GetResult();
		}

		private void CancelEdoTaskWithReason(IUnitOfWork uow, EdoTask edoTask)
		{
			var cancellationReason = $"Новая ручная переотправка пользователем {_userService.GetCurrentUser().Name}";
			_edoCancellationService.CancelTask(edoTask.Id, cancellationReason, true, uow: uow).GetAwaiter().GetResult();
		}

		private Result CanResendReceipt(ReceiptEdoTask receiptTask)
		{
			var errors = new List<Error>();

			if(receiptTask.Status is EdoTaskStatus.Completed || receiptTask.Status is EdoTaskStatus.InCancellation)
			{
				errors.Add(EdoErrors.CreateCannotResendCompletedTask(receiptTask.Id));
			}

			if(receiptTask.ReceiptStatus is EdoReceiptStatus.Completed)
			{
				errors.Add(EdoErrors.CreateCannotResendCompletedReceipt(receiptTask.Id));
			}

			if(receiptTask.ReceiptStatus is EdoReceiptStatus.SavedToPool)
			{
				errors.Add(EdoErrors.CreateCannotResendReceiptFromSavedToPool(receiptTask.Id));
			}

			if(receiptTask.FiscalDocuments?.Any() == true)
			{
				var hasInvalidDocument = receiptTask.FiscalDocuments.Any(fd =>
					fd.Stage is FiscalDocumentStage.Completed ||
					!string.IsNullOrEmpty(fd.FiscalNumber) ||
					fd.Status is FiscalDocumentStatus.Printed || 
					fd.Status is FiscalDocumentStatus.Completed);

				if(hasInvalidDocument)
				{
					errors.Add(EdoErrors.CreateCannotResendCompletedReceipt(receiptTask.Id));
				}
			}

			return errors.Any() ? Result.Failure(errors) : Result.Success();
		}

		public Result<string> TryResendUpdDocument(int orderEdoTaskId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var request = _edoRequestRepository
					.GetFirstOrDefault(uow, x => x.Task.Id == orderEdoTaskId);

				if(request.Task.TaskType == EdoTaskType.SaveCode)
				{
					var hasOtherRequests = _edoRequestRepository.GetCount(uow, x =>
						x.Order.Id == request.Order.Id
						&& x.Task.Id != orderEdoTaskId
					) > 0;

					if(hasOtherRequests)
					{
						return Result.Failure<string>(new Error("DocumentHasOtherRequests",
							$"Переотправка документа невозможна, т.к. помимо текущего документа" +
							$"по заказу {request.Order.Id} уже есть другая отправка")
						);
					}

					var edoAccount = _counterpartyEdoAccountEntityController.GetDefaultCounterpartyEdoAccountByOrganizationId(
						request.Order.Client,
						request.Order.Contract.Organization.Id
					);

					if(edoAccount.ConsentForEdoStatus != ConsentForEdoStatus.Agree)
					{
						return Result.Failure<string>(new Error("CounterpartyDontAgreeEdoConsent",
							"Переотправка документа невозможна, т.к.у контрагента нет согласия на ЭДО")
						);
					}

					var newRequest = ManualEdoRequestFactory.Create(request.Order);

					uow.Save(newRequest);
					uow.Commit();

					_bus.Publish(new EdoRequestCreatedEvent { Id = newRequest.Id });

					return Result.Success($"Документ отправлен на переформирование. \n" +
						$"Обновите список документов.");
				}

				//Если сюда попадет документ, то значит не правильно выбраны условия доступности действия
				//или не реализована отправка выбранного документами по правильным условиям
				return Result.Failure<string>(new Error("DocumentSendNotSupported",
					$"Для выбранного документа не реализована отправка. \n" +
					$"Обратитесь за технической поддержкой.")
				);
			}
		}

		public void ResendNewTaskDocument(int taskId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var task = uow.Session.Get<OrderEdoTask>(taskId);
				if(task is null)
				{
					return;
				}

				if(task.Status != EdoTaskStatus.Problem)
				{
					return;
				}

				_messageService.PublishResumeEvent(task)
					.GetAwaiter()
					.GetResult();
			}
		}

		public void RehandleNewReceiptDocumentWithProblem(int receiptEdoTaskId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var task = uow.Session.Get<ReceiptEdoTask>(receiptEdoTaskId);
				if(task == null)
				{
					return;
				}

				if(task.Status != EdoTaskStatus.Problem && task.ReceiptStatus != EdoReceiptStatus.New)
				{
					return;
				}

				var message = new ReceiptTaskCreatedEvent
				{
					ReceiptEdoTaskId = receiptEdoTaskId,
				};
				_bus.Publish(message);
			}
		}

		public Result<string> TryResendReceiptDocument(int orderEdoTaskId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var request = _edoRequestRepository
					.GetFirstOrDefault(uow, x => x.Task.Id == orderEdoTaskId);

				var receiptTask = request.Task.As<ReceiptEdoTask>();
				if(receiptTask == null)
				{
					return Result.Failure<string>(new Error("DocumentIsNotReceipt",
						"Переотправка документа невозможна, т.к. текущий документ не является чеком")
					);
				}

				if(receiptTask.ReceiptStatus == EdoReceiptStatus.SavedToPool)
				{
					var hasOtherRequests = _edoRequestRepository.GetCount(uow, x =>
						x.Order.Id == request.Order.Id
						&& x.Task.Id != orderEdoTaskId
					) > 0;

					if(hasOtherRequests)
					{
						return Result.Failure<string>(new Error("DocumentHasOtherRequests",
							$"Переотправка документа невозможна, т.к. помимо текущего документа" +
							$"по заказу {request.Order.Id} уже есть другая отправка")
						);
					}
					var productCodes = TrueMarkProductCodeFactory.CreateAutoCodesFromCancelledTask(receiptTask);

					var newRequest = ManualEdoRequestFactory.Create(request.Order, productCodes);

					uow.Save(newRequest);
					uow.Commit();

					_bus.Publish(new EdoRequestCreatedEvent { Id = newRequest.Id });

					return Result.Success($"Документ отправлен на переформирование. \n" +
						$"Обновите список документов.");
				}

				//Если сюда попадет документ, то значит не правильно выбраны условия доступности действия
				//или не реализована отправка выбранного документами по правильным условиям
				return Result.Failure<string>(new Error("DocumentSendNotSupported",
					$"Для выбранного документа не реализована отправка. \n" +
					$"Обратитесь за технической поддержкой.")
				);
			}
		}
	}
}
