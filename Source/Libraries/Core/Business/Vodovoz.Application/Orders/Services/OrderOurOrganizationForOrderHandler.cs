using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Orders.Services
{
	/// <summary>
	/// Обработчик для подбора организации при установленной нашей организации в заказе
	/// </summary>
	public class OrderOurOrganizationForOrderHandler : OrganizationForOrderHandler
	{
		public override IEnumerable<OrganizationForOrderWithGoodsAndEquipmentsAndDeposits> GetOrganizationsWithOrderItems(
			TimeSpan requestTime,
			Order order,
			IUnitOfWork uow = null)
		{
			if(order.OurOrganization != null)
			{
				return new List<OrganizationForOrderWithGoodsAndEquipmentsAndDeposits>
				{
					new OrganizationForOrderWithGoodsAndEquipmentsAndDeposits(order.OurOrganization)
				};
			}
			
			return base.GetOrganizationsWithOrderItems(requestTime, order, uow);
		}
	}
}
