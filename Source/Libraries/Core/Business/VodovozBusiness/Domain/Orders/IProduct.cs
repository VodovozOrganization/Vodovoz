using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Orders
{
	public interface IProduct
	{
		int Id { get; }
		decimal Count { get; }
		decimal GetDiscount { get; }
		bool IsDiscountInMoney { get; }
		DiscountReason DiscountReason { get; set; }
		Nomenclature Nomenclature { get; }
		PromotionalSet PromoSet { get; set; }
		decimal ActualSum { get; }
		decimal CurrentCount { get; }
		decimal Price { get; }
	}
}
