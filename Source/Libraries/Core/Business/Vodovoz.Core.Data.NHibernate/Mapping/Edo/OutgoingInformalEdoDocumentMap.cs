using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class OutgoingInformalEdoDocumentMap : SubclassMap<OutgoingInformalEdoDocument>
	{
		public OutgoingInformalEdoDocumentMap()
		{
			DiscriminatorValue(nameof(OutgoingEdoDocumentType.InformalOrderDocument));

			Map(x => x.InformalDocumentTaskId)
				.Column("informal_document_task_id");
		}
	}
}

