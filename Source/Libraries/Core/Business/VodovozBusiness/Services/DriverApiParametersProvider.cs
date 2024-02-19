using System;
using Vodovoz.Parameters;

namespace Vodovoz.Services
{
	public class DriverApiParametersProvider : IDriverApiParametersProvider
	{
		private readonly IParametersProvider _parametersProvider;

		public DriverApiParametersProvider(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public int ComplaintSourceId => _parametersProvider.GetIntValue("web_api_complaint_source_id");
		public string CompanyPhoneNumber => _parametersProvider.GetStringValue("web_api_company_phone_number");

		public Uri ApiBase => new Uri(_parametersProvider.GetValue<string>("driver_api_base"));
		public string NotifyOfSmsPaymentStatusChangedUri => _parametersProvider.GetValue<string>("NotifyOfSmsPaymentStatusChangedURI");
		public string NotifyOfFastDeliveryOrderAddedUri => _parametersProvider.GetValue<string>("NotifyOfFastDeliveryOrderAddedURI");
		public string NotifyOfWaitingTimeChangedURI => _parametersProvider.GetValue<string>(nameof(NotifyOfWaitingTimeChangedURI));

		public static void InitializeNotifications(IParametersProvider parametersProvider, string currentDatabaseName)
		{
			NotificationsEnabled = currentDatabaseName == parametersProvider.GetValue<string>("driver_api_notifications_enabled_database");
		}

		public static bool NotificationsEnabled { get; private set; }
	}
}
