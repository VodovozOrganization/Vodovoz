using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Interfaces.Sale;
using Vodovoz.Core.Domain.Sale;

namespace Vodovoz.Core.Data.Sale
{
	/// <summary>
	/// Товар онлайн заказа
	/// </summary>
	public class OnlineOrderItemWithDiscountDetailsDto : OnlineOrderItemBaseDto, IOrderedCartItemWithDiscountDetails
	{
		/// <summary>
		/// Идентификаторы скидок
		/// </summary>
		public IList<IDiscountAmount> Discounts { get; set; }
		/// <summary>
		/// Очистка скидки
		/// </summary>
		public void ClearDiscount()
		{
			Discounts.Clear();
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

		public static OnlineOrderItemWithDiscountDetailsDto Create(IOrderedCartItem onlineOrderedItem)
		{
			var discounts = new List<IDiscountAmount>();

			foreach(var discountId in onlineOrderedItem.DiscountIds)
			{
				discounts.Add(DiscountAmount.Create(discountId));
			}
			
			return new OnlineOrderItemWithDiscountDetailsDto
			{
				ErpId = onlineOrderedItem.ErpId,
				Count = onlineOrderedItem.Count,
				Price = onlineOrderedItem.Price,
				CurrentPrice = onlineOrderedItem.CurrentPrice,
				PriceWithoutDiscount = onlineOrderedItem.PriceWithoutDiscount,
				CurrentSum = onlineOrderedItem.CurrentSum,
				IsFixedPrice = onlineOrderedItem.IsFixedPrice,
				ItemType = onlineOrderedItem.ItemType,
				Discounts = discounts
			};
		}
	}
}
