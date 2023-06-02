using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Documents.MovementDocuments.BulkAccounting;

namespace Vodovoz.HibernateMapping.Documents.MovementDocuments.BulkAccounting
{
	public class BulkMovementDocumentFromWarehouseToEmployeeItemMap : SubclassMap<BulkMovementDocumentFromWarehouseToEmployeeItem>
	{
		public BulkMovementDocumentFromWarehouseToEmployeeItemMap()
		{
			DiscriminatorValue(nameof(MovementDocumentItemType.BulkMovementDocumentFromWarehouseToEmployeeItem));
		}
	}
}
