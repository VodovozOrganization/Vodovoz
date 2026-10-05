using System.Collections.Generic;
using CustomerOrders.Abstractions.Common;
using CustomerOrders.Abstractions.V8.Sale;
using CustomerOrders.Contracts.V8.Sale;
using CustomerOrdersApi.Library.V8.Dto.Orders.Promotions;
using Vodovoz.Core.Domain.Results;

namespace CustomerOrdersApi.Library.V8.Dto.Orders.FixedPrice
{
	/// <summary>
	/// Данные по применению фиксы
	/// </summary>
	public class AppliedFixedPriceDto : SalePromotionDto
	{
		protected AppliedFixedPriceDto(string message) : base(message)
		{
		}

		protected AppliedFixedPriceDto(IEnumerable<IOrderedCartItemWithDiscountDetails> saleItems, IInfoMessage warning = null)
			: base(saleItems, warning)
		{
		}

		public static ISalePromotion CreateError(Error error) => new AppliedFixedPriceDto(error.Message);
		public static ISalePromotion Create(IEnumerable<IOrderedCartItemWithDiscountDetails> saleItems, IInfoMessage warning = null) =>
			new AppliedFixedPriceDto(saleItems, warning);
	}
}
