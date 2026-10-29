using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Vodovoz.Domain.Operations;
using Vodovoz.Settings.Notifications;

namespace BitrixNotificationsSend.Library.Options
{
	public class ConfigureLastServiceOrdersDealsCreateOptions : IConfigureOptions<LastServiceOrdersDealsCreateOptions>
	{
		private readonly IBitrixNotificationsSendSettings _bitrixNotificationsSendSettings;

		public ConfigureLastServiceOrdersDealsCreateOptions(IBitrixNotificationsSendSettings bitrixNotificationsSendSettings)
		{
			_bitrixNotificationsSendSettings = bitrixNotificationsSendSettings ?? throw new ArgumentNullException(nameof(bitrixNotificationsSendSettings));
		}

		public void Configure(LastServiceOrdersDealsCreateOptions options)
		{
			options.Interval = _bitrixNotificationsSendSettings.LastServiceOrdersNotificationsSendInterval;
			options.SendTimeFrom = _bitrixNotificationsSendSettings.LastServiceOrdersSendTimeFrom;
			options.SendTimeTo = _bitrixNotificationsSendSettings.LastServiceOrdersSendTimeTo;
		}
	}
}
