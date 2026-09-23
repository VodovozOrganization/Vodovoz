using QS.DomainModel.UoW;
using Vodovoz.Domain.Goods.Rent;
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

		#region MyRegion

		/// <summary>
		/// Перенос номенклатур категорий вода и товары в заказ из выбранного заказа
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="fromOrder">Выбранный заказ</param>
		void FillOrderItems(IUnitOfWork uow, Order fromOrder);

		/// <summary>
		/// Обнвление выезда мастера (добавление или удаление)
		/// </summary>
		/// <param name="unitOfWork">unit of work</param>
		void UpdateMasterCallNomenclatureIfNeeded(IUnitOfWork unitOfWork);

		/// <summary>
		/// Добавление быстрой доставки по необходимости
		/// </summary>
		/// <param name="uow">unit of work</param>
		void TryAddFastDelivery(IUnitOfWork uow);

		#region Аренда

		/// <summary>
		/// Добавление залога за бесплатную аренду
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="freeRentPackage">Пакет бесплатной аренды</param>
		void AddFreeRentDepositItem(IUnitOfWork uow, FreeRentPackage freeRentPackage);
		
		/// <summary>
		/// Добавление залога за посуточную аренду
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="paidRentPackage">Пакет платной аренды</param>
		void AddDailyRentDepositItem(IUnitOfWork uow, PaidRentPackage paidRentPackage);
		
		/// <summary>
		/// Добавление услуги посуточной аренды
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="paidRentPackage">Пакет платной аренды</param>
		void AddDailyRentServiceItem(IUnitOfWork uow, PaidRentPackage paidRentPackage);
		
		/// <summary>
		/// Добавление залога за долгосрочную аренду
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="paidRentPackage">Пакет платной аренды</param>
		void AddNonFreeRentDepositItem(IUnitOfWork uow, PaidRentPackage paidRentPackage);
		
		/// <summary>
		/// Добавление услуги долгосрочной аренды
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="paidRentPackage">Пакет платной аренды</param>
		void AddNonFreeRentServiceItem(IUnitOfWork uow, PaidRentPackage paidRentPackage);

		#endregion

		#region Удаление

		/// <summary>
		/// Удаление оборудования из заказа
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="equipment">Удаляемое оборудование</param>
		void RemoveEquipment(IUnitOfWork uow, OrderEquipment equipment);

		/// <summary>
		/// Удаление быстрой доставки
		/// </summary>
		/// <param name="uow">unit of work</param>
		void RemoveFastDelivery(IUnitOfWork uow);

		#endregion

		#endregion
	}
}
