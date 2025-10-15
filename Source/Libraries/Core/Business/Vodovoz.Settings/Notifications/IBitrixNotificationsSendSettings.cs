using System;

namespace Vodovoz.Settings.Notifications
{
	/// <summary>
	/// Настройки отправки уведомлений в Битрикс24
	/// </summary>
	public interface IBitrixNotificationsSendSettings
	{
		/// <summary>
		/// Интервал отправки уведомлений по долгам по безналу в Битрикс24
		/// </summary>
		TimeSpan CashlessDebtsNotificationsSendInterval { get; }

		/// <summary>
		/// Адрес базового URL Битрикс24
		/// </summary>
		string BitrixBaseUrl { get; }
	}
}
