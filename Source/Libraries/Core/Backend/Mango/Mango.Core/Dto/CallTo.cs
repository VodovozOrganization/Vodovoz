using System.Text.Json.Serialization;

namespace Mango.Core.Dto
{
	public class CallTo
	{
		[JsonPropertyName("extension")]
		public string Extension { get; set; }

		[JsonPropertyName("number")]
		public string Number { get; set; }

		[JsonPropertyName("line_number")]
		public string LineNumber { get; set; }

		[JsonPropertyName("acd_group")]
		public string AcdGroup { get; set; }

		[JsonPropertyName("was_transfered")]
		public bool WasTransfered { get; set; } = false;

		[JsonPropertyName("hold_initiator")]
		public bool HoldInitiator { get; set; }
	}
}
