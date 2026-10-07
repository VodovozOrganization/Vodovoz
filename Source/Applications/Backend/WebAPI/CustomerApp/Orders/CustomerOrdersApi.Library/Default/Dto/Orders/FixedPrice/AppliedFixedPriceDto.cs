using System.Collections.Generic;
using VodovozBusiness.Domain.Orders;

namespace CustomerOrdersApi.Library.Default.Dto.Orders.FixedPrice
{
	public class AppliedFixedPriceDto
	{
		/// <summary>
		/// Список товаров с фиксой
		/// </summary>
		public IEnumerable<OnlineOrderItemWithFixedPriceDto> OnlineOrderItems { get; set; }
	}
}
