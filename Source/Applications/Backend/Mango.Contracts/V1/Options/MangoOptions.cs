namespace Mango.Contracts.V1.Options
{
	public class MangoOptions
	{
		public const string SectionName = "Mango";
		
		public string ApiKey { get; set; } = string.Empty;
		
		public string ApiSalt { get; set; } = string.Empty;
		
		public string GroupsUrl { get; set; } = "https://app.mango-office.ru/vpbx/groups";
		
		public string CallsUrl { get; set; } = "https://app.mango-office.ru/vpbx/stats/calls/request/";
		
		public string CallsResult { get; set; } = "https://app.mango-office.ru/vpbx/stats/calls/result/";

		public int Limit { get; set; } = 100;
	}
}
