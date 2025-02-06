using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.FastPayments;
using Vodovoz.Settings.Orders;
using Vodovoz.Settings.Organizations;
using VodovozBusiness.Domain.Settings;

namespace VodovozBusiness.Domain.Orders
{
	public class OrganizationForSelfDeliveryOrderByPaymentTypeHandler : GetOrganizationForOrder
	{
		public OrganizationForSelfDeliveryOrderByPaymentTypeHandler(
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
					throw new NotSupportedException(
						$"Невозможно подобрать организацию, так как тип оплаты {paymentType} не поддерживается.");
			}

			return uow.GetById<Organization>(organizationId);
		}
		
		public Organization GetOrganizationForOrder2(
			Order order,
			IUnitOfWork uow = null,
			PaymentType? paymentType = null)
		{
			int organizationId;
			var orderCreateDate = order.CreateDate;
			var onlineOrderId = order.OnlineOrder;

			var organizationsByPaymentType = uow.GetAll<PaymentTypeOrganizationSettings>();
			var paymentTypeOrganization = organizationsByPaymentType.FirstOrDefault(o => o.PaymentType == paymentType);

			if(paymentTypeOrganization is null)
			{
				throw new NotSupportedException(
					$"Невозможно подобрать организацию, так как тип оплаты {paymentType} не поддерживается.");
			}

			if(paymentTypeOrganization.PaymentType == PaymentType.SmsQR
				|| paymentTypeOrganization.PaymentType == PaymentType.DriverApplicationQR)
			{
				organizationId = GetOrganizationIdForByCard(
					uow,
					uow.GetById<PaymentFrom>(OrderSettings.GetPaymentByCardFromFastPaymentServiceId),
					orderCreateDate,
					onlineOrderId);
				
				return uow.GetById<Organization>(organizationId);
			}

			if(paymentTypeOrganization.PaymentType == PaymentType.PaidOnline)
			{
				organizationId = GetOrganizationIdForByCard(uow, order.PaymentByCardFrom, orderCreateDate, onlineOrderId);
				return uow.GetById<Organization>(organizationId);
			}

			return paymentTypeOrganization.OrganizationForOrder;
		}
	}
}
