using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Documents.MovementDocuments.BulkAccounting;

namespace Vodovoz.HibernateMapping.Documents.MovementDocuments.BulkAccounting
{
	public class BulkMovementDocumentFromCarToCarItemMap : SubclassMap<BulkMovementDocumentFromCarToCarItem>
	{
		public BulkMovementDocumentFromCarToCarItemMap()
		{
			DiscriminatorValue(nameof(MovementDocumentItemType.BulkMovementDocumentFromCarToCarItem));
		}
	}
}
