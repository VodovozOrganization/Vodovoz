using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Models.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Orders.Services
{
	/// <summary>
	/// Обработчик для подбора организации при установленной нашей организации в заказе
	/// </summary>
	public class OrderOurOrganizationForOrderHandler : OrganizationForOrderHandler
	{
		public override IEnumerable<PartOrderWithGoods> GetOrganizationsWithOrderItems(
			IUnitOfWork uow,
			TimeSpan requestTime,
			OrderOrganizationChoice organizationChoice)
		{
			if(organizationChoice.OurOrganization != null)
			{
				return new List<PartOrderWithGoods>
				{
					new PartOrderWithGoods(organizationChoice.OurOrganization)
				};
			}
			
			return base.GetOrganizationsWithOrderItems(uow, requestTime, organizationChoice);
		}
	}
}
