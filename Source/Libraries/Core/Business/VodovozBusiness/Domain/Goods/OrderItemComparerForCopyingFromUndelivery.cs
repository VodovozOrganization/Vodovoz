using System;
using System.Collections.Generic;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Goods
{
	public class OrderItemComparerForCopyingFromUndelivery : IEqualityComparer<OrderItem>
	{
		public bool Equals(OrderItem x, OrderItem y)
		{
			if(Object.ReferenceEquals(x, y))
			{
				return true;
			}

			if(Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
			{
				return false;
			}
			
			return x.Nomenclature.Id == y.Nomenclature.Id;
		}

		public int GetHashCode(OrderItem orderItem)
		{
			if(Object.ReferenceEquals(orderItem, null))
			{
				return 0;
			}
			
			return orderItem.Nomenclature.Id.GetHashCode();
		}
	}
}
