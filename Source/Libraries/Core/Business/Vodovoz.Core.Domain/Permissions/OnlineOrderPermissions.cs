using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	/// <summary>
	/// Права заказы
	/// </summary>
	public static partial class OnlineOrderPermissions
	{
		/// <summary>
		/// Может активировать акцию скидка на второй заказ
		/// </summary>
		[Display(
			Name = "Может отменять любой онлайн заказ",
			Description = "Может отменять любой онлайн заказ, в не зависимости у кого он в работе")]
		public static string CanCancelAnyOnlineOrder => $"OnlineOrder.{nameof(CanCancelAnyOnlineOrder)}";
	}
}
