using System;
using System.Collections.Generic;
using System.Linq;
using CustomerOrdersApi.Library.V7.Dto.Orders.OrderItem;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace CustomerOrdersApi.Library.V7.Dto.Orders
{
	/// <summary>
	/// Детальная информация о заказе
	/// </summary>
	public class DetailedOrderInfoDto : ActiveOrderDto
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
		/// Доступность переноса даты/времени доставки
		/// </summary>
		public bool AvailableChangeDeliverySchedule { get; set; }

		/// <summary>
		/// Доступность отмены заказа
		/// </summary>
		public bool AvailableCancelOrder { get; set; }

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
		/// Номер телефона водителя в Mango
		/// </summary>
		public string DriversMangoNumber { get; set; }

		/// <summary>
		/// Товары/услуги заказа
		/// </summary>
		public IList<OnlineOrderItemDto> OrderItems { get; private set; }

		/// <summary>
		/// Обновление данных об оценке заказа
		/// </summary>
		/// <param name="orderRating">Оценка</param>
		/// <param name="ratingAvailableFrom">Дата с которой можно оценивать заказ</param>
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
		
		/// <summary>
		/// Добавление товаров/услуг
		/// </summary>
		/// <param name="order">Заказ, из которого добавляется информация</param>
		public void UpdateOrderItems(Order order)
		{
			OrderItems = order.OrderItems
				.Where(x => x.PromoSet is null)
				.Select(OnlineOrderItemDto.Create)
				.ToList();

			AddPromoSets(order.PromotionalSets);
		}

		/// <summary>
		/// Добавление товаров/услуг
		/// </summary>
		/// <param name="onlineOrder">Онлайн заказ, из которого добавляется информация</param>
		public void UpdateOrderItems(OnlineOrder onlineOrder)
		{
			OrderItems = onlineOrder.OnlineOrderItems
				.Where(x => x.PromoSet is null)
				.Select(OnlineOrderItemDto.Create)
				.ToList();

			if(onlineOrder is OnlineOrderV2 onlineOrderV2)
			{
				AddPromoSets(onlineOrderV2.PromoSets);
			}
			else
			{
				AddPromoSets(onlineOrder.OnlineOrderItems);
			}

			AddRentPackages(onlineOrder.OnlineRentPackages);
		}

		private void AddPromoSets(IEnumerable<PromotionalSet> promoSets)
		{
			var onlineOrderItemPromoSets = OnlineOrderItemDto.Create(promoSets);
			
			foreach(var promoSet in onlineOrderItemPromoSets)
			{
				OrderItems.Add(promoSet);
			}
		}
		
		private void AddPromoSets(IEnumerable<IProduct> orderItems)
		{
			var promoSetsGroup = orderItems
				.Where(x => x.PromoSet != null)
				.ToLookup(x => x.PromoSet.Id);
			
			foreach(var orderItemGroup in promoSetsGroup)
			{
				var promo = orderItemGroup.First().PromoSet;
				var promoItemsCount = promo.PromotionalSetItems.Count;
					
				OrderItems.Add(OnlineOrderItemDto.Create(promo, orderItemGroup.Count() / promoItemsCount));
			}
		}
		
		private void AddPromoSets(IEnumerable<OnlineOrderPromoSet> onlineOrderPromoSets)
		{
			foreach(var onlineOrderPromoSet in onlineOrderPromoSets)
			{
				OrderItems.Add(OnlineOrderItemDto.Create(onlineOrderPromoSet.PromoSet, onlineOrderPromoSet.Count));
			}
		}
		
		private void AddRentPackages(IEnumerable<OnlineFreeRentPackage> freeRentPackages)
		{
			foreach(var freeRentPackage in freeRentPackages)
			{
				OrderItems.Add(OnlineOrderItemDto.Create(freeRentPackage));
			}
		}
	}
}
