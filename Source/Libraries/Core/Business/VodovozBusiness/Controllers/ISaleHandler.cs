using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Common;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
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

		#region MyRegion

		/// <summary>
		/// Добавление новой позиции на продажу
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="nomenclature">Номенклатура</param>
		/// <param name="count">Количество</param>
		/// <param name="discount">Скидка</param>
		/// <param name="discountReasons">Основания скидок</param>
		/// <returns></returns>
		Result TryAddNomenclature(
			IUnitOfWork uow,
			Nomenclature nomenclature,
			decimal count = 0,
			decimal discount = 0,
			IEnumerable<DiscountReasonBase> discountReasons = null
		);

		/// <summary>
		/// Добавление номенклатуры на продажу
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="newOrderSaleItem">Данные по новой позиции</param>
		void AddNomenclature(IUnitOfWork uow, NewOrderSaleItem newOrderSaleItem);

		/// <summary>
		/// Добавление воды на продажу
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="newOrderSaleItem">Данные по новой позиции</param>
		void AddWaterForSale(IUnitOfWork uow, NewOrderSaleItem newOrderSaleItem);

		/// <summary>
		/// Добавление продаваемой позиции
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="saleItem">Позиция на продажу</param>
		/// <param name="priceData">Данные по цене</param>
		void AddSaleItem(
			IUnitOfWork uow,
			ISaleItem saleItem,
			(SaleItemPriceType PriceType, decimal Price) priceData
		);

		/// <summary>
		/// Добавление промонабора
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="proSet">Промонабор</param>
		/// <returns></returns>
		Result TryAddPromoSet(IUnitOfWork uow, PromotionalSet proSet);

		/// <summary>
		/// Установка цены на выезд мастера при необходимости
		/// </summary>
		/// <param name="unitOfWork">unit of work</param>
		void TrySetMasterCallNomenclaturePrice(IUnitOfWork unitOfWork);
		
		/// <summary>
		/// Обновление платной доставки
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <returns></returns>
		Result UpdateDeliveryCost(IUnitOfWork uow);

		#region Удаление

		/// <summary>
		/// Удаление позиции на продажу с дополнительными проверками
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="saleItem">Позиция на продажу</param>
		/// <returns></returns>
		Result TryRemoveSaleItem(IUnitOfWork uow, ISaleItem saleItem);

		#endregion

		#endregion
	}
}
