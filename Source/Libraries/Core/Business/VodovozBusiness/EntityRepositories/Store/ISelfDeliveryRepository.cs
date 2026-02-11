using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Orders;

namespace Vodovoz.EntityRepositories.Store
{
	public interface ISelfDeliveryRepository
	{
		Dictionary<int, decimal> NomenclatureUnloaded(IUnitOfWork uow, Order order, SelfDeliveryDocument excludeDoc);
		Dictionary<int, decimal> OrderNomenclaturesLoaded(IUnitOfWork uow, Order order);
		/// <summary>
		/// Выводит список id номенклатур в заказе и количество отгруженное со склада 
		/// по документам отгрузки, включая документ отгрузки, который ещё не сохранён
		/// </summary>
		/// <returns>Id товара и сколько его отгружено</returns>
		/// <param name="order">Заказ по которому необходимо найти отгрузку товаров</param>
		/// <param name="notSavedDoc">Не сохранённый документ для включения в расчёт</param>
		Dictionary<int, decimal> OrderNomenclaturesUnloaded(IUnitOfWork uow, Order order, SelfDeliveryDocument notSavedDoc);
	}
}
