using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class ReceiptEdoTaskMap : SubclassMap<ReceiptEdoTask>
	{
		public ReceiptEdoTaskMap()
		{
			DiscriminatorValue(nameof(EdoTaskType.Receipt));

			Extends(typeof(OrderEdoTask));

			//Map(x => x.CashReceiptId)
			//	.Column("cash_receipt_id");
		}
	}
}
