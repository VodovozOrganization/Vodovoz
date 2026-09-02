using System.Collections.Generic;
using CustomerOrdersApi.Library.V7.Dto.Orders.Promotions;
using Vodovoz.Core.Data.InfoMessages;
using Vodovoz.Core.Domain.Interfaces.Common;
using Vodovoz.Core.Domain.Interfaces.Sale;
using Vodovoz.Core.Domain.Results;

namespace CustomerOrdersApi.Library.V7.Dto.Orders
{
	/// <summary>
	/// Данные по применению промокода
	/// </summary>
	public class AppliedPromoCodeDto : SaleItemPromotionDto
	{
		public AppliedPromoCodeDto(string message) : base(message)
		{
		}

		public AppliedPromoCodeDto(IEnumerable<IOrderedCartItemWithDiscountDetails> saleItems, IInfoMessage warning = null)
			: base(saleItems, warning)
		{
		}

		public static ISaleItemPromotion CreateError(Error error) => new AppliedPromoCodeDto(error.Message);
		public static ISaleItemPromotion Create(IEnumerable<IOrderedCartItemWithDiscountDetails> saleItems, IInfoMessage warning = null) =>
			new AppliedPromoCodeDto(saleItems, warning);
	}
}
