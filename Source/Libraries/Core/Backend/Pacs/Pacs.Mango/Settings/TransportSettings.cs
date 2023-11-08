using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Pacs.Mango.Settings
{
	public class TransportSettings : IMessageTransportSettings
	{
		private const string _transportSectionName = "MessageTransport";
		private const string _ttlSectionName = "MessagesTimeToLive";

		private readonly List<MessageTTLSetting> _messageTTLSettings = new List<MessageTTLSetting>();

		public TransportSettings(IConfiguration configuration)
		{
			var transportSection = configuration.GetSection(_transportSectionName);
			if(!transportSection.Exists())
			{
				throw new ArgumentException($"Не найдена секция {_transportSectionName} в конфигурации");
			}

			var messagesTTL = transportSection.GetSection(_ttlSectionName);
			if(!messagesTTL.Exists())
			{
				throw new ArgumentException($"Не найдена секция {_ttlSectionName} в конфигурации");
			}

			foreach(var messageTTL in messagesTTL.GetChildren())
			{
				var messageTTLSetting = new MessageTTLSetting
				{
					ClassFullName = messageTTL.Key,
					TTL = int.Parse(messagesTTL.Value)
				};
				_messageTTLSettings.Add(messageTTLSetting);
			}
		}

		public IEnumerable<MessageTTLSetting> MessagesTTL => _messageTTLSettings;
	}
}
