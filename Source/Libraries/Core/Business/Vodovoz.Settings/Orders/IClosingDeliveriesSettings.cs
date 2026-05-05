namespace Vodovoz.Settings.Orders
{
	public interface IClosingDeliveriesSettings
	{
		/// <summary>
		/// Почта, на которую будут приходить уведомления о закрытии поставок. Несколько адресов можно указать через точку с запятой.
		/// </summary>
		string ClosingDeliveriesNotificationEmails { get; }

		/// <summary>
		/// Дней сверх просрочки до закрытия поставок
		/// </summary>
		int DaysBeforeClosingDeliveries { get; }

		void UpdateClosingDeliveriesNotificationEmails(string value);
		void UpdateDaysBeforeClosingDeliveries(int daysBeforeClosingDeliveries);
	}
}
