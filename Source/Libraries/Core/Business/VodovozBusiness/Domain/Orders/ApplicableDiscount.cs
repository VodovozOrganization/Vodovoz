using System.Collections.Generic;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace VodovozBusiness.Domain.Orders
{
	public class ApplicableDiscount : IApplicableDiscount
	{
		public decimal Price { get; set; }
		public decimal Count { get; set; }
		public decimal CurrentRawPrice => Count * Price;
		public bool IsFixedPrice { get; set; }
		public Nomenclature Nomenclature { get; set; }
		public PromotionalSet PromoSet { get; set; }
		public IEnumerable<DiscountReasonBase> DiscountReasons { get; set; }
	}
}
