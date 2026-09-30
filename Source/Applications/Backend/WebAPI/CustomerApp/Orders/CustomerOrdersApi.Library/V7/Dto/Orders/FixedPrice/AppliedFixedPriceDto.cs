using System.Collections.Generic;
using CustomerOrdersApi.Library.V7.Dto.Orders.Promotions;
using Vodovoz.Core.Domain.Interfaces.Common;
using Vodovoz.Core.Domain.Interfaces.Sale;
using Vodovoz.Core.Domain.Results;

namespace CustomerOrdersApi.Library.V7.Dto.Orders.FixedPrice
{
	/// <summary>
	/// Данные по применению фиксы
	/// </summary>
	public class AppliedFixedPriceDto : SaleItemPromotionDto
	{
		protected AppliedFixedPriceDto(string message) : base(message)
		{
		}

		protected AppliedFixedPriceDto(IEnumerable<IOrderedCartItemWithDiscountDetails> saleItems, IInfoMessage warning = null)
			: base(saleItems, warning)
		{
		}

		public static ISaleItemPromotion CreateError(Error error) => new AppliedFixedPriceDto(error.Message);
		public static ISaleItemPromotion Create(IEnumerable<IOrderedCartItemWithDiscountDetails> saleItems, IInfoMessage warning = null) =>
			new AppliedFixedPriceDto(saleItems, warning);
	}
}
