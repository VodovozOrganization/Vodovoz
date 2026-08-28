using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace VodovozBusiness.Controllers
{
	public interface IOrderSaleHandler : ISaleHandler
	{
		void SetCountWithRecalculateRents(IRecalculateRentCount saleItem, decimal count);
		void SetRentCount(IRecalculateRentCount saleItem, int count);
		void UpdateRentsCount();
		void SetPriceForNewSaleItem(IOrderSaleItem newItem, (SaleItemPriceType PriceType, decimal Price) priceData);
		/// <summary>
		/// Установка фактического количества позиции
		/// </summary>
		/// <param name="saleItem">Позиция на продажу</param>
		/// <param name="newValue">Фактическое количество</param>
		void SetActualCount(IOrderSaleItem saleItem, decimal? newValue);
		/// <summary>
		/// Установка фактического количества в 0, где оно не установлено(т.е. null), по всему заказу
		/// </summary>
		void SetActualCountZero();
		/// <summary>
		/// Установка фактического количества в 0, где оно null, по всему заказу
		/// </summary>
		void SetActualCountZero(IOrderSaleItem saleItem);
		/// <summary>
		/// Установка фактического количества с сохранением скидки или восстановлением из кэша(при закрытии МЛ)
		/// </summary>
		/// <param name="saleItem">Позиция на продажу</param>
		/// <param name="newValue">Устанавливаемое количество</param>
		void SetActualCountWithPreserveOrRestoreDiscount(IOrderSaleItem saleItem, decimal? newValue);
		/// <summary>
		/// Устанавливает ActualCount из Count
		/// </summary>
		/// <param name="ignoreHasValue">Устанавливать, если есть значение</param>
		void PreserveActualCount(bool ignoreHasValue = false);
		/// <summary>
		/// Восстановление данных заказа по фактическому количеству и скидкам
		/// </summary>
		/// <param name="newStatus"></param>
		void RestoreSaleItemsDiscountsAndCount(RouteListItemStatus newStatus);
		/// <summary>
		/// Устанавливает количество для каждого залога как actualCount,
		/// если заказ был создан только для залога.
		/// Для отображения этих данных в отчете "Акт по бутылям и залогам"
		/// </summary>
		void SetDepositsActualCounts();
		/// <summary>
		/// Восстановление скидок и установка фактического количества в null при восстановлении заказа(например, из отмены)
		/// </summary>
		void RestoreOriginalDiscountFromRestoreOrder();
		/// <summary>
		/// Копирование скидок из переданной позиции
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="saleItem">Позиция на продажу, куда копируются скидки</param>
		/// <param name="copyingSaleItem">Позиция на продажу из которой копируются скидки</param>
		void CopyDiscounts(IUnitOfWork uow, IApplyDiscountReasonItem saleItem, IApplyDiscountReasonItem copyingSaleItem);
		/// <summary>
		/// Копирование кэшированных скидок из переданной позиции
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="saleItem">Позиция на продажу, куда копируются скидк</param>
		/// <param name="copyingSaleItem">Позиция на продажу из которой копируются скидки</param>
		void CopyOriginalDiscounts(IUnitOfWork uow, IApplyDiscountReasonItem saleItem, IPreserveDiscount copyingSaleItem);
	}
}
