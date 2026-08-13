using System;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Sale
{
	public interface IAddingDiscountItem
	{
		DiscountReason DiscountReason { get; }
		Nomenclature Nomenclature { get; }
		PromotionalSet PromoSet { get; }
	}

	public class AddingDiscountItem
	{
		private AddingDiscountItem(DiscountReason addingReason, PromotionalSet promoSet)
		{
			AddingReason = addingReason ?? throw new ArgumentNullException(nameof(addingReason));
			Nomenclature = null;
			PromoSet = promoSet ?? throw new ArgumentNullException(nameof(promoSet));
		}
		
		private AddingDiscountItem(DiscountReason addingReason, Nomenclature nomenclature)
		{
			AddingReason = addingReason ?? throw new ArgumentNullException(nameof(addingReason));
			Nomenclature = nomenclature ?? throw new ArgumentNullException(nameof(nomenclature));
			PromoSet = null;
		}
		
		public DiscountReason AddingReason { get; private set; }
		public Nomenclature Nomenclature { get; private set; }
		public PromotionalSet PromoSet { get; private set; }

		public static AddingDiscountItem CreatePromoSetItem(DiscountReason addingReason, PromotionalSet promoSet) =>
			new AddingDiscountItem(addingReason, promoSet);
		
		public static AddingDiscountItem CreateNomenclatureItem(DiscountReason addingReason, Nomenclature nomenclature) =>
			new AddingDiscountItem(addingReason, nomenclature);
	}
}
