using System.Collections.Generic;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Orders
{
	public interface IPreserveDiscount : IApplyDiscountReasonItem
	{
		decimal? OriginalDiscountMoney { get; set; }
		decimal? OriginalDiscount { get; set; }
		IList<DiscountReason> OriginalDiscountReasons { get; }
	}
}
