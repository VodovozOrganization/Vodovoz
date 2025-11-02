using System.Text.Json.Serialization;

namespace Mango.Core.Dto
{
	public class CallFrom
	{
		[JsonPropertyName("extension")]
		public string Extension { get; set; }

		[JsonPropertyName("number")]
		public string Number { get; set; }

		[JsonPropertyName("taken_from_call_id")]
		public string TakenFromCallId { get; set; }

		[JsonPropertyName("was_transfered")]
		public bool WasTransfered { get; set; }

		[JsonPropertyName("hold_initiator")]
		public bool HoldInitiator { get; set; }
	}
}
