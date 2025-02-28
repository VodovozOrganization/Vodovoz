using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class BulkAccountingEdoTaskMap : SubclassMap<BulkAccountingEdoTask>
	{
		public BulkAccountingEdoTaskMap()
		{
			DiscriminatorValue(nameof(EdoTaskType.BulkAccounting));

			Extends(typeof(OrderEdoTask));
		}
	}
}
