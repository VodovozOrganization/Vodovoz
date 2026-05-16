using QS.Extensions.Observable.Collections.List;
using System.Collections.Generic;
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
			IEnumerable<DiscountReason> discountReasons,
			bool isFixedPrice)
		{
			Price = price;
			Count = count;
			Nomenclature = nomenclature;
			PromoSet = promoSet;
			DiscountReasons = new ObservableList<DiscountReason>(discountReasons);
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
		public IObservableList<DiscountReason> DiscountReasons { get; set; }
		
		public static Goods Create(
			decimal price,
			decimal count,
			Nomenclature nomenclature,
			PromotionalSet promoSet,
			IEnumerable<DiscountReason> discountReasons,
			bool isFixedPrice = false) =>
			new Goods(price, count, nomenclature, promoSet, discountReasons, isFixedPrice);
	}
}
