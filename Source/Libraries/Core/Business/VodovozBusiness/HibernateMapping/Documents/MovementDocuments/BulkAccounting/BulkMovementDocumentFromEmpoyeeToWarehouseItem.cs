using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Documents.MovementDocuments.BulkAccounting;

namespace Vodovoz.HibernateMapping.Documents.MovementDocuments.BulkAccounting
{
	public class BulkMovementDocumentFromEmployeeToWarehouseItemMap : SubclassMap<BulkMovementDocumentFromEmployeeToWarehouseItem>
	{
		public BulkMovementDocumentFromEmployeeToWarehouseItemMap()
		{
			DiscriminatorValue(nameof(MovementDocumentItemType.BulkMovementDocumentFromEmployeeToWarehouseItem));
		}
	}
}
