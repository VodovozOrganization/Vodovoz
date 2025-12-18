using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Client
{
	// Для уровневого отображения цен поставщика
	public class SellingNomenclature : ISupplierPriceNode
	{
		public int Id { get; set; }
		public Nomenclature NomenclatureToBuy { get; set; }
		public SupplierPaymentType PaymentType { get; set; }
		public decimal Price { get; set; }

		public PaymentCondition PaymentCondition { get; set; }
		public DeliveryType DeliveryType { get; set; }
		public string Comment { get; set; }
		public AvailabilityForSale AvailabilityForSale { get; set; }
		public DateTime ChangingDate { get; set; }
		public Counterparty Supplier { get; set; }
		public bool IsEditable => false;
		public string PosNr { get; set; }

		public ISupplierPriceNode Parent { get; set; } = null;
		public IList<ISupplierPriceNode> Children { get; set; } = new List<ISupplierPriceNode>();
	}
}
