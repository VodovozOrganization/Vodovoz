namespace Mango.Contracts.V1.Options
{
	public class MangoOptions
	{
		public const string SectionName = "Mango";
		
		public string ApiKey { get; set; } = string.Empty;
		
		public string ApiSalt { get; set; } = string.Empty;
		
		public string BaseUrl { get; set; } = "https://app.mango-office.ru";

		public int Limit { get; set; } = 100;
	}
}
