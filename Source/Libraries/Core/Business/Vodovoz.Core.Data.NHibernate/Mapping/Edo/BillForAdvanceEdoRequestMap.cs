using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class BillForAdvanceEdoRequestMap : SubclassMap<BillForAdvanceEdoRequest>
	{
		public BillForAdvanceEdoRequestMap()
		{
			DiscriminatorValue(nameof(CustomerEdoRequestType.OrderWithoutShipmentForAdvancePayment));

			Map(x => x.OrderWithoutShipmentForAdvancePaymentId)
				.Column("bill_for_advance_payment_id");
		}
	}
}
