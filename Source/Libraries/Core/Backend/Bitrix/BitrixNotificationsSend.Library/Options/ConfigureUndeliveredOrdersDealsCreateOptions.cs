using Microsoft.Extensions.Options;
using System;
using Vodovoz.Settings.Notifications;

namespace BitrixNotificationsSend.Library.Options
{
	public class ConfigureUndeliveredOrdersDealsCreateOptions : IConfigureOptions<UndeliveredOrdersDealsCreateOptions>
	{
		private readonly IBitrixNotificationsSendSettings _bitrixNotificationsSendSettings;

		public ConfigureUndeliveredOrdersDealsCreateOptions(IBitrixNotificationsSendSettings bitrixNotificationsSendSettings)
		{
			_bitrixNotificationsSendSettings = bitrixNotificationsSendSettings ?? throw new ArgumentNullException(nameof(bitrixNotificationsSendSettings));
		}

		public void Configure(UndeliveredOrdersDealsCreateOptions options)
		{
			options.Interval = _bitrixNotificationsSendSettings.UndeliveredOrdersSendInterval;
			options.MinLastEditedTime = _bitrixNotificationsSendSettings.UndeliveredOrdersMinLastEditedTime;
		}
	}
}
