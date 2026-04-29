namespace CustomerOrdersApi.Library.V5.Dto.Orders
{
	public class OnlineOrderSumDto
	{
		/// <summary>
		/// Итого
		/// </summary>
		public decimal Total { get; set; }
		/// <summary>
		/// Скидка
		/// </summary>
		public decimal Discount { get; set; }
		/// <summary>
		/// Сумма без учета скидок и акций
		/// </summary>
		public decimal RawSum { get; set; }
		/// <summary>
		/// Доставка
		/// </summary>
		public decimal? Delivery { get; set; }

		public static OnlineOrderSumDto Create() => new OnlineOrderSumDto();
	}
}
