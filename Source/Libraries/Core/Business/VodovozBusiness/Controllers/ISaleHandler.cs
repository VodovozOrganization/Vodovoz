using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Sale;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace VodovozBusiness.Controllers
{
	public interface ISaleHandler
	{
		/// <summary>
		/// Пересчет данных продаваемых позиций
		/// </summary>
		void Recalculate();
		/// <summary>
		/// Пересчет цены позиции
		/// </summary>
		/// <param name="saleItem">Позиция на продажу</param>
		void RecalculatePrice(ISaleItem saleItem);
		/// <summary>
		/// Пересчет скидок
		/// </summary>
		void RecalculateDiscounts(IDataContext context);
		/// <summary>
		/// Установка источника продажи (заказ, счет без доставки на предоплату)
		/// </summary>
		/// <param name="source"></param>
		void SetSource(ISaleSource source);
		/// <summary>
		/// Установка нового количества на позицию с последующим вызовом необходимых действий(пересчет цен, налогов, скидок)
		/// </summary>
		/// <param name="saleItem">Продаваемая позиция</param>
		/// <param name="count">Устанавливаемое количество</param>
		/// <returns></returns>
		bool SetCount(INomenclatureCount saleItem, decimal count);
		/// <summary>
		/// Установка цены на позицию, с последующим вызовом необходимых действий(пересчет налогов, скидок и т.д.)
		/// </summary>
		/// <param name="saleItem">Продаваемая позиция</param>
		/// <param name="priceData">Данные цены(тип и стоимость)</param>
		void SetPrice(ISaleItem saleItem, (SaleItemPriceType PriceType, decimal Price) priceData);
	}
}
