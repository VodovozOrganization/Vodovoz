namespace Vodovoz.Settings.Orders
{
	public interface IClosingDeliveriesSettings
	{
		string ClosingDeliveriesNotificationEmails { get; }
		int DaysBeforeClosingDeliveries { get; }

		void UpdateClosingDeliveriesNotificationEmails(string value);
		void UpdateDaysBeforeClosingDeliveries(int daysBeforeClosingDeliveries);
	}
}
