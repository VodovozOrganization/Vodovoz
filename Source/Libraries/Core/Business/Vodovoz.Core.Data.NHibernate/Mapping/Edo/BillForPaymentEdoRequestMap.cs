using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class BillForPaymentEdoRequestMap : SubclassMap<BillForPaymentEdoRequest>
	{
		public BillForPaymentEdoRequestMap()
		{
			DiscriminatorValue(nameof(CustomerEdoRequestType.OrderWithoutShipmentForPayment));

			Map(x => x.OrderWithoutShipmentForPaymentId)
				.Column("bill_for_payment_id");
		}
	}
}
