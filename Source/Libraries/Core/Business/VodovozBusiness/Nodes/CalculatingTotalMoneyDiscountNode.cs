using System.Collections.Generic;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Nodes
{
	/// <inheritdoc/>
	public class CalculatingTotalMoneyDiscountNode : ICalculatingTotalMoneyDiscount
	{
		/// <inheritdoc/>
		public decimal CurrentRawPrice { get; private set; }
		/// <inheritdoc/>
		public IEnumerable<DiscountReasonBase> DiscountReasons { get; private set; }
		public PersonalDiscount PersonalDiscount { get; private set; }

		public static ICalculatingTotalMoneyDiscount Create(
			decimal currentRawPrice,
			IEnumerable<DiscountReasonBase> discountReasons,
			PersonalDiscount personalDiscount = null
		) => new CalculatingTotalMoneyDiscountNode
		{
			CurrentRawPrice = currentRawPrice,
			DiscountReasons = discountReasons,
			PersonalDiscount = personalDiscount
		};
	}
}
