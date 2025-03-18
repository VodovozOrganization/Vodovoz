using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Application.Orders.Services
{
	/// <summary>
	/// Обработчик для подбора организации через которую работает клиент
	/// </summary>
	public class OrganizationFromClientForOrderHandler : OrganizationForOrderHandler
	{
		public override IEnumerable<OrganizationForOrderWithGoodsAndEquipmentsAndDeposits> GetOrganizationsWithOrderItems(
			TimeSpan requestTime,
			Order order,
			IUnitOfWork uow = null)
		{
			if(order.Client?.WorksThroughOrganization != null)
			{
				return new List<OrganizationForOrderWithGoodsAndEquipmentsAndDeposits>
				{
					new OrganizationForOrderWithGoodsAndEquipmentsAndDeposits(order.Client.WorksThroughOrganization)
				};
			}
			
			return base.GetOrganizationsWithOrderItems(requestTime, order, uow);
		}
	}
}
