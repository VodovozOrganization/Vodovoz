using VodovozBusiness.Domain.Orders;

namespace CustomerOrdersApi.Library.Dto.Orders.OrderItem
{
	/// <summary>
	/// Позиция онлайн заказа
	/// </summary>
	public class OnlineOrderItemDto : IOnlineOrderedProduct
	{
		/// <summary>
		/// Id номенклатуры в ДВ
		/// </summary>
		public int NomenclatureId { get; set; }
		
		/// <summary>
		///  id основания скидки из справочника основания скидок
		/// </summary>
		public int? DiscountReasonId { get; set; }

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
		/// Фикса
		/// </summary>
		public bool IsFixedPrice { get; set; }
		
		public void ClearDiscount()
		{
			Discount = 0;
			IsDiscountInMoney = false;
			DiscountReasonId = null;
		}
	}
}
