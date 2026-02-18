using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Orders.V5;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.V5.Dto.Orders
{
	/// <summary>
	/// Детальная информация о заказе
	/// </summary>
	public class DetailedOrderInfoDto : OrderDto
	{
		/// <summary>
		/// Значение таймера для оплаты заказа
		/// </summary>
		public int? TimerForPaySeconds { get; set; }
		
		/// <summary>
		/// Доступность повторения заказа
		/// </summary>
		public bool AvailableRepeatOrder { get; set; }
		
		/// <summary>
		/// Быстрая доставка
		/// </summary>
		public bool IsFastDelivery { get; set; }

		/// <summary>
		/// Источник онлайн оплаты
		/// </summary>
		public OnlinePaymentSource? OnlinePaymentSource { get; set; }
		
		/// <summary>
		/// Тип онлайн оплаты
		/// </summary>
		public OnlineOrderPaymentType? OnlinePaymentType { get; set; }
		
		/// <summary>
		/// Причины оценки
		/// </summary>
		public IEnumerable<int> RatingReasonsIds { get; private set; }
		
		/// <summary>
		/// Комментарий к оценке
		/// </summary>
		public string OrderRatingComment { get; set; }
		
		/// <summary>
		/// Товары без промонаборов
		/// </summary>
		public IEnumerable<OrderItemDto> OrderItems { get; private set; }
		
		/// <summary>
		/// Промонаборы
		/// </summary>
		public IEnumerable<PromoSetDto> PromoSets { get; private set; }

		public void UpdateOrderRating(OrderRating orderRating, DateTime ratingAvailableFrom)
		{
			if(orderRating is null)
			{
				IsRatingAvailable =
					CreatedDateTimeUtc >= DateTimeOffset.Parse(ratingAvailableFrom.ToString())
					&& (OrderStatus == ExternalOrderStatus.OrderCompleted
						|| OrderStatus == ExternalOrderStatus.Canceled
						|| OrderStatus == ExternalOrderStatus.OrderDelivering);
				RatingReasonsIds = new List<int>();
				return;
			}

			RatingReasonsIds = orderRating.OrderRatingReasons.Select(x => x.Id).ToList();
			OrderRatingComment = orderRating.Comment;
			RatingValue = orderRating.Rating;
			IsRatingAvailable = false;
		}
		
		public void UpdateOrderItems(IEnumerable<IProduct> orderItems)
		{
			OrderItems = orderItems
				.Where(x => x.PromoSet == null)
				.Select(orderItem =>
					OrderItemDto.Create(
						orderItem.Nomenclature.Id,
						orderItem.CurrentCount,
						orderItem.Price,
						orderItem.IsDiscountInMoney,
						orderItem.GetDiscount))
				.ToList();

			UpdatePromoSets(orderItems);
		}

		private void UpdatePromoSets(IEnumerable<IProduct> orderItems)
		{
			var promoSetsGroup = orderItems
				.Where(x => x.PromoSet != null)
				.ToLookup(x => x.PromoSet.Id);
			
			var promoSets = new List<PromoSetDto>();

			foreach(var orderItemGroup in promoSetsGroup)
			{
				var promo = orderItemGroup.First().PromoSet;
				var promoItemsCount = promo.PromotionalSetItems.Count;
				decimal promoPrice = 0m;
				var i = 0;

				foreach(var product in orderItemGroup)
				{
					i++;
					promoPrice += product.ActualSum;

					if(i >= promoItemsCount)
					{
						break;
					}
				}
					
				promoSets.Add(
					PromoSetDto.Create(
						orderItemGroup.Key,
						orderItemGroup.Count() / promoItemsCount,
						promoPrice
					));
			}

			PromoSets = promoSets;
		}
	}
}
