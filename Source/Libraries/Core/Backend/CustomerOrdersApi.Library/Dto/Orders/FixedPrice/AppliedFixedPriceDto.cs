﻿using System.Collections.Generic;
using VodovozBusiness.Domain.Orders;

namespace CustomerOrdersApi.Library.Dto.Orders.FixedPrice
{
	public class AppliedFixedPriceDto
	{
		/// <summary>
		/// Список товаров с фиксой
		/// </summary>
		public IEnumerable<IOnlineOrderedProductWithFixedPrice> OnlineOrderItems { get; set; }
	}
}
