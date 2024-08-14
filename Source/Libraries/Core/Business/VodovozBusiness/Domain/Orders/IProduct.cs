using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Orders
{
	public interface IProduct
	{
		decimal Count { get; }
		DiscountReason DiscountReason { get; set; }
		Nomenclature Nomenclature { get; }
		PromotionalSet PromoSet { get; set; }
		decimal ActualSum { get; }
		decimal CurrentCount { get; }
		decimal Price { get; }
		bool IsDiscountInMoney { get; }
		decimal GetDiscount { get; }
	}
}
