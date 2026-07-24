namespace VodovozBusiness.Domain.Orders
{
	public interface IOnlineOrderedProduct : ICanApplyFixedPriceOnline
	{
		/// <summary>
		/// Цена
		/// </summary>
		decimal Price { get; set; }
		/// <summary>
		/// Скидка в деньгах?
		/// </summary>
		bool IsDiscountInMoney { get; set; }
		/// <summary>
		/// Скидка
		/// </summary>
		decimal Discount { get; set; }
		/// <summary>
		/// Id скидки/промокода
		/// </summary>
		int? DiscountReasonId { get; set; }
		/// <summary>
		/// Фикса
		/// </summary>
		bool IsFixedPrice { get; set; }
		/// <summary>
		/// Очистка данных по скидке
		/// </summary>
		void ClearDiscount();
	}
}
