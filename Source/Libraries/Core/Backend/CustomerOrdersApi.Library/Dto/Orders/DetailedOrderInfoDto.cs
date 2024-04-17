using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Data.Orders;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.Dto.Orders
{
	/// <summary>
	/// Детальная информация о заказе
	/// </summary>
	public class DetailedOrderInfoDto : OrderDto
	{
		/// <summary>
		/// Быстрая доставка
		/// </summary>
		public bool IsFastDelivery { get; set; }
		/// <summary>
		/// Причины оценки
		/// </summary>
		public IEnumerable<int> RatingReasonsIds { get; private set; }
		/// <summary>
		/// Комментарий к оценке
		/// </summary>
		public string OrderRatingComment { get; set; }
		/// <summary>
		/// Товары
		/// </summary>
		public IEnumerable<OrderItemDto> OrderItems { get; private set; }

		public void UpdateOrderRating(OrderRating orderRating, DateTime ratingAvailableFrom)
		{
			if(orderRating is null)
			{
				IsRatingAvailable =
					CreationDate >= ratingAvailableFrom
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
		
		public void UpdateOrderItems(IEnumerable<OrderItem> orderItems)
		{
			OrderItems =
				orderItems.Select(orderItem =>
					OrderItemDto.Create(
						orderItem.Nomenclature.Id,
						orderItem.Count,
						orderItem.Price,
						orderItem.IsDiscountInMoney,
						orderItem.GetDiscount))
				.ToList();
		}
		
		public void UpdateOrderItems(IEnumerable<OnlineOrderItem> onlineOrderItems)
		{
			OrderItems =
				onlineOrderItems.Select(orderItem =>
					OrderItemDto.Create(
						orderItem.Nomenclature.Id,
						orderItem.Count,
						orderItem.Price,
						orderItem.IsDiscountInMoney,
						orderItem.GetDiscount))
				.ToList();
		}
	}

	public class OrderItemDto
	{
		private OrderItemDto(int nomenclatureId, decimal count, decimal price, bool isDiscountInMoney, decimal discount)
		{
			NomenclatureId = nomenclatureId;
			Count = count;
			Price = price;
			IsDiscountInMoney = isDiscountInMoney;
			Discount = discount;
		}
		
		/// <summary>
		/// Id номенклатуры
		/// </summary>
		public int NomenclatureId { get; }
		
		/// <summary>
		/// Количество
		/// </summary>
		public decimal Count { get; }
		
		/// <summary>
		/// Цена
		/// </summary>
		public decimal Price { get; }
		
		/// <summary>
		/// Скидка в деньгах?
		/// </summary>
		public bool IsDiscountInMoney { get; }

		/// <summary>
		/// Скидка
		/// </summary>
		public decimal Discount { get; }

		public static OrderItemDto Create(
			int nomenclatureId,
			decimal count,
			decimal price,
			bool isDiscountInMoney,
			decimal discount) =>
			new OrderItemDto(nomenclatureId, count, price, isDiscountInMoney, discount);
	}
}
