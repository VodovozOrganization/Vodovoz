using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping.Documents
{
	public class BulkWriteOffFromWarehouseDocumentItemMap : SubclassMap<BulkWriteOffFromWarehouseDocumentItem>
	{
		public BulkWriteOffFromWarehouseDocumentItemMap()
		{
			DiscriminatorValue(nameof(AccountingType.Bulk));
		}
	}
}
