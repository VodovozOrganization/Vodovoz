﻿using System;
using QS.DomainModel.UoW;
using Vodovoz.Controllers;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;
using Vodovoz.Tools.CallTasks;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class OrderConfirmationService : IOrderConfirmationService
	{
		private readonly IFastDeliveryHandler _fastDeliveryHandler;
		private readonly ICallTaskWorker _callTaskWorker;
		private readonly IOrderDailyNumberController _orderDailyNumberController;
		private readonly IPaymentFromBankClientController _paymentFromBankClientController;
		private readonly IOrderContractUpdater _orderContractUpdater;

		public OrderConfirmationService(
			IFastDeliveryHandler fastDeliveryHandler,
			ICallTaskWorker callTaskWorker,
			IOrderDailyNumberController orderDailyNumberController,
			IPaymentFromBankClientController paymentFromBankClientController,
			IOrderContractUpdater orderContractUpdater
			)
		{
			_fastDeliveryHandler = fastDeliveryHandler ?? throw new ArgumentNullException(nameof(fastDeliveryHandler));
			_callTaskWorker = callTaskWorker ?? throw new ArgumentNullException(nameof(callTaskWorker));
			_orderDailyNumberController = orderDailyNumberController ?? throw new ArgumentNullException(nameof(orderDailyNumberController));
			_paymentFromBankClientController =
				paymentFromBankClientController ?? throw new ArgumentNullException(nameof(paymentFromBankClientController));
			_orderContractUpdater = orderContractUpdater ?? throw new ArgumentNullException(nameof(orderContractUpdater));
		}

		public Result TryAcceptOrderCreatedByOnlineOrder(IUnitOfWork uow, Employee employee, Order order)
		{
			if(!order.SelfDelivery)
			{
				var fastDeliveryResult = _fastDeliveryHandler.CheckFastDelivery(uow, order);

				if(fastDeliveryResult.IsFailure)
				{
					return fastDeliveryResult;
				}

				_fastDeliveryHandler.TryAddOrderToRouteListAndNotifyDriver(uow, order, _callTaskWorker);
			}

			AcceptOrder(uow, employee, order);

			return Result.Success();
		}

		public void AcceptOrder(IUnitOfWork uow, Employee employee, Order order, bool needUpdateContract = true)
		{
			order.AcceptOrder(employee, _callTaskWorker);
			order.SaveEntity(
				uow, _orderContractUpdater, employee, _orderDailyNumberController, _paymentFromBankClientController, needUpdateContract);
		}
	}
}
