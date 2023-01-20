using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping.Documents
{
	public class BulkWriteOffFromCarDocumentItemMap : SubclassMap<BulkWriteOffFromCarDocumentItem>
	{
		public BulkWriteOffFromCarDocumentItemMap()
		{
			DiscriminatorValue(nameof(AccountingType.Bulk));
		}
	}
}
