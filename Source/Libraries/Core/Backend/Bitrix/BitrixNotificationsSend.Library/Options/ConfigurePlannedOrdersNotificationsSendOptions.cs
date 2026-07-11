using Microsoft.Extensions.Options;
using Vodovoz.Settings.Notifications;

namespace BitrixNotificationsSend.Library.Options
{
	public class ConfigurePlannedOrdersNotificationsSendOptions : IConfigureOptions<PlannedOrdersNotificationsSendOptions>
	{
		private readonly IBitrixNotificationsSendSettings _bitrixNotificationsSendSettings;

		public ConfigurePlannedOrdersNotificationsSendOptions(IBitrixNotificationsSendSettings bitrixNotificationsSendSettings)
		{
			_bitrixNotificationsSendSettings = bitrixNotificationsSendSettings ?? throw new System.ArgumentNullException(nameof(bitrixNotificationsSendSettings));
		}

		public void Configure(PlannedOrdersNotificationsSendOptions options)
		{
			options.Interval = _bitrixNotificationsSendSettings.PlannedOrdersNotificationsSendInterval;
			options.SendTimeFrom = _bitrixNotificationsSendSettings.PlannedOrdersSendTimeFrom;
			options.SendTimeTo = _bitrixNotificationsSendSettings.PlannedOrdersSendTimeTo;
		}
	}
}
