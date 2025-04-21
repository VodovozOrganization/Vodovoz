using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using VodovozBusiness.Models.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public abstract class OrganizationForOrderHandler
	{
		protected OrganizationForOrderHandler NextHandler { get; private set; }

		public void SetNextHandler(OrganizationForOrderHandler handler)
		{
			NextHandler = handler;
		}

		/// <summary>
		/// Разбиение заказа на части на основе его содержания
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="requestTime">Время запроса</param>
		/// <param name="organizationChoice">Данные для подбора организации</param>
		/// <returns></returns>
		public virtual IEnumerable<PartOrderWithGoods> SplitOrderByOrganizations(
			IUnitOfWork uow,
			TimeSpan requestTime,
			OrderOrganizationChoice organizationChoice)
		{
			return NextHandler?.SplitOrderByOrganizations(uow, requestTime, organizationChoice);
		}
	}
}
