namespace CustomerAppsApi.Library.Configs
{
	public class RabbitOptions
	{
		public const string Path = nameof(RabbitOptions);
		
		public string SendExchange { get; set; }
		public string SendQueue { get; set; }
	}
}
