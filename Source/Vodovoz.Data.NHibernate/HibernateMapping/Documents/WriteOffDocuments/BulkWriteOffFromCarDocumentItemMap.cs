using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.WriteOffDocuments;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents.WriteOffDocuments
{
	public class BulkWriteOffFromCarDocumentItemMap : SubclassMap<BulkWriteOffFromCarDocumentItem>
	{
		public BulkWriteOffFromCarDocumentItemMap()
		{
			DiscriminatorValue(nameof(WriteOffDocumentItemType.BulkWriteOffFromCarDocumentItem));
		}
	}
}
