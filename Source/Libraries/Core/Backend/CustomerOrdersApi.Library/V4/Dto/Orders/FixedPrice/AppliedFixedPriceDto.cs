using System.Collections.Generic;
using VodovozBusiness.Domain.Orders.V4;

namespace CustomerOrdersApi.Library.V4.Dto.Orders.FixedPrice
{
	public class AppliedFixedPriceDto
	{
		/// <summary>
		/// Список товаров с фиксой
		/// </summary>
		public IEnumerable<IOnlineOrderedProductWithFixedPriceV4> OnlineOrderItems { get; set; }
	}
}
