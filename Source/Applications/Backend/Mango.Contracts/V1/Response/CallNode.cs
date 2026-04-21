using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mango.Contracts.V1.Response
{
	public class CallNode
	{
		[JsonPropertyName("call_type")]
		public string CallType { get; set; }

		[JsonPropertyName("call_abonent_id")]
		public long? CallAbonentId { get; set; }

		[JsonPropertyName("call_abonent_info")]
		public string CallAbonentInfo { get; set; }

		[JsonPropertyName("call_abonent_number")]
		public string CallAbonentNumber { get; set; }

		[JsonPropertyName("call_start_time")]
		public long? CallStartTime { get; set; }

		[JsonPropertyName("call_answer_time")]
		public long? CallAnswerTime { get; set; }

		[JsonPropertyName("call_end_time")]
		public long? CallEndTime { get; set; }

		[JsonPropertyName("call_duration")]
		public int? CallDuration { get; set; }

		[JsonPropertyName("talk_duration")]
		public int? TalkDuration { get; set; }

		[JsonPropertyName("dial_duration")]
		public int? DialDuration { get; set; }

		[JsonPropertyName("hold_duration")]
		public int? HoldDuration { get; set; }

		[JsonPropertyName("call_end_reason")]
		public int? CallEndReason { get; set; }

		[JsonPropertyName("DirectionInbound")]
		public bool? DirectionInbound { get; set; }

		[JsonPropertyName("DirectionOutbound")]
		public bool? DirectionOutbound { get; set; }

		[JsonPropertyName("ModeConversation")]
		public bool? ModeConversation { get; set; }

		[JsonPropertyName("ModeGroup")]
		public bool? ModeGroup { get; set; }

		[JsonPropertyName("members")]
		public List<CallNode> Members { get; set; }
	}
}
