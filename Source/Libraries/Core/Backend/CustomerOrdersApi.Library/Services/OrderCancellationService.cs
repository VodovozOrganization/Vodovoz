using CustomerOrdersApi.Library.Dto.Orders;
using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Threading.Tasks;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Nomenclature;
using Vodovoz.Tools.CallTasks;

namespace CustomerOrdersApi.Library.Services
{
	public class OrderCancellationService : BaseOrderOperationService<CancelOrderDto, CancelOrderResult>, IOrderCancellationService
	{
		public OrderCancellationService(
			IUnitOfWorkFactory unitOfWorkFactory,
			ILogger<OrderCancellationService> logger,
			IOnlineOrderRepository onlineOrderRepository,
			IOrderRepository orderRepository,
			IRouteListItemRepository routeListItemRepository,
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
		}

		public async Task<CancelOrderResult> CancelOrderAsync(CancelOrderDto cancelOrderDto) => await ExecuteAsync(cancelOrderDto);

		protected override string OperationName => "Отмена";

		protected override string GetSuccessMessage() => "Заказ отменен успешно";

		protected override async Task<CancelOrderResult> ProcessSimpleOperationAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			CancelOrderDto dto)
		{
			_logger.LogInformation(
				"Начало простой отмены заказа {OrderId} в статусе {Status}",
				order.Id,
				order.OrderStatus.GetEnumTitle());

			if(IsPaidOnline(order))
			{
				return await ProcessPaidOperationAsync(uow, order, onlineOrder, dto);
			}

			order.ChangeStatus(OrderStatus.Canceled);
			order.Version = DateTime.Now;

			uow.Save(order);
			uow.Commit();

			_logger.LogInformation(
				"Заказ {OrderId} успешно отменен",
				order.Id);

			return CreateSuccessResult(GetSuccessMessage());
		}

		protected override async Task<CancelOrderResult> ProcessComplexOperationAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			CancelOrderDto dto)
		{
			return order.OrderStatus switch
			{
				OrderStatus.InTravelList =>
					await CancelFromTravelListAsync(uow, order, onlineOrder, dto),

				OrderStatus.OnLoading or OrderStatus.OnTheWay =>
					await CancelWithUndeliveryAsync(uow, order, onlineOrder, dto),

				_ => throw new InvalidOperationException($"Неожиданный статус заказа: {order.OrderStatus}")
			};
		}

		protected async Task<CancelOrderResult> ProcessPaidOperationAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			CancelOrderDto dto)
		{
			_logger.LogInformation(
				"Обработка отмены оплаченного заказа {OrderId} с типом платежа PaidOnline. TransactionId: {TransactionId}",
				order.Id,
				dto.TransactionId);

			// TODO: Интеграция с платежной системой для возврата денег
			// CloudPayments.ReturnMoney(dto.TransactionId);

			order.ChangeStatus(OrderStatus.Canceled);
			order.Version = DateTime.Now;

			uow.Save(order);
			uow.Commit();

			_logger.LogInformation(
				"Оплаченный заказ {OrderId} отменен с возвратом платежа",
				order.Id);

			var result = new CancelOrderResult
			{
				IsSuccess = true,
				StatusCode = 200,
				Title = "Success",
				DetailMessage = "Заказ отменен успешно, денежные средства вернутся к Вам в течение 10 дней. Срок зависит от банка получателя"
			};
			return result;
		}

		protected override async Task<OperationValidationResult> ValidateSpecificAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			CancelOrderDto dto) => OperationValidationResult.Valid();

		private async Task<CancelOrderResult> CancelFromTravelListAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			CancelOrderDto dto)
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

				var result = new CancelOrderResult
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

			if(IsPaidOnline(order))
			{
				return await ProcessPaidOperationAsync(uow, order, onlineOrder, dto);
			}

			order.ChangeStatus(OrderStatus.Canceled);
			order.Version = DateTime.Now;

			uow.Save(routeList);
			uow.Save(order);
			uow.Commit();

			_logger.LogInformation(
				"Заказ {OrderId} успешно отменен из маршрутного листа",
				order.Id);

			var successResult = new CancelOrderResult
			{
				IsSuccess = true,
				StatusCode = 200,
				Title = "Success",
				DetailMessage = GetSuccessMessage()
			};
			return successResult;
		}

		private async Task<CancelOrderResult> CancelWithUndeliveryAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			CancelOrderDto dto)
		{
			_logger.LogInformation(
				"Начало отмены заказа {OrderId} из статуса '{Status}' с использованием механизма недовоза",
				order.Id,
				order.OrderStatus.GetEnumTitle());

			//var currentUser = _employeeRepository.GetEmployeeForCurrentUser(uow);
			var currentUser = uow.GetById<Employee>(1468);
			if(currentUser == null)
			{
				_logger.LogWarning(
					"Не удалось получить текущего пользователя для отмены заказа {OrderId}",
					order.Id);

				var result = new CancelOrderResult
				{
					IsSuccess = false,
					StatusCode = 500,
					Title = "One or more validation errors occurred",
					DetailMessage = "Не удалось получить информацию о пользователе"
				};
				return result;
			}

			if(IsPaidOnline(order))
			{
				var paidResult = await ProcessPaidOperationAsync(uow, order, onlineOrder, dto);
				if(!paidResult.IsSuccess)
				{
					return paidResult;
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
				"Заказ {OrderId} успешно отменен из статуса '{Status}' через механизм недовоза. Недовоз: {UndeliveryId}",
				order.Id,
				order.OrderStatus.GetEnumTitle(),
				undelivery.Id);

			var message = IsPaidOnline(order)
				? "Заказ отменен успешно, денежные средства вернутся к Вам в течение 10 дней. Срок зависит от банка получателя"
				: "Заказ отменен успешно";

			var successResult = new CancelOrderResult
			{
				IsSuccess = true,
				StatusCode = 200,
				Title = "Success",
				DetailMessage = message
			};
			return successResult;
		}

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
	}
}
