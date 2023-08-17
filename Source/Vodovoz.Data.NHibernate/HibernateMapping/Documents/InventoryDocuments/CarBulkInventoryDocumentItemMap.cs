using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.InventoryDocuments;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents.InventoryDocuments
{
	public class CarBulkInventoryDocumentItemMap : SubclassMap<CarBulkInventoryDocumentItem>
	{
		public CarBulkInventoryDocumentItemMap()
		{
			DiscriminatorValue(nameof(InventoryDocumentType.CarInventory));
		}
	}
}
