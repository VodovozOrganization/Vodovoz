using System;
using System.Collections.Generic;
using System.Linq;
using TaxcomEdo.Contracts.Orders;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Controllers;

namespace Vodovoz.Converters
{
	public class OrderConverter : IOrderConverter
	{
		private readonly ICounterpartyConverter _counterpartyConverter;
		private readonly IDeliveryPointConverter _deliveryPointConverter;
		private readonly ICounterpartyContractConverter _counterpartyContractConverter;
		private readonly IOrderItemConverter _orderItemConverter;
		private readonly ICounterpartyEdoAccountController _counterpartyEdoAccountController;

		public OrderConverter(
			ICounterpartyConverter counterpartyConverter,
			IDeliveryPointConverter deliveryPointConverter,
			ICounterpartyContractConverter counterpartyContractConverter,
			IOrderItemConverter orderItemConverter,
			ICounterpartyEdoAccountController counterpartyEdoAccountController)
		{
			_counterpartyConverter = counterpartyConverter ?? throw new ArgumentNullException(nameof(counterpartyConverter));
			_deliveryPointConverter = deliveryPointConverter ?? throw new ArgumentNullException(nameof(deliveryPointConverter));
			_counterpartyContractConverter =
				counterpartyContractConverter ?? throw new ArgumentNullException(nameof(counterpartyContractConverter));
			_orderItemConverter = orderItemConverter ?? throw new ArgumentNullException(nameof(orderItemConverter));
			_counterpartyEdoAccountController = counterpartyEdoAccountController ?? throw new ArgumentNullException(nameof(counterpartyEdoAccountController));
		}
		
		public OrderInfoForEdo ConvertOrderToOrderInfoForEdo(Order order)
		{
			var counterpartyInfo =
				_counterpartyConverter.ConvertCounterpartyToCounterpartyInfoForEdo(
					order.Client,
					_counterpartyEdoAccountController.GetDefaultCounterpartyEdoAccountByOrganizationId(order.Client, order.Contract.Organization.Id));
			var deliveryPointInfo = _deliveryPointConverter.ConvertDeliveryPointToDeliveryPointInfoForEdo(order.DeliveryPoint);
			var contractInfo = _counterpartyContractConverter.ConvertCounterpartyContractToCounterpartyContractInfoForEdo(
				order.Contract, order.DeliveryDate.Value);
			var orderItemsInfo = ConvertOrderItems(order.OrderItems);
			
			return new OrderInfoForEdo
			{
				Id = order.Id,
				CreationDate = order.CreateDate ?? default,
				DeliveryDate = order.DeliveryDate ?? default,
				OrderSum = order.OrderSum,
				CounterpartyExternalOrderId = order.CounterpartyExternalOrderId,
				CounterpartyInfoForEdo = counterpartyInfo,
				DeliveryPointInfoForEdo = deliveryPointInfo,
				ContractInfoForEdo = contractInfo,
				OrderItems = orderItemsInfo
			};
		}

		private IList<OrderItemInfoForEdo> ConvertOrderItems(IEnumerable<OrderItem> orderItems) =>
			orderItems.Select(orderItem => _orderItemConverter.ConvertOrderItemToOrderItemInfoForEdo(orderItem))
				.ToList();
	}
}
