using System.Collections.Generic;
using CustomerOrdersApi.Library.V5.Dto.Orders.OrderItem;
using CustomerOrdersApi.Library.V5.Dto.Orders.PromoSets;
using VodovozBusiness.Domain.Orders;

namespace CustomerOrdersApi.Library.V5.Services
{
	internal sealed class CartItemsCheck
	{
		private readonly IList<IGoods> _goods = new List<IGoods>();

		private readonly IList<(CheckedOnlineOrderItemDto CheckedItem, IGoods Product)> _combinedItems =
			new List<(CheckedOnlineOrderItemDto CheckedItem, IGoods Product)>();
		
		private readonly IList<(CheckedPromoSetDto CheckedPromoSet, IGoods Product)> _combinedPromoSets =
			new List<(CheckedPromoSetDto CheckedPromoSet, IGoods Product)>();

		public IEnumerable<IGoods> Goods => _goods;
		public IEnumerable<(CheckedOnlineOrderItemDto CheckedItem, IGoods Product)> CombinedItems => _combinedItems;
		public IEnumerable<(CheckedPromoSetDto CheckedPromoSet, IGoods Product)> CombinedPromoSets => _combinedPromoSets;
		
		public void AddItem(CheckedOnlineOrderItemDto checkedItem, IGoods product)
		{
			_combinedItems.Add((checkedItem, product));
			_goods.Add(product);
		}
		
		public void AddPromoSet(CheckedPromoSetDto checkedPromoSet, IGoods product)
		{
			_combinedPromoSets.Add((checkedPromoSet, product));
			_goods.Add(product);
		}
	}
}
