using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Organizations;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class OrganizationByPaymentTypeForOrderHandler : IGetOrganizationForOrder
	{
		private readonly OrganizationForDeliveryOrderByPaymentTypeHandler _organizationForDeliveryOrderByPaymentTypeHandler;
		private readonly OrganizationForSelfDeliveryOrderByPaymentTypeHandler _organizationForSelfDeliveryOrderByPaymentTypeHandler;

		public OrganizationByPaymentTypeForOrderHandler(
			OrganizationForDeliveryOrderByPaymentTypeHandler organizationForDeliveryOrderByPaymentTypeHandler,
			OrganizationForSelfDeliveryOrderByPaymentTypeHandler organizationForSelfDeliveryOrderByPaymentTypeHandler)
		{
			_organizationForDeliveryOrderByPaymentTypeHandler =
				organizationForDeliveryOrderByPaymentTypeHandler
				?? throw new ArgumentNullException(nameof(organizationForDeliveryOrderByPaymentTypeHandler));
			_organizationForSelfDeliveryOrderByPaymentTypeHandler =
				organizationForSelfDeliveryOrderByPaymentTypeHandler
				?? throw new ArgumentNullException(nameof(organizationForSelfDeliveryOrderByPaymentTypeHandler));
		}
		
		public IEnumerable<OrganizationForOrderWithOrderItems> GetOrganizationsWithOrderItems(
			Order order,
			IUnitOfWork uow)
		{
			return new List<OrganizationForOrderWithOrderItems>
			{
				new OrganizationForOrderWithOrderItems(GetOrganizationForOrder(order, uow))
			};
		}
		
		public Organization GetOrganizationForOrder(
			Order order,
			IUnitOfWork uow)
		{
			if(order.SelfDelivery || order.DeliveryPoint is null)
			{
				return _organizationForSelfDeliveryOrderByPaymentTypeHandler.GetOrganizationForOrder(order, uow);
			}

			return _organizationForDeliveryOrderByPaymentTypeHandler.GetOrganizationForOrder(order, uow);
		}
	}
}
