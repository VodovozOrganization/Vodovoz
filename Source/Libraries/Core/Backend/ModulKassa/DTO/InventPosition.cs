using Newtonsoft.Json;

namespace ModulKassa.DTO
{
	public class InventPosition
	{
		[JsonProperty("name", Required = Required.Always)]
		public string Name { get; set; }

		[JsonProperty("quantity", Required = Required.Always)]
		public decimal Quantity { get; set; }

		[JsonProperty("price", Required = Required.Always)]
		public decimal PriceWithoutDiscount { get; set; }

		[JsonProperty("discSum")]
		public decimal DiscSum { get; set; }

		[JsonProperty("productMark", Required = Required.Always)]
		public string ProductMark { get; set; }

		[JsonProperty("vatTag", Required = Required.Always)]
		public int VatTag { get; set; }

		/// <summary>
		/// Разрешительный режим
		/// </summary>
		[JsonProperty("industryRequisite", Required = Required.Always)]
		public IndustryRequisite IndustryRequisite { get; set; }
	}
}
