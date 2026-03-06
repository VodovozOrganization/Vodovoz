using CustomerOrdersApi.Library.Dto.Orders;
using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Threading.Tasks;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Flyers;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Models.Orders;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Tools.CallTasks;
using VodovozBusiness.Services.Orders;

namespace CustomerOrdersApi.Library.Services
{
	public class OrderTransferService : BaseOrderOperationService<TransferOrderDto, TransferOrderResult>, IOrderTransferService
	{
		private readonly IFlyerRepository _flyerRepository;
		private readonly IOrderContractUpdater _orderContractUpdater;

		public OrderTransferService(
			IUnitOfWorkFactory unitOfWorkFactory,
			ILogger<OrderTransferService> logger,
			IOnlineOrderRepository onlineOrderRepository,
			IOrderRepository orderRepository,
			IRouteListItemRepository routeListItemRepository,
			IFlyerRepository flyerRepository,
			IOrderContractUpdater orderContractUpdater,
			IEmployeeRepository employeeRepository,
			ISubdivisionRepository subdivisionRepository,
			IRouteListService routeListService,
			INomenclatureSettings nomenclatureSettings,
			ICallTaskWorker callTaskWorker)
			: base(
				unitOfWorkFactory,
				logger,
				onlineOrderRepository,
				orderRepository,
				routeListItemRepository,
				employeeRepository,
				subdivisionRepository,
				routeListService,
				nomenclatureSettings,
				callTaskWorker)
		{
			_flyerRepository = flyerRepository ?? throw new ArgumentNullException(nameof(flyerRepository));
			_orderContractUpdater = orderContractUpdater ?? throw new ArgumentNullException(nameof(orderContractUpdater));
		}

		public async Task<TransferOrderResult> TransferOrderAsync(TransferOrderDto transferOrderDto) => await ExecuteAsync(transferOrderDto);

		protected override string OperationName => "Перенос";

		protected override string GetSuccessMessage() => "Заказ перенесен успешно";

		protected override async Task<TransferOrderResult> ProcessSimpleOperationAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			TransferOrderDto dto)
		{
			_logger.LogInformation(
				"Начало простого переноса заказа {OrderId} в статусе {Status}",
				order.Id,
				order.OrderStatus.GetEnumTitle());

			order.TransferToNewDateAndSchedule(
				dto.DeliveryDate,
				dto.DeliveryScheduleId,
				_orderContractUpdater,
				out _);

			uow.Save(order);
			uow.Commit();

			_logger.LogInformation(
				"Простой перенос заказа {OrderId} на дату {NewDate} успешно завершен",
				order.Id,
				dto.DeliveryDate);

			return CreateSuccessResult(GetSuccessMessage());
		}

		protected override async Task<TransferOrderResult> ProcessComplexOperationAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			TransferOrderDto dto)
		{
			return order.OrderStatus switch
			{
				OrderStatus.InTravelList =>
					await TransferFromTravelListAsync(uow, order, onlineOrder, dto),

				OrderStatus.OnLoading or OrderStatus.OnTheWay =>
					await TransferWithUndeliveryAsync(uow, order, onlineOrder, dto),

				_ => throw new InvalidOperationException($"Неожиданный статус заказа: {order.OrderStatus}")
			};
		}
		
		protected override async Task<OperationValidationResult> ValidateSpecificAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			TransferOrderDto dto)
		{
			if(dto.DeliveryScheduleId <= 0)
			{
				return OperationValidationResult.Invalid("Не указано время доставки");
			}

			if(dto.DeliveryDate.Date < DateTime.Now.Date)
			{
				return OperationValidationResult.Invalid("Дата доставки не может быть назначена на прошлую дату");
			}

			if(order.DeliverySchedule != null &&
				dto.DeliveryScheduleId == order.DeliverySchedule.Id &&
				order.DeliveryDate.HasValue &&
				dto.DeliveryDate.Date == order.DeliveryDate.Value.Date)
			{
				return OperationValidationResult.Invalid("Заказ уже запланирован на то же время доставки");
			}

			if(order.DeliveryDate.HasValue && dto.DeliveryDate.Date == order.DeliveryDate.Value.Date)
			{
				_logger.LogWarning(
					"Попытка переноса заказа {ExternalOrderId} на ту же дату доставки {DeliveryDate}",
					dto.ExternalOrderId,
					dto.DeliveryDate);
			}

			return OperationValidationResult.Valid();
		}

		private async Task<TransferOrderResult> TransferFromTravelListAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			TransferOrderDto dto)
		{
			_logger.LogInformation(
				"Перенос заказа {OrderId} из маршрутного листа",
				order.Id);

			var routeListItem = _routeListItemRepository.GetRouteListItemForOrder(uow, order);

			if(routeListItem is null)
			{
				_logger.LogWarning(
					"Позиция маршрутного листа не найдена для заказа {OrderId}",
					order.Id);

				var result = new TransferOrderResult
				{
					IsSuccess = false,
					StatusCode = 400,
					Title = "One or more validation errors occurred",
					DetailMessage = "Позиция маршрутного листа не найдена"
				};
				return result;
			}

			var routeList = routeListItem.RouteList;
			routeList.RemoveAddress(routeListItem);
			routeList.Version = DateTime.Now;

			order.ChangeStatus(OrderStatus.Accepted);
			order.Version = DateTime.Now;
			order.TransferToNewDateAndSchedule(
				dto.DeliveryDate,
				dto.DeliveryScheduleId,
				_orderContractUpdater,
				out _);

			onlineOrder.DeliveryDate = dto.DeliveryDate;
			onlineOrder.DeliveryScheduleId = dto.DeliveryScheduleId;

			uow.Save(routeList);
			uow.Save(order);
			uow.Save(onlineOrder);
			uow.Commit();

			_logger.LogInformation(
				"Заказ {OrderId} успешно перенесен из маршрутного листа на {NewDate}",
				order.Id,
				dto.DeliveryDate);

			var successResult = new TransferOrderResult
			{
				IsSuccess = true,
				StatusCode = 200,
				Title = "Success",
				DetailMessage = GetSuccessMessage()
			};
			return successResult;
		}

		private async Task<TransferOrderResult> TransferWithUndeliveryAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			TransferOrderDto dto)
		{
			_logger.LogInformation(
				"Начало переноса заказа {OrderId} из статуса '{Status}' с использованием механизма недовоза",
				order.Id,
				order.OrderStatus.GetEnumTitle());

			//var currentUser = _employeeRepository.GetEmployeeForCurrentUser(uow);
			var currentUser = uow.GetById<Employee>(1468);
			if(currentUser is null)
			{
				_logger.LogWarning(
					"Не удалось получить текущего пользователя для переноса заказа {OrderId}",
					order.Id);

				var result = new TransferOrderResult
				{
					IsSuccess = false,
					StatusCode = 500,
					Title = "One or more validation errors occurred",
					DetailMessage = "Не удалось получить информацию о пользователе"
				};
				return result;
			}

			var deliverySchedule = uow.GetById<DeliverySchedule>(dto.DeliveryScheduleId);
			if(deliverySchedule is null)
			{
				_logger.LogWarning(
					"Расписание доставки не найдено: {DeliveryScheduleId}",
					dto.DeliveryScheduleId);

				var result = new TransferOrderResult
				{
					IsSuccess = false,
					StatusCode = 400,
					Title = "One or more validation errors occurred",
					DetailMessage = "Расписание доставки не найдено"
				};
				return result;
			}

			var newOrder = CreateOrderCopy(uow, order, dto.DeliveryDate, deliverySchedule);

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
			onlineOrder.DeliveryDate = dto.DeliveryDate;
			onlineOrder.DeliveryScheduleId = dto.DeliveryScheduleId;

			uow.Save(order);
			uow.Save(newOrder);
			uow.Save(undelivery);
			uow.Save(onlineOrder);
			uow.Commit();

			_logger.LogInformation(
				"Заказ {OrderId} успешно перенесен из статуса '{Status}' на дату {NewDate} через механизм недовоза. Новый заказ: {NewOrderId}, Недовоз: {UndeliveryId}",
				order.Id,
				order.OrderStatus.GetEnumTitle(),
				dto.DeliveryDate,
				newOrder.Id,
				undelivery.Id);

			var successResult = new TransferOrderResult
			{
				IsSuccess = true,
				StatusCode = 200,
				Title = "Success",
				DetailMessage = GetSuccessMessage()
			};
			return successResult;
		}

		private Order CreateOrderCopy(
			IUnitOfWork uow,
			Order originalOrder,
			DateTime newDeliveryDate,
			DeliverySchedule newDeliverySchedule)
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
				newDeliverySchedule.Id,
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

			uow.Save(newOrder);

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
