using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Goods.NomenclaturesOnlineParameters;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Nodes;

namespace Vodovoz.EntityRepositories.Orders
{
	public interface IPromotionalSetRepository
	{
		/// <summary>
		/// Возврат словаря, у которого ключ это <see cref="PromotionalSet.Id"/>,
		/// а значение - массив с <see cref="Order.Id"/>, для всех точек доставок
		/// похожих по полям <see cref="DeliveryPoint.City"/>,
		/// <see cref="DeliveryPoint.Street"/>, <see cref="DeliveryPoint.Building"/>,
		/// <see cref="DeliveryPoint.Room"/>
		/// </summary>
		/// <returns>Словарь</returns>
		/// <param name="uow">Unit Of Work</param>
		/// <param name="currOrder">Заказ, из которого берётся точка доставки</param>
		/// <param name="ignoreCurrentOrder">Если <c>true</c>, то в выборке будет
		/// игнорироваться заказ передаваемы в качестве параметра <paramref name="currOrder"/></param>
		Dictionary<int, int[]> GetPromotionalSetsAndCorrespondingOrdersForDeliveryPoint(
			IUnitOfWork uow, Order currOrder, bool ignoreCurrentOrder = false);
		bool AddressHasAlreadyBeenUsedForPromoForNewClients(IUnitOfWork uow, DeliveryPoint deliveryPoint);
		IEnumerable<PromotionalSetOnlineParametersNode> GetActivePromotionalSetsOnlineParametersForSend(
			IUnitOfWork uow, GoodsOnlineParameterType parameterType);
		IEnumerable<PromotionalSetItemBalanceNode> GetPromotionalSetsItemsWithBalanceForSend(
			IUnitOfWork uow,
			GoodsOnlineParameterType parameterType,
			IEnumerable<int> warehouses);
	}
}
