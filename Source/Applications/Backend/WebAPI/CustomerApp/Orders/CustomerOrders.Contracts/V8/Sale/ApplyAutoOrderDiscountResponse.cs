using System.Collections.Generic;
using CustomerOrders.Abstractions.Common;
using CustomerOrders.Abstractions.V8.Sale;
using Vodovoz.Core.Domain.Results;

namespace CustomerOrders.Contracts.V8.Sale
{
	/// <summary>
	/// Данные по результату применения/снятия скидки за автозаказ
	/// </summary>
	public class ApplyAutoOrderDiscountResponse : SalePromotionDto
	{
		public ApplyAutoOrderDiscountResponse(string message) : base(message)
		{
		}

		public ApplyAutoOrderDiscountResponse(IEnumerable<IOrderedCartItemWithDiscountDetails> saleItems, IInfoMessage warning = null)
			: base(saleItems, warning)
		{
		}

		public static ISalePromotion CreateError(Error error) => new ApplyAutoOrderDiscountResponse(error.Message);
		public static ISalePromotion Create(IEnumerable<IOrderedCartItemWithDiscountDetails> saleItems, IInfoMessage warning = null) =>
			new ApplyAutoOrderDiscountResponse(saleItems, warning);
	}
}
