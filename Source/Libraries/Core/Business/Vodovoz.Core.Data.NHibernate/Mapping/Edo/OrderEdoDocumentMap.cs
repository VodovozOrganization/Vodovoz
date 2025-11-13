using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class OrderEdoDocumentMap : SubclassMap<OrderEdoDocument>
	{
		public OrderEdoDocumentMap()
		{
			DiscriminatorValue(nameof(OutgoingEdoDocumentType.Order));

			Map(x => x.DocumentTaskId)
				.Column("document_task_id");
		}
	}
}
