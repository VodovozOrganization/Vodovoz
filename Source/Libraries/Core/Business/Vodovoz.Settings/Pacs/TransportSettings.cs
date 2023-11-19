using System.Collections.Generic;

namespace Vodovoz.Settings.Pacs
{
	public class TransportSettings : IMessageTransportSettings
	{
		public string Host { get; set; }
		public int Port { get; set; }
		public string VirtualHost { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public bool UseSSL { get; set; }
		public List<MessageTTLSetting> MessagesTimeToLive { get; set; }
	}
}
