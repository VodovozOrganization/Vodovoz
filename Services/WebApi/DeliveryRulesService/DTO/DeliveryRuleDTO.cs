using System.Text.Json.Serialization;

namespace DeliveryRulesService.DTO
{
	public class DeliveryRuleDTO
	{
		[JsonInclude]
		public string Bottles19l { get; set; }
		
		[JsonInclude]
		public string Bottles6l { get; set; }
		
		[JsonInclude]
		public string Bottles1500ml { get; set; }
		
		[JsonInclude]
		public string Bottles600ml { get; set; }
		
		[JsonInclude]
		public string Bottles500ml { get; set; }
		
		[JsonInclude]
		public string MinOrder { get; set; }
		
		[JsonInclude]
		public string Price { get; set; }
	}
}
