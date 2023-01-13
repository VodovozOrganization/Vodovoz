using System.Text.Json.Serialization;

namespace VodovozMangoService.DTO
{
    public class FromCaller
    {
		[JsonPropertyName("extension")]
		public string Extension { get; set; }

		[JsonPropertyName("number")]
		public string Number { get; set; }

		[JsonPropertyName("taken_from_call_id")]
		public string TakenFromCallId { get; set; }

		[JsonPropertyName("was_transfered")]
		public bool WasTransfered { get; set; } = false;

		[JsonPropertyName("hold_initiator")]
		public string HoldInitiator { get; set; }

		#region Calculated
		public uint? ExtensionUint => uint.TryParse (Extension, out var i) ? (uint?) i : null;
        #endregion
    }
}
