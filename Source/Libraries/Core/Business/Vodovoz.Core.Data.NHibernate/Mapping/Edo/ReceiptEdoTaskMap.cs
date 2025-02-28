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

			Map(x => x.ReceiptStatus)
				.Column("receipt_status");

			Map(x => x.CashboxId)
				.Column("cashbox_id");

			HasMany(x => x.FiscalDocuments)
				.KeyColumn("receipt_edo_task_id")
				.Cascade.AllDeleteOrphan();
		}
	}
}
