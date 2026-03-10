namespace Vodovoz.Core.Data.V5
{
	/// <summary>
	/// Контракт данных по скидке 
	/// </summary>
	public interface IDiscountData
	{
		/// <summary>
		/// Скидка
		/// </summary>
		decimal Discount { get; set; }

		/// <summary>
		/// Скидка в деньгах?
		/// </summary>
		bool IsDiscountInMoney { get; set; }

		/// <summary>
		/// Id скидки/промокода
		/// </summary>
		int? DiscountReasonId { get; set; }
	}
}
