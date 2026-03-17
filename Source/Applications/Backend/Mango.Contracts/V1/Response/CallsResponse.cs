using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mango.Contracts.V1.Response
{
	public class CallsResponse
	{
		[JsonPropertyName("result")]
		public int Result { get; set; }

		[JsonPropertyName("status")]
		public string Status { get; set; }

		[JsonPropertyName("data")]
		public List<CallsDayBlock> Data { get; set; }
	}
	
	public sealed class CallsDayBlock
    {
        [JsonPropertyName("list")]
        public List<CallEntry> List { get; set; }

        [JsonPropertyName("period")]
        public string Period { get; set; }

        [JsonPropertyName("total_talks_duration")]
        public long? TotalTalksDuration { get; set; }

        [JsonPropertyName("total_calls_duration")]
        public long? TotalCallsDuration { get; set; }

        [JsonPropertyName("total_calls_count")]
        public int? TotalCallsCount { get; set; }
    }

    public sealed class CallEntry
    {
        [JsonPropertyName("entry_id")]
        public string EntryId { get; set; }

        [JsonPropertyName("context_type")]
        public int? ContextType { get; set; }

        [JsonPropertyName("context_status")]
        public int? ContextStatus { get; set; }

        [JsonPropertyName("caller_id")]
        public long? CallerId { get; set; }

        [JsonPropertyName("caller_name")]
        public string CallerName { get; set; }

        [JsonPropertyName("caller_number")]
        public string CallerNumber { get; set; }

        [JsonPropertyName("called_number")]
        public string CalledNumber { get; set; }

        [JsonPropertyName("context_start_time")]
        public long? ContextStartTime { get; set; }

        [JsonPropertyName("duration")]
        public int? Duration { get; set; }

        [JsonPropertyName("talk_duration")]
        public int? TalkDuration { get; set; }

        [JsonPropertyName("context_init_type")]
        public int? ContextInitType { get; set; }

        [JsonPropertyName("recall_status")]
        public int? RecallStatus { get; set; }

        [JsonPropertyName("cost")]
        public decimal? Cost { get; set; }

        [JsonPropertyName("context_calls")]
        public List<CallNode> ContextCalls { get; set; }
    }

    public sealed class CallNode
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
