using System.Collections.Generic;
using QS.DomainModel.UoW;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Factories
{
	/// <summary>
	/// Класс создания данных для создания позиций на продажу
	/// </summary>
	public interface INewOrderSaleItemsFromPromoSetCreator
	{
		/// <summary>
		/// Создание переходного класса для создания позиции на продажу в заказе
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="onlineOrderPromoSet">Промонабор из онлайн заказа</param>
		/// <param name="hasPermissionsForAlternativePrice">Есть права на альтернативную цену</param>
		/// <returns></returns>
		IEnumerable<NewOrderSaleItem> Create(
			IUnitOfWork uow,
			OnlineOrderPromoSet onlineOrderPromoSet,
			bool hasPermissionsForAlternativePrice);
	}
}
