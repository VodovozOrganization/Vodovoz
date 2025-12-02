using System;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Client
{
	// Для уровневого отображения цен поставщика
	public interface ISupplierPriceNode
	{
		int Id { get; set; }
		Nomenclature NomenclatureToBuy { get; set; }
		SupplierPaymentType PaymentType { get; set; }
		decimal Price { get; set; }
		VatRate VatRate { get; set; }
		PaymentCondition PaymentCondition { get; set; }
		DeliveryType DeliveryType { get; set; }
		string Comment { get; set; }
		AvailabilityForSale AvailabilityForSale { get; set; }
		DateTime ChangingDate { get; set; }
		Counterparty Supplier { get; set; }
		bool IsEditable { get; }
		string PosNr { get; set; }

		ISupplierPriceNode Parent { get; set; }
		IList<ISupplierPriceNode> Children { get; set; }
	}
}
