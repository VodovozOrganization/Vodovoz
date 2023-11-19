using System;
using System.Collections.Generic;
using Vodovoz.Settings.Pacs;

namespace Vodovoz.Settings.Database.Pacs
{
	public class MessageTransportSettings : IMessageTransportSettings
	{
		private readonly ISettingsController _settingsController;

		public MessageTransportSettings(ISettingsController settingsController)
		{
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public string Host => _settingsController.GetStringValue("Pacs.MessageTranport.Host");

		public int Port => _settingsController.GetIntValue("Pacs.MessageTranport.Port");

		public string VirtualHost => _settingsController.GetStringValue("Pacs.MessageTranport.VirtualHost");

		public string Username => _settingsController.GetStringValue("Pacs.MessageTranport.Username");

		public string Password => _settingsController.GetStringValue("Pacs.MessageTranport.Password");

		public bool UseSSL => _settingsController.GetBoolValue("Pacs.MessageTranport.UseSSL");

		public List<MessageTTLSetting> MessagesTimeToLive => new List<MessageTTLSetting>();
	}
}
