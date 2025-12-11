using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class OrderEdoTaskMap : SubclassMap<OrderEdoTask>
	{
		public OrderEdoTaskMap()
		{
			Abstract();

			HasOne(x => x.OrderEdoRequest)
				.PropertyRef(nameof(FormalEdoRequest.Task))
				.Cascade.All();

			HasMany(x => x.Items)
				.KeyColumn("order_edo_task_id")
				.Cascade.AllDeleteOrphan()
				.Inverse();

			HasMany(x => x.TransferIterations)
				.KeyColumn("order_edo_task_id")
				.Cascade.AllDeleteOrphan();
		}
	}
}
