using System.Text.Json.Serialization;

namespace Mango.Core.Dto
{
	public class SummaryTo
	{
		[JsonPropertyName("extension")]
		public string Extension { get; set; }

		[JsonPropertyName("number")]
		public string Number { get; set; }

		[JsonPropertyName("line_number")]
		public string LineNumber { get; set; }
	}
}
