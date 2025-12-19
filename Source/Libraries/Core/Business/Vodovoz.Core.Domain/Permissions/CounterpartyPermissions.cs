using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	public static class CounterpartyPermissions
	{
		/// <summary>
		/// Пересчет классификации контрагентов
		/// </summary>
		public static string CanCalculateCounterpartyClassifications => "can_calculate_counterparty_classifications";

		/// <summary>
		/// Редактирование рефера клиента
		/// </summary>
		[Display(
			Name = "Редактирование рефера клиента",
			Description = "Дает возможность редактировать рефера клиента")]
		public static string CanEditClientRefer => nameof(CanEditClientRefer);

		/// <summary>
		/// Возможность включать/выключать рассылку
		/// </summary>
		[Display(
			Name = "Возможность включать/выключать рассылку",
			Description = "Пользователь может включать/выключать рассылку писем о задолженности")]
		public static string CanEditDebtNotification => "can_edit_debt_notification_setting";
	}
}
