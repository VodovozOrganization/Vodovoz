using System.Collections.Generic;
using CustomerOrdersApi.Library.V7.Dto.Orders.Promotions;
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

		public AppliedPromoCodeDto(IEnumerable<IOrderedCartItem> saleItems) : base(saleItems)
		{
		}

		public static ISaleItemPromotion CreateError(Error error) => new AppliedPromoCodeDto(error.Message);
		public static ISaleItemPromotion Create(IEnumerable<IOrderedCartItem> saleItems) => new AppliedPromoCodeDto(saleItems);
	}
}
