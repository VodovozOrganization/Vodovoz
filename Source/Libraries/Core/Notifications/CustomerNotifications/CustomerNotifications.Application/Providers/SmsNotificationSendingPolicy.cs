using Notifications.Infrastructure;
using System;
using Vodovoz.Services;

namespace CustomerNotifications.Application.Providers
{
	/// <inheritdoc/>
	public class SmsNotificationSendingPolicy : ISmsNotificationSendingPolicy
	{
		private readonly ISmsNotifierSettings _smsNotifierSettings;

		public SmsNotificationSendingPolicy(ISmsNotifierSettings smsNotifierSettings)
		{
			_smsNotifierSettings = smsNotifierSettings ?? throw new ArgumentNullException(nameof(smsNotifierSettings));
		}

		/// <inheritdoc/>
		public bool IsSmsSendingEnabled => _smsNotifierSettings.IsSmsNotificationsEnabled;
	}
}
