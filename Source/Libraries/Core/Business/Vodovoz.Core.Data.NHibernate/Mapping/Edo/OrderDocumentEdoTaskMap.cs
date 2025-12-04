using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class OrderDocumentEdoTaskMap : SubclassMap<OrderDocumentEdoTask>
	{
		public OrderDocumentEdoTaskMap()
		{
			DiscriminatorValue(nameof(EdoTaskType.InformalOrderDocument));
		}
	}
}
