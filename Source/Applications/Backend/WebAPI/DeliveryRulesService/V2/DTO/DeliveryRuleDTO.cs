using System.Text.Json.Serialization;

namespace DeliveryRulesService.V2.DTO
{
	/// <summary>
	/// Правило доставки
	/// </summary>
	public class DeliveryRuleDTO
	{
		/// <summary>
		/// 19л бутылей
		/// </summary>
		[JsonPropertyOrder(1)]
		public string Bottles19l { get; set; }
		
		/// <summary>
		/// 6л бутылей
		/// </summary>
		[JsonPropertyOrder(4)]
		public string Bottles6l { get; set; }
		
		/// <summary>
		/// 1,5л бутылок
		/// </summary>
		[JsonPropertyOrder(0)]
		public string Bottles1500ml { get; set; }
		
		/// <summary>
		/// 0,6л бутылок
		/// </summary>
		[JsonPropertyOrder(3)]
		public string Bottles600ml { get; set; }
		
		/// <summary>
		/// 0,5л бутылок
		/// </summary>
		[JsonPropertyOrder(2)]
		public string Bottles500ml { get; set; }
		
		/// <summary>
		/// Минимальная сумма заказа для товаров ИМ(не активно, т.к. тот ИМ не работает)
		/// </summary>
		[JsonPropertyOrder(5)]
		public string MinOrder { get; set; }
		
		/// <summary>
		/// Цена доставки
		/// </summary>
		[JsonPropertyOrder(6)]
		public string Price { get; set; }
	}
}
