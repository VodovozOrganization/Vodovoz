using System.Collections.Generic;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Nodes
{
	public class CalculatingTotalMoneyDiscountNode : ICalculatingTotalMoneyDiscount
	{
		public decimal CurrentRawPrice { get; private set; }
		public IEnumerable<DiscountReason> DiscountReasons { get; private set; }

		public static ICalculatingTotalMoneyDiscount Create(
			decimal currentRawPrice,
			IEnumerable<DiscountReason> discountReasons
		) => new CalculatingTotalMoneyDiscountNode
		{
			CurrentRawPrice = currentRawPrice,
			DiscountReasons = discountReasons
		};
	}
}
