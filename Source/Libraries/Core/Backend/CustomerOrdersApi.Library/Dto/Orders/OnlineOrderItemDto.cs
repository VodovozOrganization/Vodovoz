using VodovozBusiness.Domain.Orders;

namespace CustomerOrdersApi.Library.Dto.Orders
{
	/// <summary>
	/// Товар онлайн заказа
	/// </summary>
	public class OnlineOrderItemDto : IOnlineOrderedProduct
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

		public void ClearDiscount()
		{
			Discount = 0;
			IsDiscountInMoney = false;
			DiscountReasonId = null;
		}
	}
}
