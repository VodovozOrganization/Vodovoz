using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Documents.MovementDocuments.BulkAccounting;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents.MovementDocuments.BulkAccounting
{
	public class BulkMovementDocumentFromWarehouseToCarItemMap : SubclassMap<BulkMovementDocumentFromWarehouseToCarItem>
	{
		public BulkMovementDocumentFromWarehouseToCarItemMap()
		{
			DiscriminatorValue(nameof(MovementDocumentItemType.BulkMovementDocumentFromWarehouseToCarItem));
		}
	}
}
