using Newtonsoft.Json;

namespace Bitrix.DTO
{
	public class PromosetInfo
	{
		[JsonProperty("valueId")]
		public string ValueId { get; set; }

		[JsonProperty("value")]
		public int PromosetId { get; set; }
	}
}
