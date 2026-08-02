namespace CustomerOrdersApi.Library.V5.Dto.Orders
{
	public class OrderItemDto
	{
		private OrderItemDto(
			int nomenclatureId,
			decimal count,
			decimal price,
			bool isDiscountInMoney,
			decimal discount,
			bool giftItem)
		{
			NomenclatureId = nomenclatureId;
			Count = count;
			Price = price;
			IsDiscountInMoney = isDiscountInMoney;
			Discount = discount;
			GiftItem = giftItem;
		}
		
		/// <summary>
		/// Id номенклатуры
		/// </summary>
		public int NomenclatureId { get; }
		
		/// <summary>
		/// Количество
		/// </summary>
		public decimal Count { get; }
		
		/// <summary>
		/// Цена
		/// </summary>
		public decimal Price { get; }
		
		/// <summary>
		/// Скидка в деньгах?
		/// </summary>
		public bool IsDiscountInMoney { get; }

		/// <summary>
		/// Скидка
		/// </summary>
		public decimal Discount { get; }

		/// <summary>
		/// Этот товар - подарок?
		/// </summary>
		public bool GiftItem { get; }

		public static OrderItemDto Create(
			int nomenclatureId,
			decimal count,
			decimal price,
			bool isDiscountInMoney,
			decimal discount,
			bool giftItem) =>
			new OrderItemDto(nomenclatureId, count, price, isDiscountInMoney, discount, giftItem);
	}
}
