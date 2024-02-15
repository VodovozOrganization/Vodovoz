namespace Mango.CallsPublishing
{
	/*public class MessageTransportSettings : IMessageTransportSettings
	{

		private const string _transportSectionName = "MessageTransport";
		private const string _ttlSectionName = "MessagesTimeToLive";

		private readonly string _host;
		private readonly int _port;
		private readonly string _virtualHost;
		private readonly string _username;
		private readonly string _password;
		private readonly bool _useSSL;
		private readonly List<MessageTTLSetting> _messageTTLSettings = new List<MessageTTLSetting>();

		public MessageTransportSettings(IConfiguration configuration)
		{
			var transportSection = configuration.GetSection(_transportSectionName);
			if(!transportSection.Exists())
			{
				throw new ArgumentException($"Не найдена секция {_transportSectionName} в конфигурации");
			}

			_host = transportSection["Host"];
			_port = int.Parse(transportSection["Port"]);
			_virtualHost = transportSection["VirtualHost"];
			_username = transportSection["Username"];
			_password = transportSection["Password"];
			_useSSL = bool.Parse(transportSection["UseSSL"]);

			var messagesTTL = transportSection.GetSection(_ttlSectionName);
			if(!messagesTTL.Exists())
			{
				throw new ArgumentException($"Не найдена секция {_ttlSectionName} в конфигурации");
			}

			_messageTTLSettings = messagesTTL.Get<List<MessageTTLSetting>>();
		}

		public string Host => _host;
		public int Port => _port;
		public string VirtualHost => _virtualHost;
		public string User => _username;
		public string Password => _password;
		public bool UseSSL => _useSSL;
		public IEnumerable<MessageTTLSetting> MessagesTTL => _messageTTLSettings;
	}*/
}
