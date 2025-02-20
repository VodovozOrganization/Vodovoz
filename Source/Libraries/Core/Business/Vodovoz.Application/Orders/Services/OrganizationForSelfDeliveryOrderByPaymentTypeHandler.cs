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

namespace Vodovoz.Application.Orders.Services
{
	public class OrganizationForSelfDeliveryOrderByPaymentTypeHandler : GetOrganizationForOrder
	{
		public OrganizationForSelfDeliveryOrderByPaymentTypeHandler(
			IOrganizationSettings organizationSettings,
			IOrderSettings orderSettings,
			IFastPaymentRepository fastPaymentRepository,
			OrganizationForOrderFromSet organizationForOrderFromSet)
			: base(organizationSettings, orderSettings, fastPaymentRepository, organizationForOrderFromSet)
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
			var onlineOrderId = order.OnlineOrder;

			var organizationsByPaymentType = uow.GetAll<PaymentTypeOrganizationSettings>();
			var paymentTypeOrganization = organizationsByPaymentType.FirstOrDefault(o => o.PaymentType == paymentType);

			if(paymentTypeOrganization is null)
			{
				throw new NotSupportedException(
					$"Невозможно подобрать организацию, так как тип оплаты {paymentType} не поддерживается.");
			}

			if(paymentTypeOrganization.PaymentType == PaymentType.SmsQR
			   || paymentTypeOrganization.PaymentType == PaymentType.DriverApplicationQR
			   || paymentTypeOrganization.PaymentType == PaymentType.PaidOnline)
			{
				var organizationId = GetOrganizationId(
					uow,
					paymentTypeOrganization,
					order.Id,
					onlineOrderId);
				
				return uow.GetById<Organization>(organizationId);
			}

			return OrganizationForOrderFromSet.GetOrganizationForOrderFromSet(order.Id, paymentTypeOrganization);
		}
	}
}
