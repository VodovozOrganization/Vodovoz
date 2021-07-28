using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.OrdersWithoutShipment;
using Vodovoz.Domain.Organizations;
using Vodovoz.Domain.Sale;
using Vodovoz.Services;

namespace Vodovoz.Models
{
	public class Stage1OrganizationProvider : IOrganizationProvider
	{
		private readonly IOrganizationParametersProvider organizationParametersProvider;
		private readonly IOrderParametersProvider orderParametersProvider;

		public Stage1OrganizationProvider(IOrganizationParametersProvider organizationParametersProvider,
			IOrderParametersProvider orderParametersProvider)
		{
			this.organizationParametersProvider = organizationParametersProvider ??
				throw new ArgumentNullException(nameof(organizationParametersProvider));
			this.orderParametersProvider = orderParametersProvider ?? throw new ArgumentNullException(nameof(orderParametersProvider));
		}

		public Organization GetOrganization(IUnitOfWork uow, Order order)
		{
			if(uow == null) throw new ArgumentNullException(nameof(uow));
			if(order == null) throw new ArgumentNullException(nameof(order));

			if(IsOnlineStoreOrder(order))
			{
				return GetOrganizationForOnlineStore(uow);
			}

			if(order.SelfDelivery)
			{
				return GetOrganizationForSelfDelivery(uow, order);
			}

			return GetOrganizationForOtherOptions(uow, order);
		}

		public Organization GetOrganization(IUnitOfWork uow, PaymentType paymentType, bool isSelfDelivery,
			IEnumerable<OrderItem> orderItems = null,
			PaymentFrom paymentFrom = null, GeographicGroup geographicGroup = null)
		{
			throw new NotSupportedException();
		}

		private Organization GetOrganizationForSelfDelivery(IUnitOfWork uow, Order order)
		{
			int organizationId = 0;
			switch(order.PaymentType)
			{
				case PaymentType.barter:
				case PaymentType.cashless:
				case PaymentType.ContractDoc:
					organizationId = organizationParametersProvider.VodovozOrganizationId;
					break;
				case PaymentType.cash:
				case PaymentType.Terminal:
					organizationId = organizationParametersProvider.VodovozSouthOrganizationId;
					break;
				case PaymentType.BeveragesWorld:
					organizationId = organizationParametersProvider.BeveragesWorldOrganizationId;
					break;
				case PaymentType.ByCard:
					var idsForSosnovcev = new int[]
						{ orderParametersProvider.PaymentByCardFromMobileAppId, orderParametersProvider.PaymentByCardFromSiteId };
					if(order.PaymentByCardFrom != null && idsForSosnovcev.Contains(order.PaymentByCardFrom.Id))
					{
						organizationId = organizationParametersProvider.SosnovcevOrganizationId;
					}
					else
					{
						organizationId = organizationParametersProvider.VodovozSouthOrganizationId;
					}
					break;
				default:
					throw new NotSupportedException(
						$"Невозможно подобрать организацию, так как тип оплаты {order.PaymentType} не поддерживается.");
			}

			return uow.GetById<Organization>(organizationId);
		}

		private bool IsOnlineStoreOrder(Order order)
		{
			return order.OrderItems.Any(x =>
				x.Nomenclature.OnlineStore != null && x.Nomenclature.OnlineStore.Id != orderParametersProvider.OldInternalOnlineStoreId);
		}

		private Organization GetOrganizationForOnlineStore(IUnitOfWork uow)
		{
			return uow.GetById<Organization>(organizationParametersProvider.VodovozSouthOrganizationId);
		}

		private Organization GetOrganizationForOtherOptions(IUnitOfWork uow, Order order)
		{
			int organizationId = 0;
			switch(order.PaymentType)
			{
				case PaymentType.barter:
				case PaymentType.cashless:
				case PaymentType.ContractDoc:
					organizationId = organizationParametersProvider.VodovozOrganizationId;
					break;
				case PaymentType.cash:
					organizationId = organizationParametersProvider.SosnovcevOrganizationId;
					break;
				case PaymentType.Terminal:
					organizationId = organizationParametersProvider.VodovozSouthOrganizationId;
					break;
				case PaymentType.BeveragesWorld:
					organizationId = organizationParametersProvider.BeveragesWorldOrganizationId;
					break;
				case PaymentType.ByCard:
					var idsForSosnovcev = new int[]
						{ orderParametersProvider.PaymentByCardFromMobileAppId, orderParametersProvider.PaymentByCardFromSiteId };
					if(order.PaymentByCardFrom != null && idsForSosnovcev.Contains(order.PaymentByCardFrom.Id))
					{
						organizationId = organizationParametersProvider.SosnovcevOrganizationId;
					}
					else
					{
						organizationId = organizationParametersProvider.VodovozSouthOrganizationId;
					}
					break;
				default:
					throw new NotSupportedException($"Тип оплаты {order.PaymentType} не поддерживается, невозможно подобрать организацию.");
			}

			return uow.GetById<Organization>(organizationId);
		}

		public Organization GetOrganizationForOrderWithoutShipment(IUnitOfWork uow, OrderWithoutShipmentForAdvancePayment order)
		{
			if(uow == null) throw new ArgumentNullException(nameof(uow));
			if(order == null) throw new ArgumentNullException(nameof(order));

			int organizationId = organizationParametersProvider.VodovozOrganizationId;
			if(IsOnlineStoreOrderWithoutShipment(order))
			{
				organizationId = organizationParametersProvider.VodovozSouthOrganizationId;
			}

			return uow.GetById<Organization>(organizationId);
		}

		private bool IsOnlineStoreOrderWithoutShipment(OrderWithoutShipmentForAdvancePayment order)
		{
			return order.OrderWithoutDeliveryForAdvancePaymentItems.Any(x =>
				x.Nomenclature.OnlineStore != null && x.Nomenclature.OnlineStore.Id != orderParametersProvider.OldInternalOnlineStoreId);
		}
	}
}
