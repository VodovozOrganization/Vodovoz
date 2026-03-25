using System;

namespace Vodovoz.Settings.Notifications
{
	/// <summary>
	/// Настройки отправки уведомлений в Битрикс24
	/// </summary>
	public interface IBitrixNotificationsSendSettings
	{
		/// <summary>
		/// Наши организации по которымм отправляются уведомлений по долгам по безналу в Битрикс24
		/// </summary>
		int[] CashlessDebtsOrganizations { get; }
		/// <summary>
		/// Интервал отправки уведомлений по долгам по безналу в Битрикс24
		/// </summary>
		TimeSpan CashlessDebtsNotificationsSendInterval { get; }

		/// <summary>
		/// Адрес базового URL Битрикс24
		/// </summary>
		string BitrixBaseUrl { get; }

		/// <summary>
		/// Токен для доступа к API Битрикс24
		/// </summary>
		string BitrixToken { get; }

		/// <summary>
		/// Таймаут клиента отправки уведомлений в Битрикс24
		/// </summary>
		TimeSpan ClientTimeout { get; }
	}
}
