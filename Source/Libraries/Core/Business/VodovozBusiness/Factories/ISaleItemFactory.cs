using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Factories
{
	/// <summary>
	/// Общая фабрика по созданию позиций на продажу
	/// </summary>
	public interface ISaleItemFactory
	{
		/// <summary>
		/// Создание позиции на продажу
		/// </summary>
		/// <param name="source">Источник продажи(заказ, шаблон)</param>
		/// <param name="newSaleItem">Данные</param>
		/// <returns>Позиция на продажу <see cref="ISaleItem"/></returns>
		ISaleItem Create(object source, NewOrderSaleItem newSaleItem);
	}
}
