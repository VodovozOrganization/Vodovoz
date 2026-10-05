using System.Collections.Generic;
using CustomerOrders.Abstractions.Common;
using CustomerOrders.Abstractions.V8.Sale;
using CustomerOrders.Contracts.V8.Sale;
using Vodovoz.Core.Domain.Results;

namespace CustomerOrdersApi.Library.V8.Dto.Orders.Promotions.Discounts
{
	/// <summary>
	/// Данные по применению промокода
	/// </summary>
	public class AppliedPromoCodeDto : SalePromotionDto
	{
		public AppliedPromoCodeDto(string message) : base(message)
		{
		}

		public AppliedPromoCodeDto(IEnumerable<IOrderedCartItemWithDiscountDetails> saleItems, IInfoMessage warning = null)
			: base(saleItems, warning)
		{
		}

		public static ISalePromotion CreateError(Error error) => new AppliedPromoCodeDto(error.Message);
		public static ISalePromotion Create(IEnumerable<IOrderedCartItemWithDiscountDetails> saleItems, IInfoMessage warning = null) =>
			new AppliedPromoCodeDto(saleItems, warning);
	}
}
