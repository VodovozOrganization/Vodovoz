using System.Text.Json.Serialization;

namespace VodovozMangoService.DTO
{
	public class Dct
	{
		[JsonPropertyName("number")]
		public string Number { get; set; }

		[JsonPropertyName("type")]
		public int Type { get; set; }
	}
}
