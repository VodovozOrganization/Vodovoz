namespace Pacs.MangoCalls.Settings
{
	/*public class TransportSettings : IMessageTransportSettings
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

		public string Host => throw new NotImplementedException();

		public int Port => throw new NotImplementedException();

		public string VirtualHost => throw new NotImplementedException();

		public string User => throw new NotImplementedException();

		public string Password => throw new NotImplementedException();

		public bool UseSSL => throw new NotImplementedException();
	}*/

	//public class TransportSettings : IMessageTransportSettings
	//{
	//	public string Host { get; set; }
	//	public int Port { get; set; }
	//	public string VirtualHost { get; set; }
	//	public string User { get; set; }
	//	public string Password { get; set; }
	//	public bool UseSSL { get; set; }
	//	public List<MessageTTLSetting> MessagesTimeToLive { get; set; }
	//}
}
