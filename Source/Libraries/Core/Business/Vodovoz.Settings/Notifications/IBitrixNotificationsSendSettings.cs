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

		/// <summary>
		/// Интервал проверки необходимости отправки уведомлений по плановым заказам в Битрикс24
		/// </summary>
		TimeSpan PlannedOrdersNotificationsSendInterval { get; }

		/// <summary>
		/// Время начала интервала отправки уведомлений по плановым заказам в Битрикс24 (по Москве)
		/// </summary>
		TimeSpan PlannedOrdersSendTimeFrom { get; }

		/// <summary>
		/// Время окончания интервала отправки уведомлений по плановым заказам в Битрикс24 (по Москве)
		/// </summary>
		TimeSpan PlannedOrdersSendTimeTo { get; }

		/// <summary>
		/// Адрес базового URL Битрикс24 для отправки уведомлений по плановым заказам
		/// </summary>
		string PlannedOrdersBitrixBaseUrl { get; }

		/// <summary>
		/// Токен для доступа к API Битрикс24 для отправки уведомлений по плановым заказам
		/// </summary>
		string PlannedOrdersBitrixToken { get; }
	}
}
