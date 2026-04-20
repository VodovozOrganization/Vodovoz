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
	
}
