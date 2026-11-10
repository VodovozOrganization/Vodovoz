using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Sale;
using Vodovoz.Core.Domain.Interfaces.Sale;

namespace CustomerOrdersApi.Library.V7.Dto.Orders.OrderItem
{
	/// <summary>
	/// Товар онлайн заказа
	/// </summary>
	public class OnlineOrderItemDto : OnlineOrderItemBaseDto, IOrderedCartItem
	{
		/// <inheritdoc/>
		public IEnumerable<int> DiscountIds { get; set; }
	}
}
