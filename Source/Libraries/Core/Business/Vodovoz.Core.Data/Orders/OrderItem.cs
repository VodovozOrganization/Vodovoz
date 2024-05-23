using System;
using Vodovoz.Core.Data.Goods;

namespace Vodovoz.Core.Data.Orders
{
	public class OrderItem
	{
		public int Id { get; set; }
		public int OrderId { get; set; }
		public Nomenclature Nomenclature { get; set; }
		public decimal Count { get; set; }
		public decimal Price { get; set; }
		public decimal? ActualCount { get; set; }
		public decimal? IncludeNDS { get; set; }
		public decimal? ValueAddedTax { get; set; }
		public virtual decimal DiscountMoney { get; set; }
		
		public virtual decimal CurrentNDS => IncludeNDS ?? 0;
		public decimal CurrentCount => ActualCount ?? Count;
		public decimal ActualSum => Math.Round(Price * CurrentCount - DiscountMoney, 2);
		public decimal SumWithoutVat => Math.Round(Price * CurrentCount - CurrentNDS - DiscountMoney, 2);
		public decimal PriceWithoutVat => Math.Round((Price * CurrentCount - CurrentNDS - DiscountMoney) / CurrentCount, 2);
	}
}
