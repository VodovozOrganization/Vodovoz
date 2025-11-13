using System.Collections.Generic;
using Vodovoz.Settings.Pacs;

namespace MessageTransport
{
	public class ConfigTransportSettings : IMessageTransportSettings
	{
		public string Host { get; set; }
		public int Port { get; set; }
		public string VirtualHost { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public bool UseSSL { get; set; }
		public bool TestMode { get; set; }
		public List<MessageTTLSetting> MessagesTimeToLive { get; set; }
		public string AllowSslPolicyErrors { get; set; }
	}
}
