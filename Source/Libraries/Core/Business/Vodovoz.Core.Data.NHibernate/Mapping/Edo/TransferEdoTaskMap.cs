using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class TransferEdoTaskMap : SubclassMap<TransferEdoTask>
	{
		public TransferEdoTaskMap()
		{
			DiscriminatorValue(nameof(EdoTaskType.Transfer));

			Map(x => x.FromOrganizationId)
				.Column("from_organization_id");

			Map(x => x.ToOrganizationId)
				.Column("to_organization_id");

			Map(x => x.TransferStatus)
				.Column("transfer_status");

			Map(x => x.TransferStartTime)
				.Column("transfer_start_time");

			Map(x => x.TransferOrderId)
				.Column("transfer_order_id");

			HasMany(x => x.TransferEdoRequests)
				.KeyColumn("transfer_edo_task_id")
				.Cascade.AllDeleteOrphan()
				.LazyLoad()
				.Inverse();
		}
	}
}
