using System;
using Vodovoz.Settings.Sms;

namespace Sms.Internal.Client.Framework
{
	public class SmsClientChannelFactory : ISmsClientChannelFactory
	{
		private readonly ISmsSettings _smsSettings;

		public SmsClientChannelFactory(ISmsSettings smsSettings)
		{
			_smsSettings = smsSettings ?? throw new ArgumentNullException(nameof(smsSettings));
		}

		public SmsClientChannel OpenChannel(string url = null)
		{
			if(!_smsSettings.SmsSendingAllowed)
			{
				throw new InvalidOperationException("Отправка смс сообщений не разрешена настройками приложения.");
			}

			string serviceUrl = url ?? _smsSettings.InternalSmsServiceUrl;
			return new SmsClientChannel(serviceUrl, _smsSettings.InternalSmsServiceApiKey);
		}
	}
}
