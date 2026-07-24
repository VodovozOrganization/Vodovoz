using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Extensions;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Interfaces.Sale;
using Vodovoz.Domain.Orders;

namespace CustomerOrdersApi.Library.V7.Dto.Orders.OrderItem
{
	/// <summary>
	/// Товар онлайн заказа
	/// </summary>
	public class OnlineOrderItemDto : IOrderedCartItem
	{
		/// <summary>
		/// Id заказываемой позиции в ДВ
		/// </summary>
		public int ErpId { get; set; }
		/// <summary>
		/// Тип заказываемой позиции
		/// </summary>
		public SaleItemType ItemType { get; set; }
		/// <summary>
		/// Цена
		/// </summary>
		public decimal Price { get; set; }
		/// <summary>
		/// Цена со скидкой
		/// </summary>
		public decimal CurrentPrice { get; set; }
		/// <summary>
		/// Цена без скидки
		/// </summary>
		public decimal? PriceWithoutDiscount { get; set; }
		/// <summary>
		/// Сумма со скидкой
		/// </summary>
		public decimal CurrentSum { get; set; }
		/// <summary>
		/// Количество
		/// </summary>
		public decimal Count { get; set; }
		/// <summary>
		/// Фикса
		/// </summary>
		public bool IsFixedPrice { get; set; }
		/// <summary>
		/// Идентификаторы скидок
		/// </summary>
		public IList<int> DiscountIds { get; set; }
		/// <summary>
		/// Очистка скидки
		/// </summary>
		public void ClearDiscount()
		{
			DiscountIds.Clear();
		}
		/// <summary>
		/// Добавление фиксы
		/// </summary>
		/// <param name="fixedPrice">Фикса</param>
		public void AddFixedPrice(decimal fixedPrice)
		{
			PriceWithoutDiscount ??= Price;
			Price = fixedPrice;
			CurrentPrice = fixedPrice;
			CurrentSum = Math.Round(CurrentPrice * Count, 2);
			IsFixedPrice = true;
			
			ClearDiscount();
		}
		
		public static OnlineOrderItemDto Create(IProduct product)
		{
			return new OnlineOrderItemDto
			{
				ErpId = product.Id,
				Count = product.Count,
				Price = product.Price,
				CurrentPrice = Math.Round(product.ActualSum /  product.Count, 2),
				PriceWithoutDiscount = null,
				CurrentSum = product.ActualSum,
				IsFixedPrice = product.IsFixedPrice,
				ItemType = product.Nomenclature.Category.ToSaleItemType(),
				DiscountIds = product.DiscountReasons.Select(x => x.Id).ToList()
			};
		}
		
		public static IEnumerable<OnlineOrderItemDto> Create(IEnumerable<PromotionalSet> promoSets)
		{
			var promoSetsLookup = promoSets.ToLookup(x => x.Id);

			return promoSetsLookup
				.Select(groupedPromoSets => Create(groupedPromoSets.First(), groupedPromoSets.Count()))
				.ToList();
		}
		
		public static OnlineOrderItemDto Create(PromotionalSet promoSet, decimal count)
		{
			var currentPrice = promoSet.Sum();
			var priceWithoutDiscount = promoSet.SumWithoutDiscount();
			var currentSum = Math.Round(promoSet.Sum() * count, 2);
			
			return new OnlineOrderItemDto
			{
				ErpId = promoSet.Id,
				Count = count,
				Price = currentPrice,
				CurrentPrice = currentPrice,
				PriceWithoutDiscount = priceWithoutDiscount,
				CurrentSum = currentSum,
				IsFixedPrice = false,
				ItemType = SaleItemType.PromoSet,
				DiscountIds = new List<int>()
			};
		}
		
		public static OnlineOrderItemDto Create(OnlineFreeRentPackage freeRentPackage)
		{
			return new OnlineOrderItemDto
			{
				ErpId = freeRentPackage.Id,
				Count = freeRentPackage.Count,
				Price = freeRentPackage.Price,
				CurrentPrice = freeRentPackage.Price,
				PriceWithoutDiscount = null,
				CurrentSum = freeRentPackage.Sum,
				IsFixedPrice = false,
				ItemType = SaleItemType.RentPackage,
				DiscountIds = new List<int>()
			};
		}
	}
}
