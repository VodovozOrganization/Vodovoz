using Microsoft.Extensions.Options;
using Vodovoz.Settings.Notifications;

namespace BitrixNotificationsSend.Library.Options
{
	public class ConfigurePlannedOrdersDealsCreateOptions : IConfigureOptions<PlannedOrdersDealsCreateOptions>
	{
		private readonly IBitrixNotificationsSendSettings _bitrixNotificationsSendSettings;

		public ConfigurePlannedOrdersDealsCreateOptions(IBitrixNotificationsSendSettings bitrixNotificationsSendSettings)
		{
			_bitrixNotificationsSendSettings = bitrixNotificationsSendSettings ?? throw new System.ArgumentNullException(nameof(bitrixNotificationsSendSettings));
		}

		public void Configure(PlannedOrdersDealsCreateOptions options)
		{
			options.Interval = _bitrixNotificationsSendSettings.PlannedOrdersNotificationsSendInterval;
			options.SendTimeFrom = _bitrixNotificationsSendSettings.PlannedOrdersSendTimeFrom;
			options.SendTimeTo = _bitrixNotificationsSendSettings.PlannedOrdersSendTimeTo;
		}
	}
}
