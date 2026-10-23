using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	public static partial class OrderPermissions
	{
		/// <summary>
		/// Перенос отклоненных кодов Честного знака из отмененного заказа
		/// </summary>
		[Display(
			Name = "Перенос отклоненных кодов Честного знака из отмененного заказа",
			Description = "Пользователь может переносить отклоненные коды Честного знака из отмененного заказа")]
		public static string CanReuseRejectedCodesFromCanceledOrder =>
			"can_reuse_rejected_codes_from_canceled_order";


		/// <summary>
		/// Ввод кода маркировки вручную в заказ
		/// </summary>
		[Display(
			Name = "Ввод кода маркировки вручную в заказ",
			Description = "Пользователь может вручную вводить коды маркировки в заказ")]
		public static string CanAddCodesToOrderManually =>
			"can_add_codes_to_order_manually";
	}
}
