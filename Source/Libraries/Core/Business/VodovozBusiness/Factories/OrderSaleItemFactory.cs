using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Factories
{
	public class OrderSaleItemFactory : ISaleItemFactory
	{
		public IProduct Create(
			object source,
			decimal count,
			decimal price, //nomenclature.GetPrice(1, canApplyAlternativePrice)
			Nomenclature nomenclature
		)
		{
			return OrderItem.CreateForSale(source as Order, nomenclature, count, price);
		}
	}
}
