using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.WriteOffDocuments;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents.WriteOffDocuments
{
	public class BulkWriteOffFromWarehouseDocumentItemMap : SubclassMap<BulkWriteOffFromWarehouseDocumentItem>
	{
		public BulkWriteOffFromWarehouseDocumentItemMap()
		{
			DiscriminatorValue(nameof(WriteOffDocumentItemType.BulkWriteOffFromWarehouseDocumentItem));
		}
	}
}
