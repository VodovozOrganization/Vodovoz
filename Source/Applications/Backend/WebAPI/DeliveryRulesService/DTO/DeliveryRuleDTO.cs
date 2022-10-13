using System.Text.Json.Serialization;

namespace DeliveryRulesService.DTO
{
	public class DeliveryRuleDTO
	{
		[JsonPropertyOrder(1)]
		public string Bottles19l { get; set; }
		
		[JsonPropertyOrder(4)]
		public string Bottles6l { get; set; }
		
		[JsonPropertyOrder(0)]
		public string Bottles1500ml { get; set; }
		
		[JsonPropertyOrder(3)]
		public string Bottles600ml { get; set; }
		
		[JsonPropertyOrder(2)]
		public string Bottles500ml { get; set; }
		
		[JsonPropertyOrder(5)]
		public string MinOrder { get; set; }
		
		[JsonPropertyOrder(6)]
		public string Price { get; set; }
	}
}
