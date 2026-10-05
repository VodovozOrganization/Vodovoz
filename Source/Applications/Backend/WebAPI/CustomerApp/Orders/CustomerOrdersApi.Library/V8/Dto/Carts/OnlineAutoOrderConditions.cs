using CustomerOrdersApi.Library.V8.Dto.Orders.Promotions.Discounts;

namespace CustomerOrdersApi.Library.V8.Dto.Carts
{
	/// <summary>
	/// Условия по автозаказу
	/// </summary>
	public sealed class OnlineAutoOrderConditions
	{
		/// <summary>
		/// Доступность автозаказа
		/// </summary>
		public bool IsAvailable { get; set; }
		/// <summary>
		/// Параметры скидки на заказ при подключении автозаказа
		/// </summary>
		public DiscountDto Discount { get; set; }
	}
}
