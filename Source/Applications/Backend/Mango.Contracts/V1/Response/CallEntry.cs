using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mango.Contracts.V1.Response
{
	public class CallEntry
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
}
