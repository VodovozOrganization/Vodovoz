using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class TenderEdoTaskMap : SubclassMap<TenderEdoTask>
	{
		public TenderEdoTaskMap()
		{
			DiscriminatorValue(nameof(EdoTaskType.Tender));
			Extends(typeof(OrderEdoTask));

			Map(x => x.Stage)
				.Column("tender_task_stage");

			HasMany(x => x.UpdInventPositions)
				.KeyColumn("document_edo_task_id")
				.Cascade.AllDeleteOrphan();
		}
	}
}
