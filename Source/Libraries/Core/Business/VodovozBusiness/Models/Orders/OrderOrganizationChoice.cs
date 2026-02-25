using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.Extensions;
using Vodovoz.Settings.Orders;

namespace VodovozBusiness.Models.Orders
{
	public class OrderOrganizationChoice
	{
		public int OrderId { get; private set; }
		public DateTime? DeliveryDate { get; private set; }
		public Subdivision AuthorSubdivision { get; private set; }
		public CounterpartyContract Contract { get; private set; }
		public DeliveryPoint DeliveryPoint { get; private set; }
		public Organization OurOrganization { get; private set; }
		public Organization ClientWorksThroughOrganization { get; private set; }
		public Organization CurrentOrderOrganization { get; private set; }
		public IEnumerable<IProduct> Goods { get; private set; }
		public IEnumerable<OrderEquipment> OrderEquipments { get; private set; }
		public IEnumerable<OrderDepositItem> OrderDepositItems { get; private set; }
		public bool IsSelfDelivery { get; private set; }
		public PaymentType PaymentType { get; private set; }
		public PaymentFrom PaymentFrom { get; private set; }
		public int? OnlinePaymentNumber { get; private set; }
		public bool IsSplitedOrder { get; private set; }

		public static OrderOrganizationChoice Create(IUnitOfWork uow, IOrderSettings orderSettings, OnlineOrder onlineOrder)
		{
			var paymentFrom = onlineOrder.OnlinePaymentSource.HasValue
				? uow.GetById<PaymentFrom>(
					onlineOrder.OnlinePaymentSource.ConvertToPaymentFromId(orderSettings))
				: null;
			
			return new OrderOrganizationChoice
			{
				OrderId = 0,
				DeliveryDate = onlineOrder.DeliveryDate,
				AuthorSubdivision = onlineOrder.EmployeeWorkWith?.Subdivision,
				Contract = null,
				DeliveryPoint = onlineOrder.DeliveryPoint,
				OurOrganization = null,
				ClientWorksThroughOrganization = onlineOrder.Counterparty?.WorksThroughOrganization,
				CurrentOrderOrganization = null,
				Goods = onlineOrder.OnlineOrderItems,
				OrderEquipments = new List<OrderEquipment>(),
				OrderDepositItems = new List<OrderDepositItem>(),
				IsSelfDelivery = onlineOrder.IsSelfDelivery,
				PaymentType = onlineOrder.OnlineOrderPaymentType.ToOrderPaymentType(),
				PaymentFrom = paymentFrom,
				OnlinePaymentNumber = onlineOrder.OnlinePayment,
				IsSplitedOrder = false
			};
		}

		public static OrderOrganizationChoice Create(Order order)
		{
			return new OrderOrganizationChoice
			{
				OrderId = order.Id,
				DeliveryDate = order.DeliveryDate,
				AuthorSubdivision = order.Author?.Subdivision,
				Contract = order.Contract,
				DeliveryPoint = order.DeliveryPoint,
				OurOrganization = order.OurOrganization,
				ClientWorksThroughOrganization = order.Client?.WorksThroughOrganization,
				CurrentOrderOrganization = order.Contract?.Organization,
				Goods = order.OrderItems,
				OrderEquipments = order.OrderEquipments,
				OrderDepositItems = order.OrderDepositItems,
				IsSelfDelivery = order.SelfDelivery,
				PaymentType = order.PaymentType,
				PaymentFrom = order.PaymentByCardFrom,
				OnlinePaymentNumber = order.OnlinePaymentNumber,
				IsSplitedOrder = !string.IsNullOrWhiteSpace(order.OrderPartsIds)
			};
		}
	}
}
