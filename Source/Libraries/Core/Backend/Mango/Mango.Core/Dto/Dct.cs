using System.Text.Json.Serialization;

namespace Mango.Core.Dto
{
	public class Dct
	{
		[JsonPropertyName("number")]
		public string Number { get; set; }

		[JsonPropertyName("type")]
		public MangoCallDctType Type { get; set; }
	}
}
