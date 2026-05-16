using System.Collections.Generic;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Orders;

namespace VodovozBusiness.Nodes
{
	/// <summary>
	/// Товар
	/// </summary>
	public class GoodsV5 : IGoodsV5
	{
		public GoodsV5(){ }

		protected GoodsV5(
			decimal price,
			decimal count,
			Nomenclature nomenclature,
			PromotionalSet promoSet,
			IEnumerable<IDiscountData> discounts,
			bool isFixedPrice)
		{
			Price = price;
			Count = count;
			Nomenclature = nomenclature;
			PromoSet = promoSet;
			Discounts = discounts;
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
		public IEnumerable<IDiscountData> Discounts { get; set; }
		
		public static GoodsV5 Create(
			decimal price,
			decimal count,
			Nomenclature nomenclature,
			PromotionalSet promoSet,
			IEnumerable<IDiscountData> discounts,
			bool isFixedPrice = false) =>
			new GoodsV5(price, count, nomenclature, promoSet, discounts, isFixedPrice);
	}
}
