using Microsoft.Extensions.Options;
using Vodovoz.Settings.Notifications;

namespace BitrixNotificationsSend.Library.Options
{
	public class ConfigureCashlessDebtsNotificationsSendOptions : IConfigureOptions<CashlessDebtsNotificationsSendOptions>
	{
		private readonly IBitrixNotificationsSendSettings _bitrixNotificationsSendSettings;

		public ConfigureCashlessDebtsNotificationsSendOptions(IBitrixNotificationsSendSettings bitrixNotificationsSendSettings)
		{
			_bitrixNotificationsSendSettings = bitrixNotificationsSendSettings ?? throw new System.ArgumentNullException(nameof(bitrixNotificationsSendSettings));
		}

		public void Configure(CashlessDebtsNotificationsSendOptions options)
		{
			options.Interval = _bitrixNotificationsSendSettings.CashlessDebtsNotificationsSendInterval;
		}
	}
}
