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
		/// Адрес базового URL Битрикс24 для создания сделок
		/// </summary>
		string BitrixDealsBaseUrl { get; }

		/// <summary>
		/// Id пользователя Битрикс24 для создания сделок
		/// </summary>
		string BitrixDealsUser { get; }

		/// <summary>
		/// Токен доступа Битрикс24 для создания сделок
		/// </summary>
		string BitrixDealsToken { get; }

		/// <summary>
		/// Интервал проверки необходимости создания сделок по плановым заказам в Битрикс24
		/// </summary>
		TimeSpan PlannedOrdersNotificationsSendInterval { get; }

		/// <summary>
		/// Время начала интервала для создания сделок по плановым заказам в Битрикс24 (по Москве)
		/// </summary>
		TimeSpan PlannedOrdersSendTimeFrom { get; }

		/// <summary>
		/// Время окончания интервала для создания сделок по плановым заказам в Битрикс24 (по Москве)
		/// </summary>
		TimeSpan PlannedOrdersSendTimeTo { get; }

		/// <summary>
		/// Количество дней до планируемого заказа, если у клиента был только один заказ
		/// </summary>
		int PlannedOrdersDaysToNextOrderAfterSingleOrder { get; }

		/// <summary>
		/// Включена ли отправка уведомлений по плановым заказам в Битрикс24
		/// Когда выключена, воркер отправки не выполняет работу
		/// </summary>
		bool PlannedOrdersNotificationsSendEnabled { get; }

		/// <summary>
		/// Интервал проверки необходимости создания сделок по последним сервисным заказам в Битрикс24
		/// </summary>
		TimeSpan LastServiceOrdersNotificationsSendInterval { get; }

		/// <summary>
		/// Время начала интервала для создания сделок по последним сервисным заказам в Битрикс24 (по Москве)
		/// </summary>
		TimeSpan LastServiceOrdersSendTimeFrom { get; }

		/// <summary>
		/// Время окончания интервала для создания сделок по последним сервисным заказам в Битрикс24 (по Москве)
		/// </summary>
		TimeSpan LastServiceOrdersSendTimeTo { get; }

		/// <summary>
		/// Включена ли отправка уведомлений по последним сервисным заказам в Битрикс24
		/// </summary>
		bool LastServiceOrdersNotificationsSendEnabled { get; }
	}
}
