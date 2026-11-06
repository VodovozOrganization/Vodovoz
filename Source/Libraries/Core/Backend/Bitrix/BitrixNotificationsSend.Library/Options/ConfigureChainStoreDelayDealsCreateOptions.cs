using Microsoft.Extensions.Options;
using System;
using Vodovoz.Settings.Notifications;

namespace BitrixNotificationsSend.Library.Options
{
	public class ConfigureChainStoreDelayDealsCreateOptions : IConfigureOptions<ChainStoreDelayDealsCreateOptions>
	{
		private readonly IBitrixNotificationsSendSettings _bitrixNotificationsSendSettings;

		public ConfigureChainStoreDelayDealsCreateOptions(IBitrixNotificationsSendSettings bitrixNotificationsSendSettings)
		{
			_bitrixNotificationsSendSettings = bitrixNotificationsSendSettings
				?? throw new ArgumentNullException(nameof(bitrixNotificationsSendSettings));
		}

		public void Configure(ChainStoreDelayDealsCreateOptions options)
		{
			options.Interval = _bitrixNotificationsSendSettings.ChainStoreDelayDealsSendInterval;
			options.SendTimeFrom = _bitrixNotificationsSendSettings.ChainStoreDelayDealsSendTimeFrom;
			options.SendTimeTo = _bitrixNotificationsSendSettings.ChainStoreDelayDealsSendTimeTo;
		}
	}
}
