using Newtonsoft.Json;

namespace Bitrix.DTO
{
	public class NomenclatureInfo
	{
		[JsonProperty("valueId")] 
		public string ValueId { get; set; }

		[JsonProperty("value")] 
		public int NomenclatureId { get; set; }
	}
}
