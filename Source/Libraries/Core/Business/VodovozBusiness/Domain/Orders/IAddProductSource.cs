using System.Collections.Generic;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Domain.Sale;

namespace VodovozBusiness.Domain.Orders
{
	public interface IAddProductSource
	{
		object Source { get; }
		Counterparty Counterparty { get; }
		DeliveryPoint DeliveryPoint { get; }
		PaymentTypeSource PaymentTypeSource { get; }
		bool IsSelfDelivery { get; }
		bool IsLoadedFrom1C { get; }
		bool HasDepositItems { get; }
		bool HasNonPaidDeliveryItems { get; }
		ICollection<IProduct> Products { get; }
		ICollection<IPromoSet> PromoSets { get; }
	}
}
