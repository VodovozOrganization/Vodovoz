using System.Collections.Generic;
using CustomerOrders.Contracts.V5.Orders.FixedPrices;
using CustomerOrders.Contracts.V5.Orders.OrderItem;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using VodovozBusiness.Nodes;

namespace Vodovoz.Handlers
{
	/// <summary>
	/// Интерфейс для работы с фиксой в ИПЗ
	/// </summary>
	public interface IOnlineOrderFixedPriceHandlerV5
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="counterpartyId">Id контрагента</param>
		/// <param name="deliveryPointId">Id точки доставки</param>
		/// <param name="isSelfDelivery">Самовывоз</param>
		/// <param name="fixedPrices">Список фиксированных цен</param>
		/// <returns><c>true</c> - есть фикса, <c>false</c> - нет фиксы</returns>
		bool HasFixedPrices(
			IUnitOfWork uow,
			int? counterpartyId,
			int? deliveryPointId,
			bool isSelfDelivery,
			out IEnumerable<NomenclatureFixedPrice> fixedPrices);
		/// <summary>
		/// Применение фиксы к онлайн заказу
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="canApplyOnlineOrderFixedPrice">Данные, необходимые для проверки фиксы и товары
		/// <see cref="CanApplyOnlineOrderFixedPriceV5"/></param>
		/// <returns></returns>
		Result<IEnumerable<OnlineOrderItemWithFixedPriceV5>> TryApplyFixedPrice(
			IUnitOfWork uow, CanApplyOnlineOrderFixedPriceV5 canApplyOnlineOrderFixedPrice);
	}
}
