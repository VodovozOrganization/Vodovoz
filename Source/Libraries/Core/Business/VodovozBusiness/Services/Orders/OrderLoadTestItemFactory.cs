using System;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Services.Orders
{
	/// <summary>
	/// Фабрика позиций заказа для нагрузочного теста (без бизнес-логики подтверждения/договора).
	/// Вынесена в VodovozBusiness, чтобы использовать internal CreateForSale.
	/// </summary>
	public static class OrderLoadTestItemFactory
	{
		public static OrderItem CreateSaleItem(Order order, Nomenclature nomenclature, decimal count, decimal price)
		{
			if(order is null)
			{
				throw new ArgumentNullException(nameof(order));
			}

			if(nomenclature is null)
			{
				throw new ArgumentNullException(nameof(nomenclature));
			}

			return OrderItem.CreateForSale(order, nomenclature, count, price);
		}
	}
}
