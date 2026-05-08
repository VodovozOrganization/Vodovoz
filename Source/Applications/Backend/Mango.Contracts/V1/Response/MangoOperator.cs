using System.Text.Json.Serialization;

namespace Mango.Contracts.V1.Response
{
	public class MangoOperator
	{
		[JsonPropertyName("id")]
		public long Id { get; set; }

		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("extension")]
		public string Extension { get; set; }

		[JsonPropertyName("priority")]
		public int? Priority { get; set; }

		[JsonPropertyName("order")]
		public int? Order { get; set; }
	}
}
