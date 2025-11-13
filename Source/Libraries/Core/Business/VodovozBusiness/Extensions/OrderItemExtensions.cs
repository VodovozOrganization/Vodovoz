using Vodovoz.Domain.Orders;

namespace Vodovoz.Extensions
{
	public static class OrderItemExtensions
	{
		internal static void UpdatePriceWithRecalculate(this OrderItem newItem, decimal price)
		{
			newItem.SetPrice(price);
			newItem.RecalculateDiscountAndVat();
		}
	}
}
