using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mango.Contracts.V1.Response
{
	public class CallsDayBlock
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
}
