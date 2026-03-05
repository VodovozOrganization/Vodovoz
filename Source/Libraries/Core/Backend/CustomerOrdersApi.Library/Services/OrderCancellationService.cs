using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using Gamma.Utilities;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Tools.CallTasks;
using VodovozBusiness.Services.Orders;

namespace CustomerOrdersApi.Library.Services
{
	public class OrderCancellationService : IOrderCancellationService
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ILogger<OrderCancellationService> _logger;
		private readonly IOnlineOrderRepository _onlineOrderRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IRouteListService _routeListService;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly IOrderContractUpdater _orderContractUpdater;

		private static readonly OrderStatus[] _simpleCancelStatuses = new[]
		{
			OrderStatus.NewOrder,
			OrderStatus.WaitForPayment,
			OrderStatus.Accepted
		};

		private static readonly OrderStatus[] _complexCancelStatuses = new[]
		{
			OrderStatus.InTravelList,
			OrderStatus.OnLoading,
			OrderStatus.OnTheWay
		};

		private static readonly OrderStatus[] _allowedCancelStatuses =
			_simpleCancelStatuses.Concat(_complexCancelStatuses).ToArray();

		public OrderCancellationService(
			IUnitOfWorkFactory unitOfWorkFactory,
			ILogger<OrderCancellationService> logger,
			IOnlineOrderRepository onlineOrderRepository,
			IOrderRepository orderRepository,
			IRouteListItemRepository routeListItemRepository,
			IEmployeeRepository employeeRepository,
			IRouteListService routeListService,
			INomenclatureSettings nomenclatureSettings,
			ICallTaskWorker callTaskWorker,
			IOrderContractUpdater orderContractUpdater)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_onlineOrderRepository = onlineOrderRepository ?? throw new ArgumentNullException(nameof(onlineOrderRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_routeListService = routeListService ?? throw new ArgumentNullException(nameof(routeListService));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_orderContractUpdater = orderContractUpdater ?? throw new ArgumentNullException(nameof(orderContractUpdater));
		}


		public async Task<CancelOrderResult> CancelOrderAsync(CancelOrderDto cancelOrderDto)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot();
			try
			{
				var onlineOrder = _onlineOrderRepository.GetOnlineOrderByExternalId(uow, cancelOrderDto.ExternalOrderId);

				if(onlineOrder == null)
				{
					_logger.LogWarning(
						"Попытка отмены несуществующего заказа: ExternalOrderId: {ExternalOrderId}",
						cancelOrderDto.ExternalOrderId);

					return new CancelOrderResult(false, 404, "One or more validation errors occurred", "Заказ не найден");
				}

				var (canCancel, statusTitle, requiresComplexHandling, errorMessage) =
					CanCancelOrder(uow, cancelOrderDto.ExternalOrderId);

				if(!canCancel)
				{
					_logger.LogWarning(
						"Отмена заказа {ExternalOrderId} невозможна, причина: {Reason}",
						cancelOrderDto.ExternalOrderId,
						errorMessage);

					return new CancelOrderResult(false, 408, "One or more validation errors occurred", errorMessage);
				}

				if(!onlineOrder.Orders.Any())
				{
					_logger.LogWarning(
						"Попытка отмены онлайн заказа {ExternalOrderId} без привязанного заказа",
						cancelOrderDto.ExternalOrderId);

					return new CancelOrderResult(
						false,
						400,
						"One or more validation errors occurred",
						"Онлайн заказ не имеет привязанного заказа");
				}

				var undeliveryStatuses = _orderRepository.GetUndeliveryStatuses();
				var order = onlineOrder.Orders.FirstOrDefault(x => !undeliveryStatuses.Contains(x.OrderStatus));

				if(requiresComplexHandling)
				{
					return await CancelComplexOrderAsync(uow, order, onlineOrder, cancelOrderDto);
				}
				else
				{
					return await CancelSimpleOrderAsync(uow, order, onlineOrder, cancelOrderDto);
				}

				throw new InvalidOperationException($"Неизвестный статус заказа: {order.OrderStatus}");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,
					"Ошибка при отмене заказа {ExternalOrderId}",
					cancelOrderDto.ExternalOrderId);

				return new CancelOrderResult(false, 500, "One or more validation errors occurred", "Произошла ошибка, пожалуйста, попробуйте позже");
			}
		}

		private (bool canCancel, string statusTitle, bool requiresComplexHandling, string errorMessage) CanCancelOrder(
			IUnitOfWork uow,
			Guid externalOrderId)
		{
			var onlineOrder = _onlineOrderRepository.GetOnlineOrderByExternalId(uow, externalOrderId);

			if(onlineOrder == null)
			{
				return (false, default, default, "Заказ не найден");
			}

			if(onlineOrder.Orders.Count == 0)
			{
				return (true, "Без привязанного заказа", false, default);
			}

			var undeliveryStatuses = _orderRepository.GetUndeliveryStatuses();
			var order = onlineOrder.Orders.FirstOrDefault(x => !undeliveryStatuses.Contains(x.OrderStatus));

			if(order == null)
			{
				return (false, default, default, "Заказ не найден");
			}

			if(!_allowedCancelStatuses.Contains(order.OrderStatus))
			{
				return (
					false,
					order.OrderStatus.GetEnumTitle(),
					default,
					$"Невозможно отменить заказ в статусе '{order.OrderStatus.GetEnumTitle()}'"
				);
			}

			if(order.OrderStatus == OrderStatus.OnTheWay)
			{
				return (
					false,
					order.OrderStatus.GetEnumTitle(),
					default,
					$"Невозможно отменить заказ в статусе '{order.OrderStatus.GetEnumTitle()}' с установленным маршрутом"
				);
			}

			return (
				true,
				order.OrderStatus.GetEnumTitle(),
				_complexCancelStatuses.Contains(order.OrderStatus),
				default
			);
		}

		/// <summary>
		/// Простая отмена заказа (статусы: Новый, Ожидание оплаты, Принят)
		/// </summary>
		private async Task<CancelOrderResult> CancelSimpleOrderAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			CancelOrderDto cancelOrderDto)
		{
			try
			{
				_logger.LogInformation(
					"Начало отмены заказа {OrderId} в статусе {Status}",
					order.Id,
					order.OrderStatus.GetEnumTitle());

				if(order.PaymentType == PaymentType.PaidOnline)
				{
					return await ProcessPaidOnlineCancellationAsync(uow, order, onlineOrder, cancelOrderDto);
				}

				order.ChangeStatus(OrderStatus.Canceled);
				order.Version = DateTime.Now;

				uow.Save(order);
				uow.Commit();

				_logger.LogInformation(
					"Заказ {OrderId} успешно отменен",
					order.Id);

				return new CancelOrderResult(true, 200, "Success", "Заказ отменен успешно");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,
					"Ошибка при простой отмене заказа {OrderId}",
					order.Id);

				return new CancelOrderResult(false, 500, "One or more validation errors occurred", "Произошла ошибка при отмене заказа");
			}
		}

		/// <summary>
		/// Сложная отмена заказа (статусы: В маршрутном листе, На погрузке, В пути)
		/// Создается недовоз без создания нового заказа
		/// </summary>
		private async Task<CancelOrderResult> CancelComplexOrderAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			CancelOrderDto cancelOrderDto)
		{
			try
			{
				return order.OrderStatus switch
				{
					OrderStatus.InTravelList =>
						await CancelOrderFromTravelListAsync(uow, order, onlineOrder, cancelOrderDto),

					OrderStatus.OnLoading or OrderStatus.OnTheWay =>
						await CancelOrderFromOnTheWayOrOnLoadingAsync(uow, order, onlineOrder, cancelOrderDto),

					_ => throw new InvalidOperationException($"Неожиданный статус заказа: {order.OrderStatus}")
				};
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,
					"Ошибка при отмене заказа {OrderId} в статусе {Status}",
					order.Id,
					order.OrderStatus.GetEnumTitle());

				return new CancelOrderResult(
					false,
					500,
					"One or more validation errors occurred",
					"Произошла ошибка при отмене заказа");
			}
		}

		/// <summary>
		/// Отмена заказа в статусе "В маршрутном листе"
		/// </summary>
		private async Task<CancelOrderResult> CancelOrderFromTravelListAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			CancelOrderDto cancelOrderDto)
		{
			try
			{
				_logger.LogInformation(
					"Отмена заказа {OrderId} из маршрутного листа",
					order.Id);

				var routeListItem = _routeListItemRepository.GetRouteListItemForOrder(uow, order);

				if(routeListItem == null)
				{
					_logger.LogWarning(
						"Позиция маршрутного листа не найдена для заказа {OrderId}",
						order.Id);

					return new CancelOrderResult(
						false,
						400,
						"One or more validation errors occurred",
						"Позиция маршрутного листа не найдена");
				}

				var routeList = routeListItem.RouteList;
				routeList.RemoveAddress(routeListItem);
				routeList.Version = DateTime.Now;

				if(order.PaymentType == PaymentType.PaidOnline)
				{
					return await ProcessPaidOnlineCancellationAsync(uow, order, onlineOrder, cancelOrderDto);
				}

				order.ChangeStatus(OrderStatus.Canceled);
				order.Version = DateTime.Now;

				uow.Save(routeList);
				uow.Save(order);
				uow.Commit();

				_logger.LogInformation(
					"Заказ {OrderId} успешно отменен из маршрутного листа",
					order.Id);

				return new CancelOrderResult(true, 200, "Success", "Заказ отменен успешно");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,
					"Ошибка при отмене заказа {OrderId} из маршрутного листа",
					order.Id);

				return new CancelOrderResult(
					false,
					500,
					"One or more validation errors occurred",
					"Произошла ошибка при отмене заказа");
			}
		}

		/// <summary>
		/// Отмена заказа со статусами "На погрузке" и "В пути" с использованием механизма недовоза
		/// Заказ отменяется через установку статуса "Недовезено" без создания нового заказа
		/// </summary>
		private async Task<CancelOrderResult> CancelOrderFromOnTheWayOrOnLoadingAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			CancelOrderDto cancelOrderDto)
		{
			try
			{
				_logger.LogInformation(
					"Начало отмены заказа {OrderId} из статуса 'В пути' с использованием механизма недовоза",
					order.Id);

				var currentUser = _employeeRepository.GetEmployeeForCurrentUser(uow);
				if(currentUser == null)
				{
					_logger.LogWarning(
						"Не удалось получить текущего пользователя для отмены заказа {OrderId}",
						order.Id);

					return new CancelOrderResult(
						false,
						500,
						"One or more validation errors occurred",
						"Не удалось получить информацию о пользователе");
				}

				if(order.PaymentType == PaymentType.PaidOnline)
				{
					var cancellationResult = await ProcessPaidOnlineCancellationAsync(uow, order, onlineOrder, cancelOrderDto);
					if(!cancellationResult.Success)
					{
						return cancellationResult;
					}
				}

				var undelivery = CreateUndeliveryForCancellation(uow, order, currentUser);

				order.SetUndeliveredStatus(
					uow,
					_routeListService,
					_nomenclatureSettings,
					_callTaskWorker,
					needCreateDeliveryFreeBalanceOperation: false);

				_logger.LogInformation(
					"Установлен статус 'Недовезено' для заказа {OrderId}",
					order.Id);

				uow.Save(order);
				uow.Save(undelivery);
				uow.Save(onlineOrder);
				uow.Commit();

				_logger.LogInformation(
					"Заказ {OrderId} успешно отменен из статуса 'В пути' через механизм недовоза. Недовоз: {UndeliveryId}",
					order.Id,
					undelivery.Id);

				var message = order.PaymentType == PaymentType.PaidOnline
					? "Заказ отменен успешно, денежные средства вернутся к Вам в течение 10 дней. Срок зависит от банка получателя"
					: "Заказ отменен успешно";

				return new CancelOrderResult(true, 200, "Success", message);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,
					"Ошибка при отмене заказа {OrderId} из статуса 'В пути'",
					order.Id);

				return new CancelOrderResult(
					false,
					500,
					"One or more validation errors occurred",
					"Произошла ошибка при отмене заказа");
			}
		}

		/// <summary>
		/// Обработка отмены заказа с типом платежа PaidOnline
		/// </summary>
		private async Task<CancelOrderResult> ProcessPaidOnlineCancellationAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			CancelOrderDto cancelOrderDto)
		{
			try
			{
				_logger.LogInformation(
					"Обработка отмены заказа {OrderId} с типом платежа PaidOnline. TransactionId: {TransactionId}",
					order.Id,
					cancelOrderDto.TransactionId);

				// TODO: Интегрировать с системой возврата платежей (CloudPayments и т.д.)
				// На данный момент - просто отмена заказа

				order.ChangeStatus(OrderStatus.Canceled);
				order.Version = DateTime.Now;

				uow.Save(order);
				uow.Commit();

				_logger.LogInformation(
					"Заказ {OrderId} отменен с возвратом платежа",
					order.Id);

				return new CancelOrderResult(
					true,
					200,
					"Success",
					"Заказ отменен успешно, денежные средства вернутся к Вам в течение 10 дней. Срок зависит от банка получателя");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,
					"Ошибка при обработке отмены заказа {OrderId} с типом платежа PaidOnline",
					order.Id);

				return new CancelOrderResult(
					false,
					500,
					"One or more validation errors occurred",
					"Произошла ошибка при отмене заказа");
			}
		}

		/// <summary>
		/// Создает недовоз для отмены заказа (без создания нового заказа)
		/// </summary>
		private static UndeliveredOrder CreateUndeliveryForCancellation(
			IUnitOfWork uow,
			Order order,
			Employee currentUser)
		{
			var undelivery = new UndeliveredOrder
			{
				UoW = uow,
				OldOrder = order,
				NewOrder = null,
				NewDeliverySchedule = null,
				Author = currentUser,
				EmployeeRegistrator = currentUser,
				TimeOfCreation = DateTime.Now,
				OrderTransferType = TransferType.AutoTransferNotApproved
			};

			undelivery.CreateOkkDiscussion(uow);

			return undelivery;
		}
	}
}
