using System.Collections.Generic;
using CustomerOrders.Contracts.V5.Orders.Discounts;

namespace CustomerOrders.Contracts.V5.Orders.OrderItem
{
	public abstract class OnlineOrderItemBaseDto
	{
		/// <summary>
		/// Id номенклатуры в ДВ
		/// </summary>
		public int NomenclatureId { get; set; }
		/// <summary>
		/// Цена
		/// </summary>
		public decimal Price { get; set; }
		/// <summary>
		/// Количество
		/// </summary>
		public decimal Count { get; set; }
		/// <summary>
		/// Id промонабора
		/// </summary>
		public int? PromoSetId { get; set; }
		/// <summary>
		/// Фикса
		/// </summary>
		public bool IsFixedPrice { get; set; }
		/// <summary>
		/// Спец условия(товар недели, акция и т.д.)
		/// </summary>
		public ExternalProductMarker? Marker { get; set; }
		/// <summary>
		/// Скидки
		/// </summary>
		public IEnumerable<DiscountDto> Discounts { get; set; } = new List<DiscountDto>();
	}
}
