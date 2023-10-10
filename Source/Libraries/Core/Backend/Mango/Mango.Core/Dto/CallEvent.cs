using System.Text.Json.Serialization;

namespace Mango.Core.Dto
{
	public class CallEvent
	{
		[JsonPropertyName("entry_id")]
		public string EntryId { get; set; }

		[JsonPropertyName("call_id")]
		public string CallId { get; set; }

		[JsonPropertyName("timestamp")]
		public long Timestamp { get; set; }

		[JsonPropertyName("seq")]
		public uint Seq { get; set; }

		[JsonPropertyName("call_state")]
		public string CallState { get; set; }

		[JsonPropertyName("location")]
		public string Location { get; set; }

		[JsonPropertyName("from")]
		public FromCaller From { get; set; }

		[JsonPropertyName("to")]
		public ToCaller To { get; set; }

		[JsonPropertyName("disconnect_reason")]
		public int DisconnectReason { get; set; }

		[JsonPropertyName("dct")]
		public Dct Dct { get; set; }

		[JsonPropertyName("sip_call_id")]
		public string SipCallId { get; set; }

		[JsonPropertyName("transfer")]
		public Transfer Transfer { get; set; }

		[JsonPropertyName("command_id")]
		public string CommandId { get; set; }

		[JsonPropertyName("task_id")]
		public string TaskId { get; set; }

		[JsonPropertyName("callback_initiator")]
		public string CallbackInitiator { get; set; }
	}
}
