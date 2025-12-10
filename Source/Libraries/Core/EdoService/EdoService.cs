using Core.Infrastructure;
using Edo.Transport;
using EdoService.Library.Factories;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Extensions.Observable.Collections.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.TrueMark.TrueMarkProductCodes;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Extensions;
using VodovozBusiness.Nodes;
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
		private readonly MessageService _messageService;
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
			IEdoRepository edoRepository,
			MessageService messageService,
			IEnumerable<IInformalEdoRequestFactory> requestFactories
			)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_messageService = messageService ?? throw new ArgumentNullException(nameof(messageService));
			_requestFactories = requestFactories ?? throw new ArgumentNullException(nameof(requestFactories));
		}

		/// <summary>
		/// Переотправка документа ЭДО по заказу
		/// </summary>
		/// <param name="order"></param>
		/// <param name="docflowId"></param>
		public Result ResendEdoDocumentForOrder(OrderEntity order, Guid docflowId)
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Ставим документ в очередь на переотправку в ЭДО"))
			{
				var errors = new List<Error>();
				var document = _edoRepository.GetOrderEdoDocumentByDocflowId(uow, docflowId);

				if(document == null)
				{
					return Result.Failure(Error.NullValue);
				}

				if(!(document.Type == OutgoingEdoDocumentType.Order))
				{
					errors.Add(Vodovoz.Errors.Edo.EdoErrors.CreateInvalidOutgoingDocumentType(order.Id, document.Type));
				}

				var edoValidateDocumentResult = ValidateEdoOrderDocument(uow, document);

				if(edoValidateDocumentResult.IsFailure)
				{
					return edoValidateDocumentResult;
				}

				var hasMarkedProducts = order.OrderItems.Any(x => x.Nomenclature.IsAccountableInTrueMark);

				if(hasMarkedProducts && document.SendTime.HasValue)
				{
					var threeDaysAgo = DateTime.Now.AddDays(-3);
					if(document.SendTime.Value < threeDaysAgo)
					{
						errors.Add(Vodovoz.Errors.Edo.EdoErrors.CreateResendTimeLimitExceeded(document, order.Id));
					}
				}

				var edoTask = GetActiveEdoTaskForResend(uow, order);

				if(edoTask == null)
				{
					return Result.Failure(Error.NullValue);
				}

				var productCodes = new ObservableList<TrueMarkProductCode>(
					edoTask.Items.Select(x => x.ProductCode)
				);

				var request = CreateManualEdoRequests(order, productCodes);

				uow.Save(request);
				uow.Commit();

				_messageService.PublishEdoRequestCreatedEvent(request.Id)
					.GetAwaiter().GetResult();

				return Result.Success();
			}
		}

		/// <summary>
		/// Получает активную ЭДО задачу для переотправки документа
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="order">Заказ</param>
		/// <returns>Активная ЭДО задача или null, если нет подходящей</returns>
		private OrderEdoTask GetActiveEdoTaskForResend(IUnitOfWork uow, OrderEntity order)
		{
			var edoTasks = _edoRepository.GetEdoTaskByOrderAsync(uow, order.Id);

			if(!edoTasks.Any())
			{
				return null;
			}

			var hasMarkedProducts = order.OrderItems.Any(x => x.Nomenclature.IsAccountableInTrueMark);
			if(!hasMarkedProducts)
			{
				return edoTasks.FirstOrDefault(x => x.Status != EdoTaskStatus.Cancelled);
			}

			var activeTasksWithAcceptedCodes = edoTasks
				.Where(x => x.Status != EdoTaskStatus.Cancelled)
				.Where(x => x.OrderEdoRequest.ProductCodes.Any(c =>
					c.SourceCodeStatus.IsIn(
						SourceProductCodeStatus.Accepted,
						SourceProductCodeStatus.Changed
					)))
				.ToList();

			if(activeTasksWithAcceptedCodes.Count > 1)
			{
				return null;
			}

			return activeTasksWithAcceptedCodes.SingleOrDefault();
		}

		/// <summary>
		/// Создание ручной заявки ЭДО для переотправки документа
		/// </summary>
		/// <param name="order"></param>
		/// <param name="productCodes"></param>
		/// <returns></returns>
		private ManualEdoRequest CreateManualEdoRequests(OrderEntity order, ObservableList<TrueMarkProductCode> productCodes)
		{
			var edoRequest = new ManualEdoRequest
			{
				Type = CustomerEdoRequestType.Order,
				Time = DateTime.Now,
				Source = CustomerEdoRequestSource.Manual,
				DocumentType = EdoDocumentType.UPD,
				Order = order
			};

			if(edoRequest.ProductCodes != null && productCodes != null)
			{
				foreach(var code in productCodes)
				{
					edoRequest.ProductCodes.Add(code);
				}
			}

			return edoRequest;
		}

		public virtual void SetNeedToResendEdoDocumentForOrder<T>(T entity, DocumentContainerType type) where T : IDomainObject
		{
			using(var uow = _uowFactory.CreateWithoutRoot("Ставим документ в очередь на переотправку в ЭДО"))
			{
				var edoDocumentsActions = UpdateEdoDocumentAction(uow, entity, type);

				if(type == DocumentContainerType.Upd)
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
							.FirstOrDefault(x => x.OrderEdoRequest.Order.Id == entity.Id);
					
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
					.FirstOrDefault();

			if(edoDocumentsAction == null)
			{
				edoDocumentsAction = new OrderEdoTrueMarkDocumentsActions();
			}

			FillEdoDocumentsActionByType(edoDocumentsAction, entity, type);

			if(type == DocumentContainerType.Upd)
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

		public Result ValidateEdoOrderDocument(IUnitOfWork uow, OutgoingEdoDocument document)
		{
			var order = _orderRepository.GetOrderByOrderEdoDocumentId(uow, document.Id);

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

			if(order.OrderPaymentStatus == OrderPaymentStatus.Paid)
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

			if(order.OrderPaymentStatus == OrderPaymentStatus.Paid)
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
				return Result.Failure(Error.NullValue);
			}

			if(dockflowData.DocFlowId.HasValue == false)
			{
				return Result.Failure(Error.NullValue);
			}

			var order = _orderRepository.GetOrder(uow, dockflowData.OrderId.Value);

			var ValidateOrderForDocumentTypeResult = ValidateOrderForDocumentType(order, type);

			if(ValidateOrderForDocumentTypeResult.IsFailure)
			{
				return ValidateOrderForDocumentTypeResult;
			}

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

			if(status == EdoDocFlowStatus.InProgress 
				|| status == EdoDocFlowStatus.Succeed)
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
				
				if(informalRequest == null)
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
	}
}
