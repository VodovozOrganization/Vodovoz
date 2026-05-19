using System;
using Vodovoz.Settings.Orders;

namespace Vodovoz.Settings.Database.Orders
{
	public class ClosingDeliveriesSettings : IClosingDeliveriesSettings
	{
		private const string _closingDeliveriesNotificationEmailsTo = "closing_deliveries_notification_emails_to";
		private const string _daysBeforeClosingDeliveries = "days_before_closing_deliveries";

		private readonly ISettingsController _settingsController;

		public ClosingDeliveriesSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public string ClosingDeliveriesNotificationEmailsTo => _settingsController.GetStringValue(_closingDeliveriesNotificationEmailsTo);

		public void UpdateClosingDeliveriesNotificationEmails(string value) => _settingsController.CreateOrUpdateSetting(_closingDeliveriesNotificationEmailsTo, value);

		public int DaysBeforeClosingDeliveries => _settingsController.GetIntValue(_daysBeforeClosingDeliveries);

		public void UpdateDaysBeforeClosingDeliveries(int daysBeforeClosingDeliveries) => _settingsController.CreateOrUpdateSetting(_daysBeforeClosingDeliveries, daysBeforeClosingDeliveries.ToString());
	}
}
