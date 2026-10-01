using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace VodovozBusiness.Factories
{
	/// <summary>
	/// Фабрика по созданию позиций на продажу шаблона
	/// </summary>
	public interface IOnlineOrderTemplateSaleItemFactory : ISaleItemFactory
	{
		/// <summary>
		/// Создание позиции на продажу шаблона
		/// </summary>
		/// <param name="source">Источник(шаблон)</param>
		/// <param name="newSaleItem">Данные новой позиции</param>
		/// <returns>Позиция на продажу <see cref="OnlineOrderTemplateSaleItem"/></returns>
		new OnlineOrderTemplateSaleItem Create(object source, NewOrderSaleItem newSaleItem);
	}
}
