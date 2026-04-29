using System.Text.Json.Serialization;

namespace CustomerOrdersApi.Library.V5.Dto.Orders.PromoSets
{
	/// <summary>
	/// Проверенный промо набор онлайн заказа из корзины
	/// </summary>
	public class CheckedPromoSetDto : OrderingPromoSetDto
	{
		/// <summary>
		/// Статус товара <see cref="CartItemStatus"/>
		/// </summary>
		public CartItemStatus Status { get; set; }
		/// <summary>
		/// Промо набор для новых клиентов
		/// </summary>
		[JsonIgnore]
		public bool PromotionalSetForNewClients { get; set; }

		public static CheckedPromoSetDto Create(OrderingPromoSetDto orderingSet, CartItemStatus status = CartItemStatus.Active)
		{
			return new CheckedPromoSetDto
			{
				PromoSetId = orderingSet.PromoSetId,
				Count = orderingSet.Count,
				Price = orderingSet.Price,
				PriceWithoutDiscount = orderingSet.PriceWithoutDiscount,
				Status = status
			};
		}
	}
}
