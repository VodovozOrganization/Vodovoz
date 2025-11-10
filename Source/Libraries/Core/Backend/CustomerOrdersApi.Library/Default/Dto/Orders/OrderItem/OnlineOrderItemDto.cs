using System;
using VodovozBusiness.Domain.Orders;

namespace CustomerOrdersApi.Library.Default.Dto.Orders.OrderItem
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
		
		/// <summary>
		/// Фикса
		/// </summary>
		public bool IsFixedPrice { get; set; }

		public decimal PriceWithDiscount
		{
			get
			{
				if(Discount > 0)
				{
					return !IsDiscountInMoney
						? Math.Round(Price * (100 - Discount) / 100, 2)
						: Math.Round((Price * Count - Discount) / Count, 2);
				}

				return Price;
			}
		}

		public void ClearDiscount()
		{
			Discount = 0;
			IsDiscountInMoney = false;
			DiscountReasonId = null;
		}
	}
}
