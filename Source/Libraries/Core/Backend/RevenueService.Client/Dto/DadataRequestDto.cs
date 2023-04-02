using System.Text.Json.Serialization;

namespace RevenueService.Client.Dto
{
	public class DadataRequestDto
	{
		[JsonPropertyName("query")]
		public string Inn { get; set; }

		[JsonPropertyName("kpp")]
		public string Kpp { get; set; }
	}
}
