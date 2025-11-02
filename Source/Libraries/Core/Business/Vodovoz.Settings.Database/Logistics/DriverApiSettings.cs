using System;
using Vodovoz.Settings.Logistics;

namespace Vodovoz.Settings.Database.Logistics
{
	public class DriverApiSettings : IDriverApiSettings
	{
		private readonly ISettingsController _settingsController;

		public DriverApiSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public int ComplaintSourceId => _settingsController.GetIntValue("web_api_complaint_source_id");
		public string CompanyPhoneNumber => _settingsController.GetStringValue("web_api_company_phone_number");

		public Uri ApiBase => new Uri(_settingsController.GetValue<string>("driver_api_base"));
		public string NotifyOfSmsPaymentStatusChangedUri => _settingsController.GetValue<string>("NotifyOfSmsPaymentStatusChangedURI");
		public string NotifyOfFastDeliveryOrderAddedUri => _settingsController.GetValue<string>("NotifyOfFastDeliveryOrderAddedURI");
		public string NotifyOfWaitingTimeChangedURI => _settingsController.GetValue<string>(nameof(NotifyOfWaitingTimeChangedURI));
		public string NotifyOfRouteListChangedUri => _settingsController.GetValue<string>(nameof(NotifyOfRouteListChangedUri));

		public static void InitializeNotifications(ISettingsController settingsController, string currentDatabaseName)
		{
			NotificationsEnabled = currentDatabaseName == settingsController.GetValue<string>("driver_api_notifications_enabled_database");
		}

		public static bool NotificationsEnabled { get; private set; }

		public string NotifyOfCashRequestForDriverIsGivenForTakeUri => _settingsController.GetValue<string>(nameof(NotifyOfCashRequestForDriverIsGivenForTakeUri));

		public int DriverApiUserId => _settingsController.GetIntValue("DriverApiUserId");
	}
}
