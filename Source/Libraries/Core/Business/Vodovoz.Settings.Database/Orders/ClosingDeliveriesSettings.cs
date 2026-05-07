using System;
using Vodovoz.Settings.Orders;

namespace Vodovoz.Settings.Database.Orders
{
	public class ClosingDeliveriesSettings : IClosingDeliveriesSettings
	{
		private const string _closingDeliveriesNotificationEmails = "closing_deliveries_notification_emails";
		private const string _daysBeforeClosingDeliveries = "days_before_closing_deliveries";

		private readonly ISettingsController _settingsController;

		public ClosingDeliveriesSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public string ClosingDeliveriesNotificationEmails => _settingsController.GetStringValue(_closingDeliveriesNotificationEmails);

		public void UpdateClosingDeliveriesNotificationEmails(string value) => _settingsController.CreateOrUpdateSetting(_closingDeliveriesNotificationEmails, value);

		public int DaysBeforeClosingDeliveries => _settingsController.GetIntValue(_daysBeforeClosingDeliveries);

		public void UpdateDaysBeforeClosingDeliveries(int daysBeforeClosingDeliveries) => _settingsController.CreateOrUpdateSetting(_daysBeforeClosingDeliveries, daysBeforeClosingDeliveries.ToString());
	}
}
