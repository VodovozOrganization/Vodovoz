using CustomerOrdersApi.Library.Dto.Orders;
using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Flyers;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Models.Orders;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Tools.CallTasks;
using VodovozBusiness.Services.Orders;

namespace CustomerOrdersApi.Library.Services
{
	public class OrderTransferService : IOrderTransferService
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ILogger<OrderTransferService> _logger;
		private readonly IOnlineOrderRepository _onlineOrderRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;
		private readonly IFlyerRepository _flyerRepository;
		private readonly IOrderContractUpdater _orderContractUpdater;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IRouteListService _routeListService;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly ICallTaskWorker _callTaskWorker;

		public OrderTransferService(
			IUnitOfWorkFactory unitOfWorkFactory,
			ILogger<OrderTransferService> logger,
			IOnlineOrderRepository onlineOrderRepository,
			IOrderRepository orderRepository,
			IRouteListItemRepository routeListItemRepository,
			IFlyerRepository flyerRepository,
			IOrderContractUpdater orderContractUpdater,
			IEmployeeRepository employeeRepository,
			IRouteListService routeListService,
			INomenclatureSettings nomenclatureSettings,
			ICallTaskWorker callTaskWorker
			)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_onlineOrderRepository = onlineOrderRepository ?? throw new ArgumentNullException(nameof(onlineOrderRepository));
			_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
			_routeListItemRepository = routeListItemRepository ?? throw new ArgumentNullException(nameof(routeListItemRepository));
			_flyerRepository = flyerRepository ?? throw new ArgumentNullException(nameof(flyerRepository));
			_orderContractUpdater = orderContractUpdater ?? throw new ArgumentNullException(nameof(orderContractUpdater));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_routeListService = routeListService ?? throw new ArgumentNullException(nameof(routeListService));
			_nomenclatureSettings = nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
		}

		private static readonly OrderStatus[] _simpleTransferStatuses = new[]
		{
			OrderStatus.NewOrder,
			OrderStatus.WaitForPayment,
			OrderStatus.Accepted
		};

		private static readonly OrderStatus[] _complexTransferStatuses = new[]
		{
			OrderStatus.InTravelList,
			OrderStatus.OnLoading,
			OrderStatus.OnTheWay
		};

		private static readonly OrderStatus[] _allowedTransferStatuses =
			_simpleTransferStatuses.Concat(_complexTransferStatuses).ToArray();

		public async Task<TransferOrderResult> TransferOrderAsync(TransferOrderDto transferOrderDto)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot();
			try
			{
				var onlineOrder = _onlineOrderRepository.GetOnlineOrderByExternalId(uow, transferOrderDto.ExternalOrderId);

				if(onlineOrder == null)
				{
					_logger.LogWarning(
						"Попытка переноса несуществующего заказа: ExternalOrderId: {ExternalOrderId}",
						transferOrderDto.ExternalOrderId);

					return new TransferOrderResult(false, 404, "One or more validation errors occurred", "Заказ не найден");
				}

				var (canTransfer, statusTitle, requiresComplexHandling, errorMessage) =
					CanTransferOrder(uow, transferOrderDto.ExternalOrderId);

				if(!canTransfer)
				{
					_logger.LogWarning(
						"Перенос заказа {ExternalOrderId} невозможен, причина: {Reason}",
						transferOrderDto.ExternalOrderId,
						errorMessage);

					return new TransferOrderResult(false, 408, "One or more validation errors occurred", errorMessage);
				}

				if(!onlineOrder.Orders.Any())
				{
					_logger.LogWarning(
						"Попытка переноса онлайн заказа {ExternalOrderId} без привязанного заказа",
						transferOrderDto.ExternalOrderId);

					return new TransferOrderResult(
						false,
						400,
						"One or more validation errors occurred",
						"Онлайн заказ не имеет привязанного заказа. Сначала необходимо создать заказ.");
				}

				var undeliveryStatuses = _orderRepository.GetUndeliveryStatuses();
				var order = onlineOrder.Orders.FirstOrDefault(x => !undeliveryStatuses.Contains(x.OrderStatus));

				if(requiresComplexHandling)
				{
					return await TransferComplexOrderAsync(uow, order, onlineOrder, transferOrderDto);
				}
				else
				{
					return await TransferSimpleOrderAsync(uow, order, onlineOrder, transferOrderDto);
				}

				throw new InvalidOperationException($"Неизвестный статус заказа: {order.OrderStatus}");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,
					"Ошибка при переносе заказа {ExternalOrderId}",
					transferOrderDto.ExternalOrderId);

				return new TransferOrderResult(false, 500, "One or more validation errors occurred", "Произошла ошибка, пожалуйста, попробуйте позже");
			}
		}

		private (bool canTransfer, string statusTitle, bool requiresComplexHandling, string errorMessage) CanTransferOrder(IUnitOfWork uow, Guid externalOrderId)
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

			if(!_allowedTransferStatuses.Contains(order.OrderStatus))
			{
				return (
					false,
					order.OrderStatus.GetEnumTitle(),
					default,
					$"Невозможно перенести заказ в статусе '{order.OrderStatus.GetEnumTitle()}'"
				);
			}

			return (
				true,
				order.OrderStatus.GetEnumTitle(),
				_complexTransferStatuses.Contains(order.OrderStatus),
				default
			);
		}

		/// <summary>
		/// Простой перенос заказа (статусы: Новый, Ожидание оплаты, Принят)
		/// Просто меняем дату и время доставки
		/// </summary>
		private async Task<TransferOrderResult> TransferSimpleOrderAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			TransferOrderDto transferOrderDto)
		{
			try
			{
				_logger.LogInformation(
					"Начало переноса заказа {OrderId} в статусе {Status}",
					order.Id,
					order.OrderStatus.GetEnumTitle());

				order.TransferToNewDateAndSchedule(
					transferOrderDto.DeliveryDate,
					transferOrderDto.DeliveryScheduleId,
					_orderContractUpdater,
					out string orderMessage);

				onlineOrder.DeliveryDate = transferOrderDto.DeliveryDate;
				onlineOrder.DeliveryScheduleId = transferOrderDto.DeliveryScheduleId;

				uow.Save(order);
				uow.Save(onlineOrder);
				uow.Commit();

				_logger.LogInformation(
					"Перенос заказа {OrderId} на дату {NewDate} успешно завершен",
					order.Id,
					transferOrderDto.DeliveryDate);

				return new TransferOrderResult(true, 200, "Success", "Заказ перенесен успешно");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,
					"Ошибка при простом переносе заказа {OrderId}",
					order.Id);

				return new TransferOrderResult(false, 500, "One or more validation errors occurred", "Произошла ошибка при переносе заказа");
			}
		}

		/// <summary>
		/// Сложный перенос заказа (статусы: В маршрутном листе, На погрузке, В пути)
		/// </summary>
		private async Task<TransferOrderResult> TransferComplexOrderAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			TransferOrderDto transferOrderDto)
		{
			try
			{
				return order.OrderStatus switch
				{
					OrderStatus.InTravelList =>
						await TransferOrderFromTravelListAsync(uow, order, onlineOrder, transferOrderDto),

					OrderStatus.OnLoading or OrderStatus.OnTheWay =>
						await TransferOrderFromOnTheWayOrOnLoadingAsync(uow, order, onlineOrder, transferOrderDto),

					_ => throw new InvalidOperationException($"Неожиданный статус заказа: {order.OrderStatus}")
				};
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,
					"Ошибка при переносе заказа {OrderId} в статусе {Status}",
					order.Id,
					order.OrderStatus.GetEnumTitle());

				return new TransferOrderResult(
					false,
					500,
					"One or more validation errors occurred",
					"Произошла ошибка при переносе заказа");
			}
		}

		/// <summary>
		/// Перенос заказа из маршрутного листа (статус: В маршрутном листе)
		/// </summary>
		private async Task<TransferOrderResult> TransferOrderFromTravelListAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			TransferOrderDto transferOrderDto)
		{
			try
			{
				_logger.LogInformation(
					"Перенос заказа {OrderId} из маршрутного листа",
					order.Id);

				var routeListItem = _routeListItemRepository.GetRouteListItemForOrder(uow, order);

				if(routeListItem == null)
				{
					_logger.LogWarning(
						"Позиция маршрутного листа не найдена для заказа {OrderId}",
						order.Id);

					return new TransferOrderResult(
						false,
						400,
						"One or more validation errors occurred",
						"Позиция маршрутного листа не найдена");
				}

				var routeList = routeListItem.RouteList;
				routeList.RemoveAddress(routeListItem);
				routeList.Version = DateTime.Now;

				order.ChangeStatus(OrderStatus.Accepted);
				order.Version = DateTime.Now;

				order.TransferToNewDateAndSchedule(
					transferOrderDto.DeliveryDate,
					transferOrderDto.DeliveryScheduleId,
					_orderContractUpdater,
					out string orderMessage);

				onlineOrder.DeliveryDate = transferOrderDto.DeliveryDate;
				onlineOrder.DeliveryScheduleId = transferOrderDto.DeliveryScheduleId;

				uow.Save(routeList);
				uow.Save(order);
				uow.Save(onlineOrder);
				uow.Commit();

				_logger.LogInformation(
					"Заказ {OrderId} успешно перенесен из маршрутного листа на {NewDate}",
					order.Id,
					transferOrderDto.DeliveryDate);

				return new TransferOrderResult(true, 200, "Success", "Заказ перенесен успешно");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,
					"Ошибка при переносе заказа {OrderId} из маршрутного листа",
					order.Id);

				return new TransferOrderResult(
					false,
					500,
					"One or more validation errors occurred",
					"Произошла ошибка при переносе заказа");
			}
		}

		/// <summary>
		/// Перенос заказа со статусами "На погрузке" и "В пути" с использованием механизма недовоза.
		/// Создается копия заказа с новой датой доставки, старый заказ отменяется через недовоз
		/// </summary>
		private async Task<TransferOrderResult> TransferOrderFromOnTheWayOrOnLoadingAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			TransferOrderDto transferOrderDto)
		{
			try
			{
				_logger.LogInformation(
					"Начало переноса заказа {OrderId} из статуса 'В пути' с использованием механизма недовоза",
					order.Id);

				var currentUser = _employeeRepository.GetEmployeeForCurrentUser(uow);
				if(currentUser == null)
				{
					_logger.LogWarning(
						"Не удалось получить текущего пользователя для переноса заказа {OrderId}",
						order.Id);

					return new TransferOrderResult(
						false,
						500,
						"One or more validation errors occurred",
						"Не удалось получить информацию о пользователе");
				}

				var deliverySchedule = uow.GetById<DeliverySchedule>(transferOrderDto.DeliveryScheduleId);
				if(deliverySchedule == null)
				{
					_logger.LogWarning(
						"Расписание доставки не найдено: {DeliveryScheduleId}",
						transferOrderDto.DeliveryScheduleId);

					return new TransferOrderResult(
						false,
						400,
						"One or more validation errors occurred",
						"Расписание доставки не найдено");
				}

				var newOrder = CreateOrderCopy(uow, order, transferOrderDto.DeliveryDate, deliverySchedule);

				_logger.LogInformation(
					"Создана копия заказа {OrderId}: новый заказ {NewOrderId}",
					order.Id,
					newOrder.Id);

				var undelivery = CreateUndeliveryForTransfer(
					uow,
					order,
					newOrder,
					currentUser,
					deliverySchedule);

				order.SetUndeliveredStatus(
					uow,
					_routeListService,
					_nomenclatureSettings,
					_callTaskWorker,
					needCreateDeliveryFreeBalanceOperation: false);

				_logger.LogInformation(
					"Установлен статус 'Недовезено' для заказа {OrderId}",
					order.Id);

				onlineOrder.Orders.Add(newOrder);
				onlineOrder.DeliveryDate = transferOrderDto.DeliveryDate;
				onlineOrder.DeliveryScheduleId = transferOrderDto.DeliveryScheduleId;

				uow.Save(order);
				uow.Save(newOrder);
				uow.Save(undelivery);
				uow.Save(onlineOrder);
				uow.Commit();

				_logger.LogInformation(
					"Заказ {OrderId} успешно перенесен из статуса 'В пути' на дату {NewDate} через механизм недовоза. Новый заказ: {NewOrderId}, Недовоз: {UndeliveryId}",
					order.Id,
					transferOrderDto.DeliveryDate,
					newOrder.Id,
					undelivery.Id);

				return new TransferOrderResult(true, 200, "Success", "Заказ перенесен успешно");
			}
			catch(Exception ex)
			{
				_logger.LogError(ex,
					"Ошибка при переносе заказа {OrderId} из статуса 'В пути'",
					order.Id);

				return new TransferOrderResult(
					false,
					500,
					"One or more validation errors occurred",
					"Произошла ошибка при переносе заказа");
			}
		}

		/// <summary>
		/// Создает копию заказа с новой датой доставки
		/// </summary>
		private Order CreateOrderCopy(
			IUnitOfWork uow,
			Order originalOrder,
			DateTime newDeliveryDate,
			DeliverySchedule newDeliverySchedule)
		{
			var newOrder = new Order();
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
				newDeliverySchedule.Id,
				_orderContractUpdater,
				out _);
			newOrder.OrderStatus = OrderStatus.Accepted;
			newOrder.IsCopiedFromUndelivery = true;

			if(copying.GetCopiedOrder.PaymentType == PaymentType.PaidOnline)
			{
				copying.CopyPaymentByCardDataIfPossible();
			}

			if(copying.GetCopiedOrder.PaymentType == PaymentType.DriverApplicationQR
				|| copying.GetCopiedOrder.PaymentType == PaymentType.SmsQR)
			{
				copying.CopyPaymentByQrDataIfPossible();
			}

			uow.Save(newOrder);

			return newOrder;
		}

		private static UndeliveredOrder CreateUndeliveryForTransfer(
			IUnitOfWork uow,
			Order oldOrder,
			Order newOrder,
			Employee currentUser,
			DeliverySchedule newDeliverySchedule)
		{
			var undelivery = new UndeliveredOrder
			{
				UoW = uow,
				OldOrder = oldOrder,
				NewOrder = newOrder,
				NewDeliverySchedule = newDeliverySchedule,
				Author = currentUser,
				EmployeeRegistrator = currentUser,
				TimeOfCreation = DateTime.Now,
				OrderTransferType = TransferType.AutoTransferApproved // Или TransferredByCounterparty?
			};

			undelivery.CreateOkkDiscussion(uow);

			return undelivery;
		}
	}
}
