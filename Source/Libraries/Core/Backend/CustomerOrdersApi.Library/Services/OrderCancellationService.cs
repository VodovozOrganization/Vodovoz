using CustomerOrdersApi.Library.Dto.Orders;
using CustomerOrdersApi.Library.Dto.Orders.CancelOrder;
using CustomerOrdersApi.Library.Factories;
using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Payments;
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
		private readonly IPaymentRefundServiceFactory _paymentRefundServiceFactory;

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
			ICallTaskWorker callTaskWorker,
			IPaymentRefundServiceFactory paymentRefundServiceFactory)
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
			_paymentRefundServiceFactory = paymentRefundServiceFactory ?? throw new ArgumentNullException(nameof(paymentRefundServiceFactory));
		}

		public async Task<CancelOrderResult> CancelOrderAsync(CancelOrderDto cancelOrderDto, CancellationToken cancellationToken) => await ExecuteAsync(cancelOrderDto, cancellationToken);

		protected override string OperationName => "Отмена";

		protected override string GetSuccessMessage() => "Заказ отменен успешно";

		protected override async Task<CancelOrderResult> ProcessSimpleOperationAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			CancelOrderDto dto,
			CancellationToken cancellationToken)
		{
			_logger.LogInformation(
				"Начало простой отмены заказа {OrderId} в статусе {Status}",
				order.Id,
				order.OrderStatus.GetEnumTitle());

			if(IsPaidOnline(onlineOrder))
			{
				return await ProcessPaidOperationAsync(uow, order, onlineOrder, dto, cancellationToken);
			}

			order.ChangeStatus(OrderStatus.Canceled);
			order.Version = DateTime.Now;

			await uow.SaveAsync(order, cancellationToken: cancellationToken);
			await uow.CommitAsync(cancellationToken);

			_logger.LogInformation(
				"Заказ {OrderId} успешно отменен",
				order.Id);

			return CreateSuccessResult(GetSuccessMessage());
		}

		protected override async Task<CancelOrderResult> ProcessComplexOperationAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			CancelOrderDto dto, 
			CancellationToken cancellationToken)
		{
			return order.OrderStatus switch
			{
				OrderStatus.InTravelList =>
					await CancelFromTravelListAsync(uow, order, onlineOrder, dto, cancellationToken),

				OrderStatus.OnLoading or OrderStatus.OnTheWay =>
					await CancelWithUndeliveryAsync(uow, order, onlineOrder, dto, cancellationToken),

				_ => throw new InvalidOperationException($"Неожиданный статус заказа: {order.OrderStatus}")
			};
		}

		protected async Task<CancelOrderResult> ProcessPaidOperationAsync(
			IUnitOfWork uow,
			Order order,
			OnlineOrder onlineOrder,
			CancelOrderDto dto,
			CancellationToken cancellationToken)
		{
			_logger.LogInformation(
				"Обработка отмены оплаченного заказа {OrderId}. " +
				"Тип оплаты: {PaymentType}, TransactionId: {TransactionId}, PaymentSource: {PaymentSource}",
				order.Id,
				order.PaymentType,
				dto.TransactionId,
				onlineOrder?.OnlinePaymentSource);

			var refundService = _paymentRefundServiceFactory.GetRefundService(onlineOrder);

			var refundRequest = new RefundRequestDto(
				OnlineOrder: onlineOrder,
				TransactionId: dto.TransactionId,
				Amount: onlineOrder.OnlineOrderSum,
				ExternalOrderId: onlineOrder?.ExternalOrderId.ToString()
			);

			var refundResult = await refundService.ProcessRefundAsync(uow, refundRequest, cancellationToken);

			if(!refundResult.Success)
			{
				_logger.LogWarning(
					"Не удалось выполнить возврат для заказа {OrderId}: {ErrorMessage}",
					order.Id,
					refundResult.ErrorMessage);

				return new CancelOrderResult
				{
					IsSuccess = false,
					StatusCode = 400,
					Title = "Payment refund failed",
					DetailMessage = "Не удалось выполнить возврат платежа. Пожалуйста, обратитесь в поддержку."
				};
			}

			if(onlineOrder is not null && refundResult.NewPaymentStatus != onlineOrder.OnlineOrderPaymentStatus)
			{
				onlineOrder.OnlineOrderPaymentStatus = refundResult.NewPaymentStatus;
				_logger.LogInformation(
					"Статус оплаты онлайн заказа {OnlineOrderId} обновлен на {NewStatus}",
					onlineOrder.Id,
					refundResult.NewPaymentStatus);
			}

			order.ChangeStatus(OrderStatus.Canceled);
			order.Version = DateTime.Now;

			await uow.SaveAsync(order, cancellationToken: cancellationToken);

			if(onlineOrder != null)
			{
				await uow.SaveAsync(onlineOrder, cancellationToken: cancellationToken); 
			}

			await uow.CommitAsync(cancellationToken);

			_logger.LogInformation(
				"Оплаченный заказ {OrderId} отменен.", order.Id);

			var message = "Заказ отменен, возврат средств инициирован. Статус возврата можно отслеживать в истории заказов.";

			var result = new CancelOrderResult
			{
				IsSuccess = true,
				StatusCode = 200,
				Title = "Success",
				DetailMessage = message
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
			CancelOrderDto dto, 
			CancellationToken cancellationToken)
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

			if(IsPaidOnline(onlineOrder))
			{
				return await ProcessPaidOperationAsync(uow, order, onlineOrder, dto, cancellationToken);
			}

			order.ChangeStatus(OrderStatus.Canceled);
			order.Version = DateTime.Now;

			await uow.SaveAsync(routeList, cancellationToken: cancellationToken);
			await uow.SaveAsync(order, cancellationToken: cancellationToken);
			await uow.CommitAsync(cancellationToken);

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
			CancelOrderDto dto, 
			CancellationToken cancellationToken)
		{
			_logger.LogInformation(
				"Начало отмены заказа {OrderId} из статуса '{Status}' с использованием механизма недовоза",
				order.Id,
				order.OrderStatus.GetEnumTitle());

			var currentUser = await _employeeRepository.GetEmployeeBySourceAsync(uow, dto.Source, cancellationToken);
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

			if(IsPaidOnline(onlineOrder))
			{
				var paidResult = await ProcessPaidOperationAsync(uow, order, onlineOrder, dto, cancellationToken);
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

			await uow.SaveAsync(order, cancellationToken: cancellationToken);
			await uow.SaveAsync(undelivery, cancellationToken: cancellationToken);
			await uow.SaveAsync(onlineOrder, cancellationToken: cancellationToken);
			await uow.CommitAsync(cancellationToken);

			_logger.LogInformation(
				"Заказ {OrderId} успешно отменен из статуса '{Status}' через механизм недовоза. Недовоз: {UndeliveryId}",
				order.Id,
				order.OrderStatus.GetEnumTitle(),
				undelivery.Id);

			var message = IsPaidOnline(onlineOrder)
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
