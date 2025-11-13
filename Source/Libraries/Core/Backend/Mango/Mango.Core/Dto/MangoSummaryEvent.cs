using System.Text.Json.Serialization;

namespace Mango.Core.Dto
{
	public class MangoSummaryEvent
	{
		[JsonPropertyName("entry_id")]
		public string EntryId { get; set; }

		[JsonPropertyName("call_direction")]
		public MangoCallDirection CallDirection { get; set; }

		[JsonPropertyName("from")]
		public SummaryFrom From { get; set; }

		[JsonPropertyName("to")]
		public SummaryTo To { get; set; }

		[JsonPropertyName("create_time")]
		public long CreateTime { get; set; }

		[JsonPropertyName("forward_time")]
		public long ForwardTime { get; set; }

		[JsonPropertyName("talk_time")]
		public long TalkTime { get; set; }

		[JsonPropertyName("end_time")]
		public long EndTime { get; set; }

		[JsonPropertyName("entry_result")]
		public MangoCallEntryResult EntryResult { get; set; }

		[JsonPropertyName("disconnect_reason")]
		public int DisconnectReason { get; set; }

		[JsonPropertyName("sip_call_id")]
		public uint SipCallId { get; set; }
	}
}
