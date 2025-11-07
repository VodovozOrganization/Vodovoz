<<<<<<<< HEAD:Source/Libraries/Core/Backend/CustomerOrdersApi.Library/V4/Dto/Orders/OnlineOrderItemDto.cs
﻿namespace CustomerOrdersApi.Library.V4.Dto.Orders
========
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Nodes
>>>>>>>> master:Source/Libraries/Core/Business/VodovozBusiness/Nodes/OnlineOrderItemWithFixedPrice.cs
{
	/// <summary>
	/// Позиция онлайн заказа с фиксой
	/// </summary>
	public class OnlineOrderItemWithFixedPrice :  IOnlineOrderedProductWithFixedPrice
	{
		/// <summary>
		/// Id номенклатуры в ДВ
		/// </summary>
		public int NomenclatureId { get; set; }
		/// <summary>
		/// Старая цена
		/// </summary>
		public decimal OldPrice { get; set; }
		/// <summary>
		/// Новая цена(фикса)
		/// </summary>
		public decimal? NewPrice { get; set; }
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
	}
}
