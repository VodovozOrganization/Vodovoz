using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.InventoryDocuments;

namespace Vodovoz.HibernateMapping.Documents.InventoryDocuments
{
	public class WarehouseBulkInventoryDocumentItemMap : SubclassMap<WarehouseBulkInventoryDocumentItem>
	{
		public WarehouseBulkInventoryDocumentItemMap()
		{
			DiscriminatorValue(nameof(InventoryDocumentType.WarehouseInventory));
		}
	}
}
