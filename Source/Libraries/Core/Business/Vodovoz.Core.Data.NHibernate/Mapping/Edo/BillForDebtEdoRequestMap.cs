using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class BillForDebtEdoRequestMap : SubclassMap<BillForDebtEdoRequest>
	{
		public BillForDebtEdoRequestMap()
		{
			DiscriminatorValue(nameof(CustomerEdoRequestType.OrderWithoutShipmentForDebt));

			Map(x => x.OrderWithoutShipmentForDebtId)
				.Column("bill_for_debt_id");
		}
	}
}
