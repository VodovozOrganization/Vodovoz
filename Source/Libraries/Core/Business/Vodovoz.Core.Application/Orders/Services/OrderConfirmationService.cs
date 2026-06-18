using CustomerNotifications.Contracts;
using Notifications.Infrastructure;
using QS.DomainModel.UoW;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Orders.OrderEnums;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Services.Logistics;
using Vodovoz.Tools.CallTasks;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Core.Application.Orders.Services
{
	public class OrderConfirmationService : IOrderConfirmationService
	{
		private readonly IFastDeliveryHandler _fastDeliveryHandler;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly IOrderDailyNumberController _orderDailyNumberController;
		private readonly IPaymentFromBankClientController _paymentFromBankClientController;
		private readonly IOrderContractUpdater _orderContractUpdater;
		private readonly IOutboxNotificationPublisher<CustomerNotificationDomainEvent> _customerNotificationPublisher;

		public OrderConfirmationService(
			IFastDeliveryHandler fastDeliveryHandler,
			ICallTaskWorker callTaskWorker,
			IOrderDailyNumberController orderDailyNumberController,
			IPaymentFromBankClientController paymentFromBankClientController,
			IOrderContractUpdater orderContractUpdater,
			IOutboxNotificationPublisher<CustomerNotificationDomainEvent> customerNotificationPublisher
			)
		{
			_fastDeliveryHandler = fastDeliveryHandler ?? throw new ArgumentNullException(nameof(fastDeliveryHandler));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_orderDailyNumberController = orderDailyNumberController ?? throw new ArgumentNullException(nameof(orderDailyNumberController));
			_paymentFromBankClientController =
				paymentFromBankClientController ?? throw new ArgumentNullException(nameof(paymentFromBankClientController));
			_orderContractUpdater = orderContractUpdater ?? throw new ArgumentNullException(nameof(orderContractUpdater));
			_customerNotificationPublisher = customerNotificationPublisher ?? throw new ArgumentNullException(nameof(customerNotificationPublisher));
		}

		public async Task<Result<bool>> TryAcceptOrderCreatedByOnlineOrderAsync(
			IUnitOfWork uow,
			Employee employee,
			Order order,
			IRouteListService routeListService,
			CancellationToken cancellationToken
		)
		{
			Result<bool> addingFastDeliveryOrderToRouteListResult = null;

			if(!order.SelfDelivery)
			{
				var fastDeliveryResult = await _fastDeliveryHandler.CheckFastDeliveryAsync(uow, order, cancellationToken);

				if(fastDeliveryResult.IsFailure)
				{
					return Result.Failure<bool>(fastDeliveryResult.Errors);
				}

				// Необходимо сделать асинхронным
				addingFastDeliveryOrderToRouteListResult = _fastDeliveryHandler.TryAddOrderToRouteList(
					uow,
					order,
					routeListService, 
					_callTaskWorker,
					employee);
				
				if(addingFastDeliveryOrderToRouteListResult.IsFailure)
				{
					return addingFastDeliveryOrderToRouteListResult;
				}
			}

			// Необходимо сделать асинхронным
			AcceptOrder(uow, employee, order);

			if(addingFastDeliveryOrderToRouteListResult is null)
			{
				return Result.Success(false);
			}
			else
			{
				return addingFastDeliveryOrderToRouteListResult;
			}			
		}

		public void AcceptOrder(IUnitOfWork uow, Employee employee, Order order, bool needUpdateContract = true)
		{
			order.AcceptOrder(employee, _callTaskWorker);
			order.SaveEntity(
				uow, 
				_orderContractUpdater, 
				employee, 
				_orderDailyNumberController, 
				_paymentFromBankClientController, 
				needUpdateContract
			);
		}
	}
}
