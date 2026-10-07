using System.Collections.Generic;

namespace CustomerOrdersApi.Library.V5.Dto.Orders.FixedPrice
{
	public class AppliedFixedPriceDto
	{
		/// <summary>
		/// Список товаров с фиксой
		/// </summary>
		public IEnumerable<OnlineOrderItemWithFixedPriceDto> OnlineOrderItems { get; set; }
	}
}
