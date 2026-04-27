using System.Text.Json.Serialization;

namespace Mango.Contracts.V1.Request
{
	public class CallsRequest
	{
		[JsonPropertyName("key")]
		public string Key { get; set; }
	}
}
