using System.Collections.Generic;
using CustomerOrders.Abstractions.V8.Sale;
using CustomerOrders.Contracts.V8.Orders.Items;

namespace CustomerOrders.Contracts.V8.Sale.Templates
{
	/// <summary>
	/// Данные продаваемой позиции из шаблона
	/// </summary>
	public class TemplateSaleItemWithDiscountDetailsDto : OnlineOrderItemBaseDto
	{
		/// <summary>
		/// Скидки
		/// </summary>
		public IList<IDiscountAmount> Discounts { get; set; }
	}
}
