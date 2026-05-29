using CustomerOrdersApi.Library.Default.Factories;
using CustomerOrdersApi.Library.V5.Dto.Orders.CancelOrder;
using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Errors.Clients;
using Vodovoz.Errors.Orders;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Tools.CallTasks;
using IOrderRepository = Vodovoz.EntityRepositories.Orders.IOrderRepository;

namespace CustomerOrdersApi.Library.V5.Services
{
	public class CustomerOrderCancellationService : ICustomerOrderCancellationService
	{
		private readonly ILogger<CustomerOrderCancellationService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IOnlineOrderRepository _onlineOrderRepository;
		private readonly ICounterpartyRepository _counterpartyRepository;
		private readonly IOnlinePaymentRepository _onlinePaymentRepository;
		private readonly IRouteListService _routeListService;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly IPaymentRefundServiceFactory _paymentRefundServiceFactory;

		public CustomerOrderCancellationService(
			ILogger<CustomerOrderCancellationService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IRouteListItemRepository routeListItemRepository,
			IEmployeeRepository employeeRepository,
			ISubdivisionRepository subdivisionRepository,
			IOrderRepository orderRepository,
			IOnlineOrderRepository onlineOrderRepository,
			ICounterpartyRepository counterpartyRepository,
			IOnlinePaymentRepository onlinePaymentRepository,
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
			_onlineOrderRepository = onlineOrderRepository ?? throw new ArgumentNullException(nameof(onlineOrderRepository));
			_counterpartyRepository = counterpartyRepository ?? throw new ArgumentNullException(nameof(counterpartyRepository));
			_onlinePaymentRepository = onlinePaymentRepository ?? throw new ArgumentNullException(nameof(onlinePaymentRepository));
			_routeListService = routeListService ?? throw new ArgumentNullException(nameof(routeListService));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_paymentRefundServiceFactory = paymentRefundServiceFactory ?? throw new ArgumentNullException(nameof(paymentRefundServiceFactory));
		}

		public async Task<Result> CanCancel(IUnitOfWork uow, Order order, OnlineOrder onlineOrder, CancellationToken cancellationToken)
		{
			if(order is null)
			{
				if(onlineOrder is not null 
					&& onlineOrder.OnlineOrderPaymentType is not OnlineOrderPaymentType.PaidOnline)
				{
					return Result.Success();
				}
				
				return Result.Failure(OrderErrors.NotFound);
			}

			var allowedStatuses = _orderRepository.GetStatusesForTransferOrCancellationOnlineOrder();

			if(!allowedStatuses.Contains(order.OrderStatus))
			{
				return Result.Failure(OrderErrors.CannotCancelOrderInStatus(order.OrderStatus));
			}

			if(onlineOrder is null)
			{
				if(order.PaymentType 
					is PaymentType.Cash
					or PaymentType.SmsQR
					or PaymentType.Terminal
					or PaymentType.DriverApplicationQR)
				{ 
					return Result.Success();
				}
				else
				{
					return Result.Failure(OrderErrors.CannotCancelOrderWithPaymentType(order.PaymentType));
				}
			}

			if(onlineOrder.OnlineOrderPaymentType is not OnlineOrderPaymentType.PaidOnline)
			{
				return Result.Success();
			}

			if(IsUnPaidOnline(onlineOrder) || !onlineOrder.OnlinePayment.HasValue)
			{
				return Result.Failure(OrderErrors.CannotCancelOrderWithDetails("Не оплачен заказ"));
			}

			if(IsPaidOnline(onlineOrder) && !onlineOrder.OnlinePayment.HasValue)
			{
				return Result.Failure(OrderErrors.CannotCancelOrderWithDetails("Нет номера оплаты"));
			}

			var onlinePayment = await _onlinePaymentRepository.GetByExternalIdAsync(
				uow,
				onlineOrder.OnlinePayment.Value,
				cancellationToken);

			if(onlinePayment is null)
			{
				_logger.LogWarning(
					"Заказ {OrderId} оплачен, номер оплаты {OnlinePayment} есть, но нет записи в online_payments",
					order.Id,
					onlineOrder.OnlinePayment.Value);

				return Result.Failure(OrderErrors.CannotCancelOrderWithDetails("Нет номера оплаты"));
			}

			if(string.IsNullOrWhiteSpace(onlinePayment.TransactionId))
			{
				_logger.LogWarning(
					"Заказ {OrderId} имеет запись в online_payments, но TransactionId пуст",
					order.Id);

				return Result.Failure(OrderErrors.CannotCancelOrder);
			}

			return Result.Success();
		}

		public async Task<Result<string>> ApplyCancellationAsync(
			Source source,
			int counterpartyId,
			int? orderId,
			int? onlineOrderId,
			CancellationToken cancellationToken)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Сервис отмены заказа");

			var counterparty = await _counterpartyRepository.GetCounterpartyByIdAsync(uow, counterpartyId, cancellationToken);
			if(counterparty is null)
			{
				_logger.LogWarning("Контрагент с EprCounterpartyId {EprCounterpartyId} не найден", counterpartyId);

				return Result.Failure<string>(CounterpartyErrors.NotFound);
			}

			var orderSearchResult = await GetOrderForCancellationAsync(uow, orderId, onlineOrderId, cancellationToken);
			if(orderSearchResult.IsFailure)
			{
				return Result.Failure<string>(orderSearchResult.Errors);
			}

			var (order, onlineOrder) = orderSearchResult.Value;

			var canCancelResult = await CanCancel(uow, order, onlineOrder, cancellationToken);
			if(canCancelResult.IsFailure)
			{
				return Result.Failure<string>(canCancelResult.Errors);
			}

			if(order is null)
			{
				if(onlineOrder is not null 
					&& onlineOrder.OnlineOrderPaymentType is not OnlineOrderPaymentType.PaidOnline)
				{
					await CancelOnlineOrder(uow, onlineOrder, cancellationToken);
					await uow.CommitAsync(cancellationToken);

					return Result.Success("Заказ отменен успешно");
				}

				return Result.Failure<string>(OrderErrors.CannotCancelOrder);
			}

			if(order.Client?.Id != counterparty.Id)
			{
				_logger.LogWarning("Заказ {OrderId} не принадлежит контрагенту {CounterpartyId}", order.Id, counterparty.Id);

				return Result.Failure<string>(OrderErrors.OrderDoesNotBelongToCounterparty);
			}

			Result<string> result;

			if(IsSimpleStatus(order.OrderStatus))
			{
				result = await ApplySimpleCancellationAsync(uow, order, onlineOrder, cancellationToken);
			}
			else if(order.OrderStatus is OrderStatus.InTravelList)
			{
				result = await ApplyCancellationFromTravelListAsync(uow, order, onlineOrder, cancellationToken);
			}
			else if(order.OrderStatus is OrderStatus.OnLoading or OrderStatus.OnTheWay)
			{
				result = await ApplyCancellationWithUndeliveryAsync(uow, order, onlineOrder, source, cancellationToken);
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
				_logger.LogWarning("Отмена заказа {OrderId} не выполнена, ошибка: {Error}", order.Id, result.Errors.FirstOrDefault().Message);
			}

			return result;
		}

		private async Task<Result<(Order order, OnlineOrder onlineOrder)>> GetOrderForCancellationAsync(
		   IUnitOfWork uow,
		   int? orderId,
		   int? onlineOrderId,
		   CancellationToken cancellationToken)
		{
			if(orderId.HasValue && onlineOrderId.HasValue)
			{
				var order = await _orderRepository.GetOrderByIdAsync(uow, orderId.Value, cancellationToken);
				var onlineOrder = _onlineOrderRepository.GetOnlineOrderById(uow, onlineOrderId.Value);

				if(order is not null && onlineOrder is not null)
				{
					return Result.Success<(Order, OnlineOrder)>((order, onlineOrder));
				}

				if(order is null && onlineOrder is null)
				{
					_logger.LogWarning("Заказы с OrderId {OrderId} и OnlineOrderId {OnlineOrderId} не найдены",
						orderId.Value, onlineOrderId.Value);

					return Result.Failure<(Order, OnlineOrder)>(OrderErrors.NotFound);
				}

				if(order is null && onlineOrder is not null)
				{
					_logger.LogInformation("Найден онлайн заказ {OnlineOrderId}, ДВ заказ отсутствует",
							onlineOrder.Id);
					return Result.Success<(Order, OnlineOrder)>((null, onlineOrder));
				}

				if(order is not null && onlineOrder is null)
				{
					_logger.LogInformation("Найден ДВ заказ {OrderId}, онлайн заказ отсутствует", order.Id);
					return Result.Success<(Order, OnlineOrder)>((order, null));
				}
			}

			if(orderId.HasValue)
			{
				var order = await _orderRepository.GetOrderByIdAsync(uow, orderId.Value, cancellationToken);
				if(order is null)
				{
					_logger.LogWarning("Заказ с OrderId {OrderId} не найден", orderId.Value);
					return Result.Failure<(Order, OnlineOrder)>(OrderErrors.NotFound);
				}

				var onlineOrder = order.OnlineOrder;
				if(onlineOrder is not null)
				{
					_logger.LogInformation("Найден заказ {OrderId} со связанным онлайн заказом {OnlineOrderId}",
						order.Id, onlineOrder.Id);
					return Result.Success<(Order, OnlineOrder)>((order, onlineOrder));
				}

				_logger.LogInformation("Найден ДВ заказ {OrderId} без онлайн заказа", order.Id);
				return Result.Success<(Order, OnlineOrder)>((order, null));
			}

			if(onlineOrderId.HasValue)
			{
				var onlineOrder = _onlineOrderRepository.GetOnlineOrderById(uow, onlineOrderId.Value);
				if(onlineOrder is null)
				{
					_logger.LogWarning("Онлайн заказ с OnlineOrderId {OnlineOrderId} не найден", onlineOrderId.Value);
					return Result.Failure<(Order, OnlineOrder)>(OnlineOrderErrors.OnlineOrderNotFound);
				}

				var order = GetActiveOrder(onlineOrder);
				if(order is not null)
				{
					_logger.LogInformation("Найден онлайн заказ {OnlineOrderId}, связанный с заказом {OrderId}",
						onlineOrder.Id, order.Id);
					return Result.Success((order, onlineOrder));
				}

				if(onlineOrder.OnlineOrderPaymentType is not OnlineOrderPaymentType.PaidOnline)
				{
					_logger.LogInformation("Найден онлайн заказ {OnlineOrderId} с типом оплаты не PaidOnline, без связанного ДВ заказа",
						onlineOrder.Id);
					return Result.Success<(Order, OnlineOrder)>((null, onlineOrder));
				}

				_logger.LogWarning("Онлайн заказ {OnlineOrderId} имеет тип оплаты PaidOnline, но не связан с активным заказом",
					onlineOrderId.Value);
				return Result.Failure<(Order, OnlineOrder)>(OnlineOrderErrors.IsOnlineOrderDoesNotHaveALinkedOrder);
			}

			return Result.Failure<(Order, OnlineOrder)>(OrderErrors.NotFound);
		}

		private Order GetActiveOrder(OnlineOrder onlineOrder)
		{
			var undeliveryStatuses = _orderRepository.GetUndeliveryStatuses();

			return onlineOrder.Orders.FirstOrDefault(x => !undeliveryStatuses.Contains(x.OrderStatus));
		}

		/// <summary>
		/// Получение TransactionId для возврата средств
		/// </summary>
		private async Task<string> GetTransactionIdForCancellationAsync(
			IUnitOfWork uow,
			OnlineOrder onlineOrder,
			CancellationToken cancellationToken)
		{
			if(onlineOrder.OnlinePayment.HasValue is true)
			{
				var onlinePayment = await _onlinePaymentRepository.GetByExternalIdAsync(uow, onlineOrder.OnlinePayment.Value, cancellationToken);
				if(onlinePayment is not null && !string.IsNullOrWhiteSpace(onlinePayment.TransactionId))
				{
					return onlinePayment.TransactionId;
				}
			}

			_logger.LogWarning("Не удалось найти TransactionId для заказа {OrderId}", onlineOrder?.Id);
			return null;
		}

		/// <summary>
		/// Простая отмена заказа (статусы: NewOrder, WaitForPayment, Accepted)
		/// </summary>
		private async Task<Result<string>> ApplySimpleCancellationAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			CancellationToken cancellationToken)
		{
			_logger.LogInformation(
				"Простая отмена заказа {OrderId} в статусе {Status}",
				order.Id,
				order.OrderStatus.GetEnumTitle());

			var refundResult = await ProcessRefundIfNeededAsync(uow, order, onlineOrder, cancellationToken);
			if(refundResult.IsFailure)
			{
				return Result.Failure<string>(refundResult.Errors);
			}

			order.ChangeStatus(OrderStatus.Canceled);
			await uow.SaveAsync(order, cancellationToken: cancellationToken);
			await CancelOnlineOrder(uow, onlineOrder, cancellationToken);

			_logger.LogInformation("Заказ {OrderId} успешно отменен", order.Id);

			return Result.Success("Заказ отменен успешно");
		}

		private static async Task CancelOnlineOrder(IUnitOfWork uow, OnlineOrder onlineOrder, CancellationToken cancellationToken)
		{
			if(onlineOrder is not null)
			{
				onlineOrder.OnlineOrderStatus = OnlineOrderStatus.Canceled;
				await uow.SaveAsync(onlineOrder, cancellationToken: cancellationToken);
			}
		}

		/// <summary>
		/// Отмена заказа из маршрутного листа (статус InTravelList)
		/// </summary>
		private async Task<Result<string>> ApplyCancellationFromTravelListAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			CancellationToken cancellationToken)
		{
			_logger.LogInformation(
				"Отмена заказа {OrderId} из маршрутного листа",
				order.Id);

			var routeListItem = _routeListItemRepository.GetRouteListItemForOrder(uow, order);

			if(routeListItem is null)
			{
				var error = OrderErrors.RouteListItemNotFound(order.Id);
				_logger.LogWarning("Заказ {OrderId}: {ErrorMessage}", order.Id, error.Message);

				return Result.Failure<string>(error);
			}

			var routeList = routeListItem.RouteList;
			routeList.RemoveAddress(routeListItem);

			var refundResult = await ProcessRefundIfNeededAsync(uow, order, onlineOrder, cancellationToken);
			if(refundResult.IsFailure)
			{
				return Result.Failure<string>(refundResult.Errors);
			}

			order.ChangeStatus(OrderStatus.Canceled);
			await uow.SaveAsync(order, cancellationToken: cancellationToken);
			await uow.SaveAsync(routeList, cancellationToken: cancellationToken);
			await CancelOnlineOrder(uow, onlineOrder, cancellationToken);

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
			CancellationToken cancellationToken)
		{
			_logger.LogInformation(
				"Отмена заказа {OrderId} из статуса '{Status}' с использованием механизма недовоза",
				order.Id,
				order.OrderStatus.GetEnumTitle());

			var currentUser = await _employeeRepository.GetEmployeeBySourceAsync(uow, source, cancellationToken);
			if(currentUser is null)
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

			var refundResult = await ProcessRefundIfNeededAsync(uow, order, onlineOrder, cancellationToken);
			if(refundResult.IsFailure)
			{
				return Result.Failure<string>(refundResult.Errors);
			}

			await CancelOnlineOrder(uow, onlineOrder, cancellationToken);
			await uow.SaveAsync(order, cancellationToken: cancellationToken);
			await uow.SaveAsync(undelivery, cancellationToken: cancellationToken);

			_logger.LogInformation(
				"Заказ {OrderId} успешно отменен с недовозом. Недовоз: {UndeliveryId}",
				order.Id,
				undelivery.Id);

			return Result.Success("Заказ отменен успешно");
		}

		private async Task<Result> ProcessRefundIfNeededAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			CancellationToken cancellationToken)
		{
			if(!IsPaidOnline(onlineOrder))
			{
				return Result.Success();
			}

			var transactionId = await GetTransactionIdForCancellationAsync(uow, onlineOrder, cancellationToken);
			var refundResult = await ProcessRefundAsync(uow, order, onlineOrder, transactionId, cancellationToken);

			if(refundResult.IsFailure)
			{
				return Result.Failure(refundResult.Errors);
			}

			return Result.Success();
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
			if(string.IsNullOrEmpty(transactionId))
			{
				return Result.Failure(OrderErrors.CannotCancelOrder);
			}

			_logger.LogInformation(
				"Обработка возврата для оплаченного заказа {OrderId}. TransactionId: {TransactionId}",
				order.Id,
				transactionId);

			var refundService = _paymentRefundServiceFactory.GetRefundService(onlineOrder);

			var refundRequest = new RefundRequestDto
			{
				OnlineOrder = onlineOrder,
				TransactionId = transactionId,
				Amount = onlineOrder.OnlineOrderSum,
				ExternalOrderId = onlineOrder?.ExternalOrderId.ToString()
			};

			var refundResult = await refundService.ProcessRefundAsync(uow, refundRequest, cancellationToken);

			if(!refundResult.Success)
			{
				_logger.LogWarning(
					"Не удалось выполнить возврат для заказа {OrderId}: {ErrorMessage}",
					order.Id,
					refundResult.ErrorMessage);

				return Result.Failure(OrderErrors.CannotCancelOrderWithError(refundResult.ErrorMessage));
			}

			onlineOrder.OnlineOrderPaymentStatus = OnlineOrderPaymentStatus.Refund;
			await uow.SaveAsync(onlineOrder, cancellationToken: cancellationToken);

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

		private static bool IsPaidOnline(OnlineOrder onlineOrder) => onlineOrder is not null
			&& onlineOrder.OnlineOrderPaymentType is OnlineOrderPaymentType.PaidOnline
			&& onlineOrder.OnlineOrderPaymentStatus is OnlineOrderPaymentStatus.Paid;

		private static bool IsUnPaidOnline(OnlineOrder onlineOrder) => onlineOrder is not null
			&& onlineOrder.OnlineOrderPaymentType is OnlineOrderPaymentType.PaidOnline
			&& onlineOrder.OnlineOrderPaymentStatus is not OnlineOrderPaymentStatus.Paid;
	}
}
