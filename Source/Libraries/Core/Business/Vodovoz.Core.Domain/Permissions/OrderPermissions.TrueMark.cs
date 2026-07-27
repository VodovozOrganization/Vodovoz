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
		public static string CanTransferRejectedCodesFromCanceledOrder =>
			nameof(CanTransferRejectedCodesFromCanceledOrder);
	}
}
