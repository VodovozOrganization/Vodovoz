using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using OneOf.Types;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Flyers;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Errors.Orders;
using Vodovoz.Models.Orders;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Tools.CallTasks;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Core.Application.Orders.Services
{
	public class CustomerOrderTransferService : ICustomerOrderTransferService
	{
		private readonly ILogger<CustomerOrderTransferService> _logger;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly ISubdivisionRepository _subdivisionRepository;
		private readonly IFlyerRepository _flyerRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IRouteListService _routeListService;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly IOrderContractUpdater _orderContractUpdater;
		private readonly IRouteListItemRepository _routeListItemRepository;

		public CustomerOrderTransferService(
			ILogger<CustomerOrderTransferService> logger,
			IEmployeeRepository employeeRepository,
			ISubdivisionRepository subdivisionRepository,
			IFlyerRepository flyerRepository,
			IOrderRepository orderRepository,
			IRouteListService routeListService,
			INomenclatureSettings nomenclatureSettings,
			ICallTaskWorker callTaskWorker,
			IOrderContractUpdater orderContractUpdater,
			IRouteListItemRepository routeListItemRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_routeListService = routeListService ?? throw new ArgumentNullException(nameof(routeListService));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_flyerRepository = flyerRepository ?? throw new ArgumentNullException(nameof(flyerRepository));
			_orderContractUpdater = orderContractUpdater ?? throw new ArgumentNullException(nameof(orderContractUpdater));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
		}

		public Result CanTransfer(Order order, DateTime? newDeliveryDate, DeliverySchedule newDeliverySchedule)
		{
			var statusCheckResult = CanTransfer(order);
			if(statusCheckResult.IsFailure)
			{
				return statusCheckResult;
			}

			if(newDeliveryDate.HasValue && newDeliveryDate.Value.Date < DateTime.Now.Date)
			{
				return Result.Failure(OrderErrors.InvalidDeliveryDate(newDeliveryDate.Value));
			}

			var maxDeliveryDate = DateTime.Now.Date.AddDays(30);
			if(newDeliveryDate.Value.Date > maxDeliveryDate)
			{
				return Result.Failure(OrderErrors.DeliveryDateExceedsMaxPeriod(newDeliveryDate.Value, 30));
			}

			if(newDeliverySchedule is null)
			{
				return Result.Failure(OrderErrors.DeliveryScheduleNotFound);
			}

			if(newDeliveryDate.Value.Date == DateTime.Now.Date)
			{
				var currentTime = DateTime.Now.TimeOfDay;

				if(newDeliverySchedule.To <= currentTime)
				{
					return Result.Failure(OrderErrors.DeliveryScheduleAlreadyPassed(newDeliverySchedule.DeliveryTime));
				}
			}

			var deliveryDateChanged = !order.DeliveryDate.HasValue
				|| order.DeliveryDate.Value.Date != newDeliveryDate.Value.Date;

			var deliveryScheduleChanged = order.DeliverySchedule?.Id != newDeliverySchedule.Id;

			if(!deliveryDateChanged && !deliveryScheduleChanged)
			{
				return Result.Failure(OrderErrors.SameDeliveryParameters);
			}

			return Result.Success();
		}

		public Result CanTransfer(Order order)
		{
			var allowedStatuses = _orderRepository.GetStatusesForTransferOrCancellationOnlineOrder();

			if(!allowedStatuses.Contains(order.OrderStatus))
			{
				return Result.Failure(OrderErrors.CannotTransferOrderInStatus(order.OrderStatus));
			}

			return Result.Success();
		}

		public bool IsDeliveryParametersChanged(Order order, DateTime? newDeliveryDate, int? newDeliveryScheduleId)
		{
			if(!newDeliveryDate.HasValue || !newDeliveryScheduleId.HasValue)
			{
				return false;
			}

			var deliveryDateChanged = !order.DeliveryDate.HasValue
				|| order.DeliveryDate.Value.Date != newDeliveryDate.Value.Date;

			var deliveryScheduleChanged = order.DeliverySchedule?.Id != newDeliveryScheduleId.Value;

			return deliveryDateChanged || deliveryScheduleChanged;
		}

		public async Task<Result> ApplyTransferAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			DateTime? newDeliveryDate,
			DeliverySchedule newDeliverySchedule,
			Source source,
			CancellationToken cancellationToken)
		{
			_logger.LogInformation(
				"Применение переноса для заказа {OrderId}, статус: {Status}",
				order.Id,
				order.OrderStatus.GetEnumTitle());

			var canTransferResult = CanTransfer(order, newDeliveryDate, newDeliverySchedule);
			if(canTransferResult.IsFailure)
			{
				_logger.LogWarning(
					"Заказ {OrderId}: {ErrorMessage}",
					order.Id,
					canTransferResult.Errors.FirstOrDefault().Message);

				return canTransferResult;
			}

			if(IsSimpleStatus(order.OrderStatus))
			{
				return await ApplySimpleTransferAsync(uow, order, onlineOrder, newDeliveryDate, newDeliverySchedule, cancellationToken);
			}

			if(order.OrderStatus is OrderStatus.InTravelList)
			{
				return await ApplyTransferFromTravelListAsync(uow, order, onlineOrder, newDeliveryDate, newDeliverySchedule, cancellationToken);
			}

			if(order.OrderStatus is OrderStatus.OnLoading || order.OrderStatus is OrderStatus.OnTheWay)
			{
				return await ApplyTransferWithUndeliveryAsync(uow, order, onlineOrder, newDeliveryDate, newDeliverySchedule, source, cancellationToken);
			}

			var error = OrderErrors.UnsupportedOrderStatusForTransfer(order.OrderStatus);
			_logger.LogWarning("Заказ {OrderId}: {ErrorMessage}", order.Id, error.Message);

			return Result.Failure(error);
		}

		private static bool IsSimpleStatus(OrderStatus status) =>
			status is OrderStatus.NewOrder
			|| status is OrderStatus.WaitForPayment
			|| status is OrderStatus.Accepted;

		/// <summary>
		/// Простой перенос заказа - изменение даты и интервала доставки
		/// Для статусов: NewOrder, WaitForPayment, Accepted
		/// </summary>
		private async Task<Result> ApplySimpleTransferAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			DateTime? newDeliveryDate,
			DeliverySchedule newDeliverySchedule,
			CancellationToken cancellationToken)
		{
			_logger.LogInformation(
				"Простой перенос заказа {OrderId}",
				order.Id);

			try
			{
				order.TransferToNewDateAndSchedule(
					newDeliveryDate,
					newDeliverySchedule,
					_orderContractUpdater,
					out _);

				onlineOrder.UpdateOnlineOrderDeliveryData(
					newDeliverySchedule,
					newDeliverySchedule.Id,
					newDeliveryDate,
					onlineOrder.IsFastDelivery);

				await uow.SaveAsync(order, cancellationToken: cancellationToken);
				await uow.SaveAsync(onlineOrder, cancellationToken: cancellationToken);

				_logger.LogInformation(
					"Простой перенос заказа {OrderId} на дату {NewDate} завершен",
					order.Id,
					newDeliveryDate);

				return Result.Success();
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при простом переносе заказа {OrderId}",
					order.Id);

				return Result.Failure(OrderErrors.TransferFailed(ex.Message));
			}
		}

		/// <summary>
		/// Перенос заказа из маршрутного листа (статус InTravelList)
		/// </summary>
		private async Task<Result> ApplyTransferFromTravelListAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			DateTime? newDeliveryDate,
			DeliverySchedule newDeliverySchedule,
			CancellationToken cancellationToken)
		{
			_logger.LogInformation(
				"Перенос заказа {OrderId} из маршрутного листа",
				order.Id);

			var routeListItem = _routeListItemRepository.GetRouteListItemForOrder(uow, order);

			if(routeListItem is null)
			{
				var error = OrderErrors.RouteListItemNotFound(order.Id);
				_logger.LogWarning("Заказ {OrderId}: {ErrorMessage}", order.Id, error.Message);

				return Result.Failure(error);
			}

			try
			{
				var routeList = routeListItem.RouteList;
				routeList.RemoveAddress(routeListItem);

				order.ChangeStatus(OrderStatus.Accepted);
				order.TransferToNewDateAndSchedule(
					newDeliveryDate,
					newDeliverySchedule,
					_orderContractUpdater,
					out _);

				onlineOrder.UpdateOnlineOrderDeliveryData(
					newDeliverySchedule,
					newDeliverySchedule.Id,
					newDeliveryDate,
					onlineOrder.IsFastDelivery);

				await uow.SaveAsync(order, cancellationToken: cancellationToken);
				await uow.SaveAsync(onlineOrder, cancellationToken: cancellationToken);
				await uow.SaveAsync(routeList, cancellationToken: cancellationToken);

				_logger.LogInformation(
					"Заказ {OrderId} успешно перенесен из маршрутного листа на {NewDate}",
					order.Id,
					newDeliveryDate);

				return Result.Success();
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при переносе заказа {OrderId} из маршрутного листа",
					order.Id);

				return Result.Failure(OrderErrors.TransferFailed(ex.Message));
			}
		}

		/// <summary>
		/// Перенос заказа с использованием механизма недовоза (статусы OnLoading, OnTheWay)
		/// Создает копию заказа для новой даты доставки
		/// </summary>
		private async Task<Result> ApplyTransferWithUndeliveryAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			DateTime? newDeliveryDate,
			DeliverySchedule newDeliverySchedule,
			Source source,
			CancellationToken cancellationToken)
		{
			_logger.LogInformation(
				"Перенос заказа {OrderId} с использованием механизма недовоза",
				order.Id);

			try
			{
				var currentUser = await _employeeRepository.GetEmployeeBySourceAsync(uow, source, cancellationToken);
				if(currentUser is null)
				{
					var error = OnlineOrderErrors.EmployeeNotFound(source);
					_logger.LogWarning("Заказ {OrderId}: {ErrorMessage}", order.Id, error.Message);
					return Result.Failure(error);
				}

				if(newDeliverySchedule is null)
				{
					var error = OrderErrors.DeliveryScheduleNotFound;
					_logger.LogWarning("Заказ {OrderId}: {ErrorMessage}", order.Id, error.Message);
					return Result.Failure(error);
				}

				var newOrder = await CreateOrderCopy(uow, order, newDeliveryDate, newDeliverySchedule, cancellationToken);

				_logger.LogInformation(
					"Создана копия заказа {OrderId}: новый заказ {NewOrderId}",
					order.Id,
					newOrder.Id);

				var undelivery = CreateUndeliveryForTransfer(
					uow,
					order,
					newOrder,
					currentUser,
					newDeliverySchedule);

				order.SetUndeliveredStatus(
					uow,
					_routeListService,
					_nomenclatureSettings,
					_callTaskWorker,
					needCreateDeliveryFreeBalanceOperation: false);

				onlineOrder.UpdateOnlineOrderDeliveryData(
					newDeliverySchedule,
					newDeliverySchedule.Id,
					newDeliveryDate,
					onlineOrder.IsFastDelivery);

				onlineOrder.Orders.Add(newOrder);

				await uow.SaveAsync(order, cancellationToken: cancellationToken);
				await uow.SaveAsync(newOrder, cancellationToken: cancellationToken);
				await uow.SaveAsync(undelivery, cancellationToken: cancellationToken);
				await uow.SaveAsync(onlineOrder, cancellationToken: cancellationToken);

				_logger.LogInformation(
					"Заказ {OrderId} успешно перенесен с недовозом. Новый заказ: {NewOrderId}, Недовоз: {UndeliveryId}",
					order.Id,
					newOrder.Id,
					undelivery.Id);

				return Result.Success();
			}
			catch(Exception ex)
			{
				_logger.LogError(
					ex,
					"Ошибка при переносе заказа {OrderId} с недовозом",
					order.Id);

				return Result.Failure(OrderErrors.TransferFailed(ex.Message));
			}
		}

		/// <summary>
		/// Создает копию заказа с новыми датой и интервалом доставки
		/// </summary>
		private async Task<Order> CreateOrderCopy(
			IUnitOfWork uow,
			Order originalOrder,
			DateTime? newDeliveryDate,
			DeliverySchedule newDeliverySchedule,
			CancellationToken cancellationToken)
		{
			var newOrder = new Order
			{
				UoW = uow
			};

			var orderCopyModel = new OrderCopyModel(_nomenclatureSettings, _flyerRepository, _orderContractUpdater);

			var copying = orderCopyModel.StartCopyOrder(uow, originalOrder.Id, newOrder)
				.CopyFields()
				.CopyStockBottle()
				.CopyPromotionalSets()
				.CopyOrderItems(false, true, true)
				.CopyPaidDeliveryItem()
				.CopyAdditionalOrderEquipments()
				.CopyOrderDepositItems()
				.CopyAttachedDocuments();

			newOrder.TransferToNewDateAndSchedule(
				newDeliveryDate,
				newDeliverySchedule,
				_orderContractUpdater,
				out _);
			newOrder.OrderStatus = OrderStatus.Accepted;
			newOrder.IsCopiedFromUndelivery = true;

			if(copying.GetCopiedOrder.PaymentType is PaymentType.PaidOnline)
			{
				copying.CopyPaymentByCardDataIfPossible();
			}

			if(copying.GetCopiedOrder.PaymentType is PaymentType.DriverApplicationQR
				|| copying.GetCopiedOrder.PaymentType is PaymentType.SmsQR)
			{
				copying.CopyPaymentByQrDataIfPossible();
			}

			await uow.SaveAsync(newOrder, cancellationToken: cancellationToken);

			return newOrder;
		}

		private UndeliveredOrder CreateUndeliveryForTransfer(
			IUnitOfWork uow,
			Order oldOrder,
			Order newOrder,
			Employee currentUser,
			DeliverySchedule newDeliverySchedule)
		{
			var oksSubdivision = _subdivisionRepository.GetQCDepartment(uow);
			var undelivery = new UndeliveredOrder
			{
				UoW = uow,
				OldOrder = oldOrder,
				NewOrder = newOrder,
				NewDeliverySchedule = newDeliverySchedule,
				Author = currentUser,
				EmployeeRegistrator = currentUser,
				TimeOfCreation = DateTime.Now,
				OrderTransferType = TransferType.AutoTransferApproved,
				Reason = "Перенос заказа клиентом",
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
	}
}
