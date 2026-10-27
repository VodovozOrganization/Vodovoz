namespace DeliveryRulesService.V2.DTO
{
	/// <summary>
	/// Запрос по правилам доставки
	/// </summary>
	public class DeliveryRulesRequest
	{
		/// <summary>
		/// Долгота
		/// </summary>
		public decimal Latitude { get; set; }
		/// <summary>
		/// Широта
		/// </summary>
		public decimal Longitude { get; set; }
		/// <summary>
		/// Идентификатор точки доставки в ДВ
		/// </summary>
		public int? ErpDeliveryPointId { get; set; }
		/// <summary>
		/// Самовывоз
		/// </summary>
		public bool IsSelfDelivery { get; set; }
		/// <summary>
		/// Корзина
		/// </summary>
		public SaleItemDto[] SaleItems { get; set; }
	}
}
