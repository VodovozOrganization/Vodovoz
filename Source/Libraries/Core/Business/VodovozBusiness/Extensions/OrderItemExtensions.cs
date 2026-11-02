using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Controllers;

namespace Vodovoz.Extensions
{
	public static class OrderItemExtensions
	{
		internal static void UpdatePriceWithRecalculate(
			this OrderItem newItem,
			(SaleItemPriceType PriceType, decimal Price) priceData,
			IOrderSaleHandler saleHandler)
		{
			saleHandler.SetPriceForNewSaleItem(newItem, priceData);
		}
	}
}
