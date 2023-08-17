using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.InventoryDocuments;

namespace Vodovoz.HibernateMapping.Documents.InventoryDocuments
{
	public class EmployeeBulkInventoryDocumentItemMap : SubclassMap<EmployeeBulkInventoryDocumentItem>
	{
		public EmployeeBulkInventoryDocumentItemMap()
		{
			DiscriminatorValue(nameof(InventoryDocumentType.EmployeeInventory));
		}
	}
}
