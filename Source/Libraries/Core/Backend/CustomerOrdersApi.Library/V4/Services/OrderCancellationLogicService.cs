using CustomerOrdersApi.Library.Factories;
using CustomerOrdersApi.Library.V4.Dto.Orders.CancelOrder;
using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Errors.Orders;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Tools.CallTasks;

namespace CustomerOrdersApi.Library.V4.Services
{
	public class OrderCancellationLogicService : IOrderCancellationLogicService
	{
		private readonly ILogger<OrderCancellationLogicService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IRouteListService _routeListService;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly IPaymentRefundServiceFactory _paymentRefundServiceFactory;

		public OrderCancellationLogicService(
			ILogger<OrderCancellationLogicService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IRouteListItemRepository routeListItemRepository,
			IEmployeeRepository employeeRepository,
			ISubdivisionRepository subdivisionRepository,
			IOrderRepository orderRepository,
			IRouteListService routeListService,
			INomenclatureSettings nomenclatureSettings,
			ICallTaskWorker callTaskWorker,
			IPaymentRefundServiceFactory paymentRefundServiceFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_routeListService = routeListService ?? throw new ArgumentNullException(nameof(routeListService));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_paymentRefundServiceFactory = paymentRefundServiceFactory ?? throw new ArgumentNullException(nameof(paymentRefundServiceFactory));
		}

		/// <summary>
		/// Проверяет, можно ли отменить заказ в текущем статусе
		/// </summary>
		public Result CanCancel(Order order)
		{
			var allowedStatuses = _orderRepository.GetStatusesForTransferOrCancellationOnlineOrder();

			if(order is null)
			{
				return Result.Failure(OrderErrors.NotFound);
			}

			if(!allowedStatuses.Contains(order.OrderStatus))
			{
				return Result.Failure(OrderErrors.CannotCancelOrderInStatus(order.OrderStatus));
			}

			return Result.Success();
		}

		/// <summary>
		/// Применяет отмену заказа
		/// </summary>
		public async Task<Result<string>> ApplyCancellationAsync(
			Guid externalOrderId,
			Source source,
			string transactionId,
			CancellationToken cancellationToken)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Сервис отмены заказа");

			var onlineOrder = await uow.Session.GetAsync<OnlineOrder>(externalOrderId, cancellationToken);
			if(onlineOrder == null)
			{
				var onlineOrderNotFoundError = OnlineOrderErrors.OnlineOrderNotFound;
				_logger.LogWarning("Заказ {ExternalOrderId}: {ErrorMessage}", externalOrderId, onlineOrderNotFoundError.Message);
				return Result.Failure<string>(onlineOrderNotFoundError);
			}

			var order = GetActiveOrder(onlineOrder);
			if(order == null)
			{
				var isOnlineOrderDoesNotHaveALinkedOrderError = OnlineOrderErrors.IsOnlineOrderDoesNotHaveALinkedOrder;
				_logger.LogWarning("Заказ {ExternalOrderId}: {ErrorMessage}", externalOrderId, isOnlineOrderDoesNotHaveALinkedOrderError.Message);
				return Result.Failure<string>(isOnlineOrderDoesNotHaveALinkedOrderError);
			}

			_logger.LogInformation(
				"Начало отмены заказа {OrderId}, статус: {Status}, оплачен: {IsPaid}",
				order.Id,
				order.OrderStatus.GetEnumTitle(),
				IsPaidOnline(onlineOrder));

			var canCancelResult = CanCancel(order);
			if(canCancelResult.IsFailure)
			{
				_logger.LogWarning("Заказ {OrderId}: {ErrorMessage}",
					   order.Id,
					   canCancelResult.Errors.FirstOrDefault().Message);

				return Result.Failure<string>(canCancelResult.Errors);
			}

			Result<string> result;

			if(IsSimpleStatus(order.OrderStatus))
			{
				result = await ApplySimpleCancellationAsync(uow, order, onlineOrder, transactionId, cancellationToken);
			}
			else if(order.OrderStatus is OrderStatus.InTravelList)
			{
				result = await ApplyCancellationFromTravelListAsync(uow, order, onlineOrder, transactionId, cancellationToken);
			}
			else if(order.OrderStatus is OrderStatus.OnLoading or OrderStatus.OnTheWay)
			{
				result = await ApplyCancellationWithUndeliveryAsync(uow, order, onlineOrder, source, transactionId, cancellationToken);
			}
			else
			{
				var error = OrderErrors.UnsupportedOrderStatusForCancellation(order.OrderStatus);
				_logger.LogWarning("Заказ {OrderId}: {ErrorMessage}", order.Id, error.Message);
				return Result.Failure<string>(error);
			}

			if(result.IsSuccess)
			{
				await uow.CommitAsync(cancellationToken);
				_logger.LogInformation("Заказ {OrderId} успешно отменен", order.Id);
			}
			else
			{
				_logger.LogWarning("Отмена заказа {OrderId} не выполнена", order.Id);
			}

			return result;
		}


		private Order GetActiveOrder(OnlineOrder onlineOrder)
		{
			var undeliveryStatuses = _orderRepository.GetUndeliveryStatuses();
			return onlineOrder.Orders.FirstOrDefault(x => !undeliveryStatuses.Contains(x.OrderStatus));
		}

		/// <summary>
		/// Простая отмена заказа (статусы: NewOrder, WaitForPayment, Accepted)
		/// </summary>
		private async Task<Result<string>> ApplySimpleCancellationAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			string transactionId,
			CancellationToken cancellationToken)
		{
			_logger.LogInformation(
				"Простая отмена заказа {OrderId} в статусе {Status}",
				order.Id,
				order.OrderStatus.GetEnumTitle());

			if(IsPaidOnline(onlineOrder))
			{
				var refundResult = await ProcessRefundAsync(uow, order, onlineOrder, transactionId, cancellationToken);
				if(refundResult.IsFailure)
				{
					return Result.Failure<string>(refundResult.Errors);
				}
			}

			order.ChangeStatus(OrderStatus.Canceled);
			onlineOrder.OnlineOrderStatus = OnlineOrderStatus.Canceled;

			await uow.SaveAsync(order, cancellationToken: cancellationToken);
			await uow.SaveAsync(onlineOrder, cancellationToken: cancellationToken);

			_logger.LogInformation("Заказ {OrderId} успешно отменен", order.Id);

			var message = IsPaidOnline(onlineOrder)
				? "Заказ отменен, возврат средств инициирован. Статус возврата можно отслеживать в истории заказов."
				: "Заказ отменен успешно";

			return Result.Success(message);
		}

		/// <summary>
		/// Отмена заказа из маршрутного листа (статус InTravelList)
		/// </summary>
		private async Task<Result<string>> ApplyCancellationFromTravelListAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			string transactionId,
			CancellationToken cancellationToken)
		{
			_logger.LogInformation(
				"Отмена заказа {OrderId} из маршрутного листа",
				order.Id);

			var routeListItem = _routeListItemRepository.GetRouteListItemForOrder(uow, order);

			if(routeListItem == null)
			{
				var error = OrderErrors.RouteListItemNotFound(order.Id);
				_logger.LogWarning("Заказ {OrderId}: {ErrorMessage}", order.Id, error.Message);
				return Result.Failure<string>(error);
			}

			var routeList = routeListItem.RouteList;
			routeList.RemoveAddress(routeListItem);

			if(IsPaidOnline(onlineOrder))
			{
				var refundResult = await ProcessRefundAsync(uow, order, onlineOrder, transactionId, cancellationToken);
				if(refundResult.IsFailure)
				{
					return Result.Failure<string>(refundResult.Errors);
				}
			}

			order.ChangeStatus(OrderStatus.Canceled);
			onlineOrder.OnlineOrderStatus = OnlineOrderStatus.Canceled;

			await uow.SaveAsync(routeList, cancellationToken: cancellationToken);
			await uow.SaveAsync(order, cancellationToken: cancellationToken);
			await uow.SaveAsync(onlineOrder, cancellationToken: cancellationToken);

			_logger.LogInformation(
				"Заказ {OrderId} успешно отменен из маршрутного листа",
				order.Id);

			return Result.Success("Заказ отменен успешно");
		}

		/// <summary>
		/// Отмена заказа с использованием механизма недовоза (статусы OnLoading, OnTheWay)
		/// </summary>
		private async Task<Result<string>> ApplyCancellationWithUndeliveryAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			Source source,
			string transactionId,
			CancellationToken cancellationToken)
		{
			_logger.LogInformation(
				"Отмена заказа {OrderId} из статуса '{Status}' с использованием механизма недовоза",
				order.Id,
				order.OrderStatus.GetEnumTitle());

			var currentUser = await _employeeRepository.GetEmployeeBySourceAsync(uow, source, cancellationToken);
			if(currentUser == null)
			{
				var error = OnlineOrderErrors.EmployeeNotFound(source);
				_logger.LogWarning("Заказ {OrderId}: {ErrorMessage}", order.Id, error.Message);
				return Result.Failure<string>(error);
			}

			var undelivery = CreateUndeliveryForCancellation(uow, order, currentUser);

			order.SetUndeliveredStatus(
				uow,
				_routeListService,
				_nomenclatureSettings,
				_callTaskWorker,
				needCreateDeliveryFreeBalanceOperation: false);

			_logger.LogInformation("Установлен статус недовоза для заказа {OrderId}", order.Id);

			if(IsPaidOnline(onlineOrder))
			{
				var refundResult = await ProcessRefundAsync(uow, order, onlineOrder, transactionId, cancellationToken);
				if(refundResult.IsFailure)
				{
					return Result.Failure<string>(refundResult.Errors);
				}
			}

			onlineOrder.OnlineOrderStatus = OnlineOrderStatus.Canceled;

			await uow.SaveAsync(order, cancellationToken: cancellationToken);
			await uow.SaveAsync(onlineOrder, cancellationToken: cancellationToken);
			await uow.SaveAsync(undelivery, cancellationToken: cancellationToken);

			_logger.LogInformation(
				"Заказ {OrderId} успешно отменен с недовозом. Недовоз: {UndeliveryId}",
				order.Id,
				undelivery.Id);

			var message = IsPaidOnline(onlineOrder)
				? "Заказ отменен успешно, денежные средства вернутся к Вам в течение 10 дней. Срок зависит от банка получателя"
				: "Заказ отменен успешно";

			return Result.Success(message);
		}

		/// <summary>
		/// Обработка возврата средств для оплаченного заказа
		/// </summary>
		private async Task<Result> ProcessRefundAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			string transactionId,
			CancellationToken cancellationToken)
		{
			_logger.LogInformation(
				"Обработка возврата для оплаченного заказа {OrderId}. TransactionId: {TransactionId}",
				order.Id,
				transactionId);

			var refundService = _paymentRefundServiceFactory.GetRefundService(onlineOrder);

			var refundRequest = new RefundRequestDto(
				onlineOrder: onlineOrder,
				transactionId: transactionId,
				amount: onlineOrder.OnlineOrderSum,
				externalOrderId: onlineOrder?.ExternalOrderId.ToString()
			);

			var refundResult = await refundService.ProcessRefundAsync(uow, refundRequest, cancellationToken);

			if(!refundResult.Success)
			{
				_logger.LogWarning(
					"Не удалось выполнить возврат для заказа {OrderId}: {ErrorMessage}",
					order.Id,
					refundResult.ErrorMessage);

				return Result.Failure(OnlineOrderErrors.CantUpdateOrder(refundResult.ErrorMessage));
			}

			if(refundResult.NewPaymentStatus != onlineOrder.OnlineOrderPaymentStatus)
			{
				onlineOrder.OnlineOrderPaymentStatus = refundResult.NewPaymentStatus;
				_logger.LogInformation(
					"Статус оплаты онлайн заказа {OnlineOrderId} обновлен на {NewStatus}",
					onlineOrder.Id,
					refundResult.NewPaymentStatus);
			}

			_logger.LogInformation("Возврат для заказа {OrderId} выполнен успешно", order.Id);

			return Result.Success();
		}

		/// <summary>
		/// Создает запись недовоза для отмены заказа
		/// </summary>
		private UndeliveredOrder CreateUndeliveryForCancellation(
			IUnitOfWork uow,
			Order order,
			Employee currentUser)
		{
			var oksSubdivision = _subdivisionRepository.GetQCDepartment(uow);
			var undelivery = new UndeliveredOrder
			{
				UoW = uow,
				OldOrder = order,
				NewOrder = null,
				NewDeliverySchedule = null,
				Author = currentUser,
				EmployeeRegistrator = currentUser,
				TimeOfCreation = DateTime.Now,
				OrderTransferType = TransferType.AutoTransferNotApproved,
				Reason = "Отмена заказа клиентом",
				InProcessAtDepartment = oksSubdivision
			};

			var guilty = new GuiltyInUndelivery
			{
				UndeliveredOrder = undelivery,
				GuiltySide = GuiltyTypes.None
			};

			undelivery.GuiltyInUndelivery.Add(guilty);
			undelivery.CreateOkkDiscussion(uow);

			return undelivery;
		}

		private static bool IsSimpleStatus(OrderStatus status) => 
			status is OrderStatus.NewOrder
				   or OrderStatus.WaitForPayment
				   or OrderStatus.Accepted;

		private static bool IsPaidOnline(OnlineOrder order) =>
			order.OnlineOrderPaymentType is OnlineOrderPaymentType.PaidOnline;
	}
}
