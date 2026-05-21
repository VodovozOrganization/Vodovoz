using System.Collections.Generic;
using VodovozBusiness.Domain.Orders;
using VodovozBusiness.Domain.Orders.V5;

namespace CustomerOrdersApi.Library.V5.Dto.Orders.FixedPrice
{
	public class AppliedFixedPriceDto
	{
		/// <summary>
		/// Список товаров с фиксой
		/// </summary>
		public IEnumerable<IOnlineOrderedProductWithFixedPriceV5> OnlineOrderItems { get; set; }
	}
}
