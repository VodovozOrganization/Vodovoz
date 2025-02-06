using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.Settings.Orders;
using Vodovoz.Settings.Organizations;

namespace VodovozBusiness.Domain.Orders
{
	public class OrganizationForDeliveryOrderByPaymentTypeHandler : GetOrganizationForOrder, IGetOrganizationForOrder
	{
		public OrganizationForDeliveryOrderByPaymentTypeHandler(
			IOrganizationSettings organizationSettings,
			IOrderSettings orderSettings,
			IFastPaymentRepository fastPaymentRepository)
		: base(organizationSettings, orderSettings, fastPaymentRepository)
		{
		}

		public IReadOnlyDictionary<Organization, IEnumerable<OrderItem>> GetOrganizationsForOrder(
			Order order,
			IUnitOfWork uow = null,
			PaymentType? paymentType = null)
		{
			return new Dictionary<Organization, IEnumerable<OrderItem>>
			{
				{ GetOrganizationForOrder(order, uow, paymentType), null }
			};
		}
		
		public Organization GetOrganizationForOrder(
			Order order,
			IUnitOfWork uow = null,
			PaymentType? paymentType = null)
		{
			int organizationId;
			var orderCreateDate = order.CreateDate;
			var onlineOrderId = order.OnlineOrder;
			
			switch(paymentType)
			{
				case PaymentType.Barter:
				case PaymentType.Cashless:
				case PaymentType.ContractDocumentation:
					organizationId = OrganizationSettings.VodovozOrganizationId;
					break;
				case PaymentType.Cash:
					organizationId = OrganizationSettings.VodovozSouthOrganizationId;
					break;
				case PaymentType.Terminal:
					organizationId = OrganizationSettings.BeveragesWorldOrganizationId;
					break;
				case PaymentType.DriverApplicationQR:
				case PaymentType.SmsQR:
					organizationId = GetOrganizationIdForByCard(
						uow,
						uow.GetById<PaymentFrom>(OrderSettings.GetPaymentByCardFromFastPaymentServiceId),
						orderCreateDate,
						onlineOrderId);
					break;
				case PaymentType.PaidOnline:
					organizationId = GetOrganizationIdForByCard(uow, order.PaymentByCardFrom, orderCreateDate, onlineOrderId);
					break;
				default:
					throw new NotSupportedException($"Тип оплаты {paymentType} не поддерживается, невозможно подобрать организацию.");
			}

			return uow.GetById<Organization>(organizationId);
		}
	}
}
