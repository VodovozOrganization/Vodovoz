using System.Collections.Generic;

namespace MessageTransport
{
	public interface IMessageTransportSettings
	{
		string Host { get; }
		int Port { get; }
		string VirtualHost { get; }
		string User { get; }
		string Password { get; }
		bool UseSSL { get; }
		IEnumerable<MessageTTLSetting> MessagesTTL { get; }
	}
}
