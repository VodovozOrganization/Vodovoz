using System.Collections.Generic;
using Vodovoz.Core.Domain.Interfaces.Sale;
using VodovozBusiness.Domain.Orders;

namespace CustomerOrdersApi.Library.V7.Dto.Orders.FixedPrice
{
	public class AppliedFixedPriceDto
	{
		/// <summary>
		/// Список товаров с фиксой
		/// </summary>
		public IEnumerable<IOrderedCartItem> OnlineOrderItems { get; set; }
	}
}
