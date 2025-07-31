using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using Vodovoz.Errors;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Nodes;

namespace Vodovoz.Handlers
{
	/// <summary>
	/// Интерфейс для работы с фиксой в ИПЗ
	/// </summary>
	public interface IOnlineOrderFixedPriceHandler
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
		/// <see cref="CanApplyOnlineOrderFixedPrice"/></param>
		/// <returns></returns>
		Result<IEnumerable<IOnlineOrderedProductWithFixedPrice>> TryApplyFixedPrice(
			IUnitOfWork uow, CanApplyOnlineOrderFixedPrice canApplyOnlineOrderFixedPrice);
	}
}
