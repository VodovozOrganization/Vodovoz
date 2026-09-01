using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Sale;
using Vodovoz.Core.Domain.Extensions;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Interfaces.Sale;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.V7.Dto.Orders.OrderItem
{
	/// <summary>
	/// Товар онлайн заказа
	/// </summary>
	public class OnlineOrderItemDto : OnlineOrderItemBaseDto, IOrderedCartItem
	{
		/// <summary>
		/// Идентификаторы скидок
		/// </summary>
		public IEnumerable<int> DiscountIds { get; set; }
		/// <summary>
		/// Очистка скидки
		/// </summary>
		public void ClearDiscount()
		{
			DiscountIds = Enumerable.Empty<int>();
		}
		/// <summary>
		/// Добавление фиксы
		/// </summary>
		/// <param name="fixedPrice">Фикса</param>
		public override void AddFixedPrice(decimal fixedPrice)
		{
			base.AddFixedPrice(fixedPrice);
			ClearDiscount();
		}
	}
}
