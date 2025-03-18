using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public abstract class OrganizationForOrderHandler
	{
		protected OrganizationForOrderHandler NextHandler { get; private set; }

		public void SetNextHandler(OrganizationForOrderHandler handler)
		{
			NextHandler = handler;
		}

		public virtual IEnumerable<OrganizationForOrderWithGoodsAndEquipmentsAndDeposits> GetOrganizationsWithOrderItems(
			TimeSpan requestTime,
			Order order,
			IUnitOfWork uow = null)
		{
			return NextHandler?.GetOrganizationsWithOrderItems(requestTime, order, uow);
		}
	}
}
