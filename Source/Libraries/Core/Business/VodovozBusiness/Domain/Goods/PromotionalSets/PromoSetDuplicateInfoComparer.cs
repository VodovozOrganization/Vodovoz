using System.Collections.Generic;

namespace Vodovoz.Domain.Goods.PromotionalSets
{
	public class PromoSetDuplicateInfoComparer : IEqualityComparer<PromoSetDuplicateInfoNode>
	{
		public bool Equals(PromoSetDuplicateInfoNode x, PromoSetDuplicateInfoNode y)
		{
			if(ReferenceEquals(x, null))
			{
				return false;
			}

			if(ReferenceEquals(y, null))
			{
				return false;
			}

			return x.OrderId == y.OrderId;
		}

		public int GetHashCode(PromoSetDuplicateInfoNode obj)
		{
			return obj.OrderId.GetHashCode() + obj.Date.GetHashCode() +
					obj.Client.GetHashCode() + obj.Address.GetHashCode() + obj.Phone.GetHashCode();
		}
	}
}
