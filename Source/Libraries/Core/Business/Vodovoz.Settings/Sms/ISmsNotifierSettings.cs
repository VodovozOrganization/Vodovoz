namespace Vodovoz.Services
{
	public interface ISmsNotifierSettings
	{
		bool IsSmsNotificationsEnabled { get; }

		/// <summary>
		/// Разрешена ли отправка смс уведомлений в качестве резервных уведомлений для пользователей, которые не используют мобильное приложение
		/// </summary>
		bool IsSmsFallbackNotificationsEnabled { get; }

		string NewClientSmsTextTemplate { get; }
		decimal LowBalanceLevel { get; }
		string LowBalanceNotifiedPhone { get; }
		string LowBalanceNotifyText { get; }
		string UndeliveryAutoTransferNotApprovedTextTemplate { get; }

		/// <summary>
		/// Шаблон текста смс уведомления о том, что курьер в пути
		/// </summary>
		string CourierOnTheWaySmsTextTemplate { get; }
	}
}
