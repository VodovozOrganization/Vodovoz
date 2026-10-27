using System.Collections.Generic;
using Vodovoz.Core.Domain.Interfaces.Common;
using Vodovoz.Core.Domain.Interfaces.Sale;

namespace CustomerOrdersApi.Library.V7.Dto.Orders.Promotions
{
	/// <inheritdoc/>
	public abstract class SaleItemPromotionDto : ISaleItemPromotion
	{
		protected SaleItemPromotionDto(string message)
		{
			Ok = false;
			Message = message;
			SaleItems = null;
		}
		
		protected SaleItemPromotionDto(IEnumerable<IOrderedCartItemWithDiscountDetails> saleItems, IInfoMessage warning = null)
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
