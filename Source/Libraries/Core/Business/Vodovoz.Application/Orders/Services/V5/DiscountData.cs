using Vodovoz.Core.Data.Orders.V5;

namespace Vodovoz.Application.Orders.Services.V5
{
	public class DiscountData : IDiscountData
	{
		/// <inheritdoc/>
		public decimal Discount { get; set; }
		/// <inheritdoc/>
		public bool IsDiscountInMoney { get; set; }
		/// <inheritdoc/>
		public int? DiscountReasonId { get; set; }
		
		public static DiscountData Create(
			bool isDiscountInMoney,
			int discountReasonId) => new DiscountData
		{
			IsDiscountInMoney = isDiscountInMoney,
			DiscountReasonId = discountReasonId
		};
	}
}
