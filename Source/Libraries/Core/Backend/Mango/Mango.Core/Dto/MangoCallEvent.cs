using System.Text.Json.Serialization;

namespace Mango.Core.Dto
{
	public class MangoCallEvent
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
		public MangoCallState CallState { get; set; }

		[JsonPropertyName("location")]
		public MangoCallLocation Location { get; set; }

		[JsonPropertyName("from")]
		public CallFrom From { get; set; }

		[JsonPropertyName("to")]
		public CallTo To { get; set; }

		[JsonPropertyName("disconnect_reason")]
		public int? DisconnectReason { get; set; }

		[JsonPropertyName("dct")]
		public Dct Dct { get; set; }

		[JsonPropertyName("transfer")]
		public MangoCallTransferType? Transfer { get; set; }

		[JsonPropertyName("sip_call_id")]
		public string SipCallId { get; set; }

		[JsonPropertyName("command_id")]
		public string CommandId { get; set; }

		[JsonPropertyName("task_id")]
		public int? TaskId { get; set; }

		[JsonPropertyName("callback_initiator")]
		public string CallbackInitiator { get; set; }
	}
}
