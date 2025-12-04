using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	public static partial class LogisticPermissions
	{
		public static class RouteListItem
		{
			/// <summary>
			/// Пользователь может установить статус Выполнен даже если не все коды ЧЗ были добавлены
			/// </summary>
			[Display(
				Name = "Пользователь может установить статус Выполнен даже если не все коды ЧЗ были добавлены")]
			public static string CanSetCompletedStatusWhenNotAllTrueMarkCodesAdded => "can_set_completed_status_when_not_all_true_mark_codes_added";
		}
	}
}
