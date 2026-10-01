using Vodovoz.Domain.Goods.Rent;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Factories
{
	/// <summary>
	/// Фабрика по созданию позиций на продажу заказа
	/// </summary>
	public interface IOrderSaleItemFactory : ISaleItemFactory
	{
		/// <summary>
		/// Создание позиции на продажу
		/// </summary>
		/// <param name="source">Заказ</param>
		/// <param name="newSaleItem">Данные для создания позиции(строки заказа)</param>
		/// <returns>Строка заказа <see cref="OrderItem"/></returns>
		new OrderItem Create(object source, NewOrderSaleItem newSaleItem);
		/// <summary>
		/// Создание залоговой позиции из платного пакета аренды(долгосрочная аренда)
		/// </summary>
		/// <param name="source">Заказ</param>
		/// <param name="paidRentPackage">Платный пакет аренды</param>
		/// <returns>Строка заказа <see cref="OrderItem"/></returns>
		OrderItem CreateNewNonFreeRentDepositItem(object source, PaidRentPackage paidRentPackage);
		/// <summary>
		/// Создание долгосрочной услуги аренды из платного пакета аренды 
		/// </summary>
		/// <param name="source">Заказ</param>
		/// <param name="paidRentPackage">Платный пакет аренды</param>
		/// <returns>Строка заказа <see cref="OrderItem"/></returns>
		OrderItem CreateNewNonFreeRentServiceItem(object source, PaidRentPackage paidRentPackage);
		/// <summary>
		/// Создание залоговой позиции из платного пакета аренды(посуточная аренда)
		/// </summary>
		/// <param name="source">Заказ</param>
		/// <param name="paidRentPackage">Платный пакет аренды</param>
		/// <returns>Строка заказа <see cref="OrderItem"/></returns>
		OrderItem CreateNewDailyRentDepositItem(object source, PaidRentPackage paidRentPackage);
		/// <summary>
		/// Создание посуточной услуги аренды из платного пакета аренды 
		/// </summary>
		/// <param name="source">Заказ</param>
		/// <param name="paidRentPackage">Платный пакет аренды</param>
		/// <returns>Строка заказа <see cref="OrderItem"/></returns>
		OrderItem CreateNewDailyRentServiceItem(object source, PaidRentPackage paidRentPackage);
		/// <summary>
		/// Создание залоговой позиции из бесплатного пакета аренды 
		/// </summary>
		/// <param name="source">Заказ</param>
		/// <param name="freeRentPackage">Бесплатный пакет аренды</param>
		/// <returns>Строка заказа <see cref="OrderItem"/></returns>
		OrderItem CreateNewFreeRentDepositItem(object source, FreeRentPackage freeRentPackage);
	}
}
