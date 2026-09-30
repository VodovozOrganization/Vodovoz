using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Interfaces.Sale;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Handlers
{
	public interface IFixedPriceHandler
	{
		/// <summary>
		/// Есть фикса или нет
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="counterpartyId">Идентификатор клиента</param>
		/// <param name="deliveryPointId">Идентификатор ТД</param>
		/// <param name="isSelfDelivery">Саомвывоз</param>
		/// <param name="fixedPrices">Список найденных фиксированных цен, если они есть, иначе пустой список</param>
		/// <returns></returns>
		bool HasFixedPrices(
			IUnitOfWork uow,
			int? counterpartyId,
			int? deliveryPointId,
			bool isSelfDelivery,
			out IEnumerable<NomenclatureFixedPrice> fixedPrices);
		
		/// <summary>
		/// Применима ли фикса
		/// </summary>
		/// <param name="saleItem">Позиция из корзины ИПЗ</param>
		/// <param name="discountReasons">Список оснований скидок позиции</param>
		/// <param name="fixedPrice">Фикса для применения</param>
		/// <returns></returns>
		Result IsApplicable(
			IOrderedCartItemWithDiscountDetails saleItem,
			IEnumerable<DiscountReasonBase> discountReasons,
			NomenclatureFixedPrice fixedPrice);
	}
}
