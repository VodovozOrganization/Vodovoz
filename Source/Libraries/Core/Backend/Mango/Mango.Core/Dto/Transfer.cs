using System.Text.Json.Serialization;

namespace Mango.Core.Dto
{
	public class Transfer
	{
		[JsonPropertyName("consultative")]
		public int Consultative { get; set; }

		[JsonPropertyName("blind")]
		public int Blind { get; set; }

		[JsonPropertyName("return_blind")]
		public int ReturnBlind { get; set; }
	}
}
