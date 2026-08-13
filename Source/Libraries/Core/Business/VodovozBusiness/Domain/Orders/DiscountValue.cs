using Vodovoz.Core.Domain.Interfaces;

namespace VodovozBusiness.Domain.Orders
{
	/// <inheritdoc/>
	public class DiscountValue : IDiscountValue
	{
		private DiscountValue(bool isDiscountMoney, decimal discount, decimal discountMoney)
		{
			IsDiscountMoney = isDiscountMoney;
			Discount = discount;
			DiscountMoney = discountMoney;
		}
		
		/// <inheritdoc/>
		public bool IsDiscountMoney { get; }
		/// <inheritdoc/>
		public decimal Discount { get; }
		/// <inheritdoc/>
		public decimal DiscountMoney { get; }

		public static IDiscountValue Create(bool isDiscountMoney, decimal discount, decimal discountMoney) =>
			new DiscountValue(isDiscountMoney, discount, discountMoney);
	}
}
