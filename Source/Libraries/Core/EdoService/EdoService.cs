using Core.Infrastructure;
using Edo.Contracts.Messages.Events;
using Edo.Problems;
using Edo.Problems.Custom.Sources;
using Edo.Transport;
using EdoService.Library.Factories;
using MassTransit;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Extensions;
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
		private readonly IEdoRequestCreatedEventPublisher _edoRequestCreatedEventPublisher;
		private readonly IBus _bus;
		private readonly IEnumerable<IInformalEdoRequestFactory> _requestFactories;
		private readonly EdoProblemRegistrar _edoProblemRegistrar;

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
			IEdoRequestCreatedEventPublisher edoRequestCreatedEventPublisher,
			IBus bus,
			IEnumerable<IInformalEdoRequestFactory> requestFactories,
			EdoProblemRegistrar edoProblemRegistrar
			)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_receiptRepository = receiptRepository ?? throw new ArgumentNullException(nameof(receiptRepository));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_edoRequestCreatedEventPublisher = edoRequestCreatedEventPublisher
				?? throw new ArgumentNullException(nameof(edoRequestCreatedEventPublisher));
			_bus = bus ?? throw new ArgumentNullException(nameof(bus));
			_requestFactories = requestFactories ?? throw new ArgumentNullException(nameof(requestFactories));
			_edoProblemRegistrar = edoProblemRegistrar ?? throw new ArgumentNullException(nameof(edoProblemRegistrar));
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
					return Result.Failure(Vodovoz.Errors.Edo.EdoErrors.HasProblem);
				}

				return ResendEdoDocument(uow, order);
			}
		}

		private Result ResendEdoDocument(IUnitOfWork uow, OrderEntity order)
		{
			if(order.IsUndeliveredStatus)
			{
				return Result.Failure(Vodovoz.Errors.Edo.EdoErrors.IsUndeliveredOrder);
			}

			var documents = _edoRepository.GetOrderEdoDocumentsByOrderId(uow, order.Id);
			if(documents is null || !documents.Any())
			{
				return Result.Failure(Vodovoz.Errors.Edo.EdoErrors.HasProblem);
			}

			foreach(var doc in documents)
			{
				if(!CanResendEdoDocument(doc.Status))
				{
					return Result.Failure(Vodovoz.Errors.Edo.EdoErrors.CreateAlreadySuccefullSended(order, doc));
				}

				var validateResult = ValidateEdoOrderDocument(uow, doc);
				if(validateResult.IsFailure)
				{
					return validateResult;
				}
			}

			var orderItems = _orderRepository.GetOrderItems(uow, order.Id);
			var hasMarkedProducts = orderItems.Any(x => x.Nomenclature.IsAccountableInTrueMark);

			var document = documents.First();
			if(document.Type != OutgoingEdoDocumentType.Order)
			{
				return Result.Failure(Vodovoz.Errors.Edo.EdoErrors.CreateInvalidOutgoingDocumentType(order.Id, document.Type));
			}

			if(hasMarkedProducts && document.CreationTime != null)
			{
				var threeMonthAgo = DateTime.Now.AddMonths(-3);
				if(document.CreationTime < threeMonthAgo)
				{
					return Result.Failure(Vodovoz.Errors.Edo.EdoErrors.CreateResendTimeLimitExceeded(document, order.Id));
				}
			}

			var activeEdoTask = GetActiveEdoTaskForResend(uow, order);
			if(activeEdoTask is null)
			{
				return Result.Failure(Vodovoz.Errors.Edo.EdoErrors.NoActiveEdoTaskForResend);
			}

			var productCodes = new ObservableList<TrueMarkProductCode>(
					activeEdoTask.Items.Select(x => x.ProductCode)
				);

			var request = ManualEdoRequestFactory.Create(order, productCodes);

			activeEdoTask.Status = EdoTaskStatus.Cancelled;

			RegisterProblem(activeEdoTask, CancellationToken.None)
				.GetAwaiter()
				.GetResult();

			uow.Save(request);
			uow.Save(activeEdoTask);
			uow.Commit();

			_edoRequestCreatedEventPublisher.Publish(request.Id, "Ручная переотправка документов ЭДО")
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();

			return Result.Success();
		}

		private OrderEntity GetOrderByTaskId(IUnitOfWork uow, int taskId)
		{
			var edoTask = uow.Session.Get<DocumentEdoTask>(taskId);
			return edoTask?.FormalEdoRequest?.Order;
		}

		public bool CanResendEdoDocument(EdoDocumentStatus? status) => status.HasValue
			&& _resendableEdoDocumentStatuses.Contains(status.Value);

		/// <summary>
		/// Получает активную ЭДО задачу для переотправки документа
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="order">Заказ</param>
		/// <returns>Активная ЭДО задача или null, если нет подходящей</returns>
		private OrderEdoTask GetActiveEdoTaskForResend(IUnitOfWork uow, OrderEntity order)
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
				return edoTasks.FirstOrDefault(x => x.Status != EdoTaskStatus.Cancelled);
			}

			var activeTasksWithAcceptedCodes = edoTasks
				.Where(x => x.Status != EdoTaskStatus.Cancelled)
				.Where(x => x.FormalEdoRequest.ProductCodes.Any(c =>
					c.SourceCodeStatus.IsIn(
						SourceProductCodeStatus.Accepted,
						SourceProductCodeStatus.Changed
					)))
				.ToList(); 

			return activeTasksWithAcceptedCodes.FirstOrDefault();
		}

		public async Task<Result> ResendReceiptFromSavedToPool(
			IUnitOfWork uow,
			int? orderTaskId,
			int orderId,
			CancellationToken cancellationToken = default)
		{
			var tasksResult = await _receiptRepository.GetAsync(
				uow,
				f => f.FormalEdoRequest.Order.Id == orderId
					&& f.Id != orderTaskId, cancellationToken: cancellationToken);

			if(tasksResult.IsFailure) 
			{
				return Result.Failure(tasksResult.Errors);
			}
			var tasks = tasksResult.Value;

			if(tasks.Any(x => x.ReceiptStatus != EdoReceiptStatus.SavedToPool))
			{
				return Result.Failure(Vodovoz.Errors.Edo.EdoErrors.CreateCannotResendReceiptFromSavedToPoolTask(orderId));
			}

			var order = await _orderRepository.GetOrderByIdAsync(uow, orderId, cancellationToken);

			var productCodes = new ObservableList<TrueMarkProductCode>(
				tasks.FirstOrDefault().Items.Select(x => x.ProductCode)
			);

			var newRequest = ManualEdoRequestFactory.Create(order, productCodes);

			await uow.SaveAsync(newRequest, cancellationToken: cancellationToken);
			await uow.CommitAsync(cancellationToken);

			await _edoRequestCreatedEventPublisher.Publish(newRequest.Id, "Ручная переотправка чека из пула", cancellationToken);

			return Result.Success();
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
					errors.Add(Vodovoz.Errors.Edo.EdoErrors.CreateAlreadySuccefullSended(edoContainer));
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
				return Result.Failure(Vodovoz.Errors.Edo.EdoErrors.HasProblem);
			}

			var order = _orderRepository.GetOrderByOrderEdoDocumentId(uow, document.Id);

			if(order is null)
			{
				return Result.Failure(Vodovoz.Errors.Edo.EdoErrors.HasProblem);
			}

			var errors = new List<Error>();

			if(!_resendableEdoDocumentStatuses.Contains(document.Status))
			{
				errors.Add(Vodovoz.Errors.Edo.EdoErrors.CreateResendableEdoDocumentStatuses(order.Id, _resendableEdoDocumentStatuses));
			}

			return errors.Any() ? Result.Failure(errors) : Result.Success();
		}

		public Result ValidateOrderForDocument(OrderEntity order, DocumentContainerType type)
		{
			var errors = new List<Error>();

			if(order.OrderPaymentStatus is OrderPaymentStatus.Paid)
			{
				errors.Add(Vodovoz.Errors.Edo.EdoErrors.CreateAlreadyPaidUpd(order.Id, type));
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
				errors.Add(Vodovoz.Errors.Edo.EdoErrors.CreateAlreadyPaidUpd(order.Id, type));
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
				return Result.Failure(Vodovoz.Errors.Edo.EdoErrors.HasProblem);
			}

			if(dockflowData.DocFlowId.HasValue == false)
			{
				return Result.Failure(Vodovoz.Errors.Edo.EdoErrors.HasProblem);
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
			PublishSendDocumentTaskCreatedEvent(edoTask.Id)
				.ConfigureAwait(false)
				.GetAwaiter()
				.GetResult();
			
			return Result.Success();
		}

		private async Task PublishSendDocumentTaskCreatedEvent(int edoTaskId)
		{
			await _bus.Publish(new DocumentTaskCreatedEvent { Id = edoTaskId });
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
				errors.Add(Vodovoz.Errors.Edo.EdoErrors.AlreadySuccefullSended);
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

				PublishInformalEdoRequestCreatedEvent(informalRequest.Id)
					.GetAwaiter().GetResult();
			}
		}

		private async Task PublishInformalEdoRequestCreatedEvent(int informalRequestId)
		{
			await _bus.Publish(new InformalEdoRequestCreatedEvent { InformalRequestId = informalRequestId });
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
					return Result.Failure(Vodovoz.Errors.Edo.EdoErrors.HasProblem);
				}

				var order = receiptTask.FormalEdoRequest?.Order;
				if(order is null)
				{
					return Result.Failure(Vodovoz.Errors.Edo.EdoErrors.HasProblem);
				}

				var canResendResult = CanResendReceipt(receiptTask);
				if(canResendResult.IsFailure)
				{
					return canResendResult;
				}

				receiptTask.Status = EdoTaskStatus.Cancelled;
				receiptTask.ReceiptStatus = EdoReceiptStatus.New;

				var productCodes = new ObservableList<TrueMarkProductCode>(
					receiptTask.Items.Select(x => x.ProductCode)
				);

				var request = ManualEdoRequestFactory.Create(order, productCodes);

				await RegisterProblem(receiptTask, cancellationToken);

				await uow.SaveAsync(request, cancellationToken: cancellationToken);
				await uow.SaveAsync(receiptTask, cancellationToken: cancellationToken);
				await uow.CommitAsync(cancellationToken);

				await _edoRequestCreatedEventPublisher.Publish(request.Id, "Ручная переотправка чека", cancellationToken);

				return Result.Success();
			}
		}

		private async Task RegisterProblem(OrderEdoTask task, CancellationToken cancellationToken)
		{
			await _edoProblemRegistrar.RegisterCustomProblem<TaskHasBeenCancelledWithReason>(
									task,
									Enumerable.Empty<EdoTaskItem>(),
									cancellationToken);
		}

		private Result CanResendReceipt(ReceiptEdoTask receiptTask)
		{
			var errors = new List<Error>();

			if(receiptTask.Status is EdoTaskStatus.Completed || receiptTask.Status is EdoTaskStatus.InCancellation)
			{
				errors.Add(Vodovoz.Errors.Edo.EdoErrors.CreateCannotResendCompletedTask(receiptTask.Id));
			}

			if(receiptTask.ReceiptStatus is EdoReceiptStatus.Completed)
			{
				errors.Add(Vodovoz.Errors.Edo.EdoErrors.CreateCannotResendCompletedReceipt(receiptTask.Id));
			}

			if(receiptTask.ReceiptStatus is EdoReceiptStatus.SavedToPool)
			{
				errors.Add(Vodovoz.Errors.Edo.EdoErrors.CreateCannotResendReceiptFromSavedToPool(receiptTask.Id));
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
					errors.Add(Vodovoz.Errors.Edo.EdoErrors.CreateCannotResendCompletedReceipt(receiptTask.Id));
				}
			}


			return errors.Any() ? Result.Failure(errors) : Result.Success();
		}

		public void RehandleNewUpdDocumentWithProblem(int updEdoTaskId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var task = uow.Session.Get<DocumentEdoTask>(updEdoTaskId);
				if(task is null)
				{
					return;
				}

				if(task.Status != EdoTaskStatus.Problem && task.Stage != DocumentEdoTaskStage.New)
				{
					return;
				}

				var message = new DocumentTaskCreatedEvent 
				{ 
					Id = updEdoTaskId,
				};

				_bus.Publish(message);
			}
		}

		public void RehandleNewReceiptDocumentWithProblem(int receiptEdoTaskId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var task = uow.Session.Get<ReceiptEdoTask>(receiptEdoTaskId);
				if(task is null)
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
	}
}
