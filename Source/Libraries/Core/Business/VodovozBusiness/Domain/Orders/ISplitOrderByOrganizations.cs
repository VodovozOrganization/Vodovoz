using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using VodovozBusiness.Models.Orders;
using VodovozBusiness.Services.Orders;

namespace VodovozBusiness.Domain.Orders
{
	/// <summary>
	/// Контракт разбивания заказа по организациям
	/// </summary>
	public interface ISplitOrderByOrganizations
	{
		/// <summary>
		/// Получение списка организаций с товарами из разбитого заказа
		/// </summary>
		/// <param name="requestTime">Время запроса</param>
		/// <param name="organizationChoice">Обрабатываемые данные</param>
		/// <param name="uow">unit Of Work</param>
		/// <returns>Список организаций с товарами</returns>
		IEnumerable<PartOrderWithGoods> SplitOrderByOrganizations(
			IUnitOfWork uow,
			TimeSpan requestTime,
			OrderOrganizationChoice organizationChoice
			);
	}
}
