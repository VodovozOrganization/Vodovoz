using System.Text.Json.Serialization;

namespace Mango.Contracts.V1.Response
{
	public class CallsStatResponse
	{
		[JsonPropertyName("key")]
		public string Key { get; set; }
	}
}
