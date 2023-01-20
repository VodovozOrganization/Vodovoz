using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping.Documents
{
	public class BulkWriteOffDocumentItemMap : SubclassMap<BulkWriteOffDocumentItem>
	{
		public BulkWriteOffDocumentItemMap()
		{
			DiscriminatorValue(nameof(AccountingType.Bulk));
		}
	}
}
