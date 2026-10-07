using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Permissions
{
	public static class CounterpartyPermissions
	{
		/// <summary>
		/// Архивация телефона с удалением привязанных пользователей ИПЗ и данных сопоставления.
		/// </summary>
		[Display(
			Name = "Архивация телефона, привязанного к пользователю(ям) ИПЗ",
			Description = "Разрешает архивацию телефона с удалением связанных пользователей ИПЗ, заявок и уведомлений о сопоставлении")]
		public static string CanArchivePhoneWithExternalCounterparties => nameof(CanArchivePhoneWithExternalCounterparties);

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
		/// Доступен ли массовый пересчет отсрочки платежей
		/// </summary>
		[Display(
			Name = "Массовый пересчет отсрочки платежей",
			Description = "Доступен ли массовый пересчет отсрочки платежей")]
		public static string CanMassiveChangePaymentDeferment => "can_massive_change_payment_deferment";

		/// <summary>
		/// Возможность включать/выключать рассылку
		/// </summary>
		[Display(
			Name = "Возможность включать/выключать рассылку",
			Description = "Пользователь может включать/выключать рассылку писем о задолженности")]
		public static string CanEditDebtNotification => "can_edit_debt_notification_setting";
	}
}
