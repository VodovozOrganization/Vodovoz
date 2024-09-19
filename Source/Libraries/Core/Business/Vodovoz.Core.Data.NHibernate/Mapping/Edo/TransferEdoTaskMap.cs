using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.TrueMark;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class TransferEdoTaskMap : SubclassMap<TransferEdoTask>
	{
		public TransferEdoTaskMap()
		{
			DiscriminatorValue(nameof(EdoTaskType.Transfer));

			Map(x => x.OrderEdoTaskId)
				.Column("order_edo_task_id");

			Map(x => x.FromOrganizationId)
				.Column("from_organization_id");

			Map(x => x.ToOrganizationId)
				.Column("to_organization_id");
		}
	}
}
