using System.Text.Json.Serialization;

namespace Mango.Core.Dto
{
	public class SummaryFrom
	{
		[JsonPropertyName("extension")]
		public string Extension { get; set; }

		[JsonPropertyName("number")]
		public string Number { get; set; }
	}
}
