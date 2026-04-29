using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace CustomerOrdersApi.Library.V5.Dto.Orders.OrderItem
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
		/// Скидка в деньгах?
		/// </summary>
		public bool IsDiscountInMoney { get; set; }
		/// <summary>
		/// Скидка
		/// </summary>
		public decimal Discount { get; set; }
		/// <summary>
		/// Id промонабора
		/// </summary>
		public int? PromoSetId { get; set; }
		/// <summary>
		/// Id скидки/промокода
		/// </summary>
		public int? DiscountReasonId { get; set; }
		/// <summary>
		/// Фикса
		/// </summary>
		public bool IsFixedPrice { get; set; }
		/// <summary>
		/// Спец условия(товар недели, акция и т.д.)
		/// </summary>
		public NomenclatureOnlineMarker? Marker { get; set; }
	}
}
