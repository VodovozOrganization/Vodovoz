using Newtonsoft.Json;

namespace BitrixApi.DTO
{
	public class OurProductInfo
	{
		[JsonProperty("valueId")] 
		public string ValueId { get; set; }

		[JsonProperty("value")] 
		public string IsOurProduct { get; set; }
	}
}
