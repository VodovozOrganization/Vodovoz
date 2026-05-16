using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Nodes
{
	public class DiscountData : IDiscountData
	{
		/// <inheritdoc/>
		public bool IsDiscountInMoney { get; private set; }
		/// <inheritdoc/>
		public decimal Discount { get; private set; }
		/// <inheritdoc/>
		public DiscountReason DiscountReason { get; private set; }

		public static DiscountData Create(bool isDiscountInMoney, decimal discount, DiscountReason discountReason)
		{
			return new DiscountData
			{
				IsDiscountInMoney = isDiscountInMoney,
				Discount = discount,
				DiscountReason = discountReason
			};
		}
	}
}
