using System.Collections.Generic;
using CustomerOrders.Abstractions.Common;
using CustomerOrders.Abstractions.V8.Sale;

namespace CustomerOrders.Contracts.V8.Sale
{
	/// <inheritdoc/>
	public abstract class SalePromotionDto : ISalePromotion
	{
		protected SalePromotionDto(string message)
		{
			Ok = false;
			Message = message;
			SaleItems = null;
		}
		
		protected SalePromotionDto(IEnumerable<IOrderedCartItemWithDiscountDetails> saleItems, IInfoMessage warning = null)
		{
			Ok = true;
			Message = null;
			SaleItems = saleItems;
			Warning = warning;
		}
		
		/// <inheritdoc/>
		public bool Ok { get; set; }
		/// <inheritdoc/>
		public string Message { get; set; }
		/// <inheritdoc/>
		public IInfoMessage Warning { get; set; }
		/// <inheritdoc/>
		public IEnumerable<IOrderedCartItemWithDiscountDetails> SaleItems { get; set; }
	}
}
