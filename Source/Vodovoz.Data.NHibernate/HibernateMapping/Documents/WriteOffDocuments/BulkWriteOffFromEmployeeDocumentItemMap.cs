using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.WriteOffDocuments;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents.WriteOffDocuments
{
	public class BulkWriteOffFromEmployeeDocumentItemMap : SubclassMap<BulkWriteOffFromEmployeeDocumentItem>
	{
		public BulkWriteOffFromEmployeeDocumentItemMap()
		{
			DiscriminatorValue(nameof(WriteOffDocumentItemType.BulkWriteOffFromEmployeeDocumentItem));
		}
	}
}
