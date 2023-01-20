using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping.Documents
{
	public class BulkWriteOffFromEmployeeDocumentItemMap : SubclassMap<BulkWriteOffFromEmployeeDocumentItem>
	{
		public BulkWriteOffFromEmployeeDocumentItemMap()
		{
			DiscriminatorValue(nameof(AccountingType.Bulk));
		}
	}
}
