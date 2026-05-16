using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Nodes
{
	/// <summary>
	/// Товар
	/// </summary>
	public class Goods : IGoods
	{
		public Goods(){ }

		protected Goods(
			decimal price,
			decimal count,
			Nomenclature nomenclature,
			PromotionalSet promoSet,
			DiscountReason discountReason,
			bool isFixedPrice)
		{
			Price = price;
			Count = count;
			Nomenclature = nomenclature;
			PromoSet = promoSet;
			DiscountReason = discountReason;
			IsFixedPrice = isFixedPrice;
		}
		
		/// <inheritdoc/>
		public decimal Price { get; }
		/// <inheritdoc/>
		public decimal Count { get; set; }
		/// <inheritdoc/>
		public bool IsFixedPrice { get; set; }
		/// <inheritdoc/>
		public Nomenclature Nomenclature { get; set; }
		/// <inheritdoc/>
		public PromotionalSet PromoSet { get; set; }
		/// <inheritdoc/>
		public DiscountReason DiscountReason { get; set; }
		
		public static Goods Create(
			decimal price,
			decimal count,
			Nomenclature nomenclature,
			PromotionalSet promoSet,
			DiscountReason discountReason,
			bool isFixedPrice = false) =>
			new Goods(price, count, nomenclature, promoSet, discountReason, isFixedPrice);
	}
}
