using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Permissions
{
	/// <summary>
	/// Права Основание скидки
	/// </summary>
	public static class DiscountReasonPermissions
	{
		/// <summary>
		/// Пользователь может менять параметры промокода
		/// </summary>
		[Display(
			Name = "Пользователь может менять параметры промокода",
			Description = "Пользователь может менять параметры промокода")]
		public static string CanEditPromoCode => "can_edit_promo_code";
	}
}
