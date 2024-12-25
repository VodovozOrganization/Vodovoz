using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class CustomerEdoTaskMap : SubclassMap<CustomerEdoTask>
	{
		public CustomerEdoTaskMap()
		{
			Abstract();

			HasOne(x => x.CustomerEdoRequest)
				.PropertyRef(nameof(CustomerEdoRequest.Task))
				.Cascade.All();

			HasMany(x => x.Items)
				.KeyColumn("customer_task_id")
				.Cascade.AllDeleteOrphan();
		}
	}
}
