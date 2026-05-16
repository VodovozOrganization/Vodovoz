using System.Collections.Generic;
using System.Linq;
using CustomerOrders.Contracts.V5.Orders.OrderItem;
using CustomerOrders.Contracts.V5.Orders.PromoSets;
using VodovozBusiness.Domain.Orders;

namespace CustomerOrdersApi.Library.V5.Services
{
	internal sealed class CartItemsCheck
	{
		private readonly IList<IGoodsV5> _goods = new List<IGoodsV5>();

		private readonly IList<(CheckedOnlineOrderItemDto CheckedItem, IGoodsV5 Product)> _combinedItems =
			new List<(CheckedOnlineOrderItemDto CheckedItem, IGoodsV5 Product)>();
		
		private readonly IList<(CheckedPromoSetDto CheckedPromoSet, IGoodsV5 Product)> _combinedPromoSets =
			new List<(CheckedPromoSetDto CheckedPromoSet, IGoodsV5 Product)>();

		public IEnumerable<IGoodsV5> Goods => _goods;
		public IEnumerable<(CheckedOnlineOrderItemDto CheckedItem, IGoodsV5 Product)> CombinedItems => _combinedItems;
		public IEnumerable<(CheckedPromoSetDto CheckedPromoSet, IGoodsV5 Product)> CombinedPromoSets => _combinedPromoSets;
		
		public void AddItem(CheckedOnlineOrderItemDto checkedItem, IGoodsV5 product)
		{
			_combinedItems.Add((checkedItem, product));
			_goods.Add(product);
		}
		
		public void AddPromoSet(CheckedPromoSetDto checkedPromoSet, IGoodsV5 product)
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
					goodsV5.Discounts.Select(x => x.DiscountReason)
					);
				
				products.Add(product);
			}
			
			return products;
		}
	}
}
