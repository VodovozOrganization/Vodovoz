using System.Collections.Generic;
using System.Linq;
using CustomerOrders.Contracts.V5.Orders.OrderItem;
using CustomerOrders.Contracts.V5.Orders.PromoSets;
using VodovozBusiness.Domain.Orders;

namespace CustomerOrdersApi.Library.V5.Services
{
	internal sealed class CartItemsCheck
	{
		private readonly IList<IGoodsWithManyDiscounts> _goods = new List<IGoodsWithManyDiscounts>();

		private readonly IList<(CheckedOnlineOrderItemDto CheckedItem, IGoodsWithManyDiscounts Product)> _combinedItems =
			new List<(CheckedOnlineOrderItemDto CheckedItem, IGoodsWithManyDiscounts Product)>();
		
		private readonly IList<(CheckedPromoSetDto CheckedPromoSet, IGoodsWithManyDiscounts Product)> _combinedPromoSets =
			new List<(CheckedPromoSetDto CheckedPromoSet, IGoodsWithManyDiscounts Product)>();

		public IEnumerable<IGoodsWithManyDiscounts> Goods => _goods;
		public IEnumerable<(CheckedOnlineOrderItemDto CheckedItem, IGoodsWithManyDiscounts Product)> CombinedItems => _combinedItems;
		public IEnumerable<(CheckedPromoSetDto CheckedPromoSet, IGoodsWithManyDiscounts Product)> CombinedPromoSets => _combinedPromoSets;
		
		public void AddItem(CheckedOnlineOrderItemDto checkedItem, IGoodsWithManyDiscounts product)
		{
			_combinedItems.Add((checkedItem, product));
			_goods.Add(product);
		}
		
		public void AddPromoSet(CheckedPromoSetDto checkedPromoSet, IGoodsWithManyDiscounts product)
		{
			_combinedPromoSets.Add((checkedPromoSet, product));
			_goods.Add(product);
		}

		public IEnumerable<IGoods> ToOldGoods()
		{
			var products = new List<IGoods>();

			foreach(var goodsV5 in _goods)
			{
				var product = VodovozBusiness.Nodes.Goods.Create(
					goodsV5.Price,
					goodsV5.Count,
					goodsV5.Nomenclature,
					goodsV5.PromoSet,
					goodsV5.Discounts.FirstOrDefault()?.DiscountReason
					);
				
				products.Add(product);
			}
			
			return products;
		}
	}
}
