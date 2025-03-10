using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class DocumentEdoTaskMap : SubclassMap<DocumentEdoTask>
	{
		public DocumentEdoTaskMap()
		{
			DiscriminatorValue(nameof(EdoTaskType.Document));

			Extends(typeof(OrderEdoTask));
				
			Map(x => x.FromOrganization)
				.Column("from_organization_id");

			Map(x => x.ToCustomer)
				.Column("to_customer_id");

			Map(x => x.DocumentType)
				.Column("document_type");

			Map(x => x.Stage)
				.Column("document_task_stage");

			HasMany(x => x.UpdInventPositions)
				.KeyColumn("document_edo_task_id")
				.Cascade.AllDeleteOrphan();
		}
	}
}
