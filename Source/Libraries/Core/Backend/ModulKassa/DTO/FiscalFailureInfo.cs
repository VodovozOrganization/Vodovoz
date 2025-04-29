using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ModulKassa.DTO
{
	public class FiscalFailureInfo
	{
		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }
	}
}
