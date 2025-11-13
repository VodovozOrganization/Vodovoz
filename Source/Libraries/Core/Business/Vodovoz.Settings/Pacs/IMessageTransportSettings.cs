using System.Collections.Generic;

namespace Vodovoz.Settings.Pacs
{
	public interface IMessageTransportSettings
	{
		string Host { get; }
		int Port { get; }
		string VirtualHost { get; }
		string Username { get; }
		string Password { get; }
		bool UseSSL { get; }
		List<MessageTTLSetting> MessagesTimeToLive { get; }
		string AllowSslPolicyErrors { get; }
		bool TestMode { get; }
	}
}
