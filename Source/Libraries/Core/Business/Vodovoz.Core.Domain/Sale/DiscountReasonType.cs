using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Sale
{
	public enum DiscountReasonType
	{
		/// <summary>
		/// Простая скидка
		/// </summary>
		[Display(Name = "Обычная скидка")]
		Discount = -1,
		/// <summary>
		/// Скидка при первом заказе в МП/сайте
		/// </summary>
		[Display(Name = "Скидка при первом заказе в МП/сайте")]
		FirstOnlineOrderDiscount = 1,
		/// <summary>
		/// Промокод
		/// </summary>
		[Display(Name = "Промокод")]
		PromoCode = 2,
		/// <summary>
		/// Автозаказ
		/// </summary>
		[Display(Name = "Автозаказ")]
		AutoOrder = 3
	}
}
