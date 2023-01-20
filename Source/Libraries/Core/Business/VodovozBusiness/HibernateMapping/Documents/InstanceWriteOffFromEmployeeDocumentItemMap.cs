using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping.Documents
{
	public class InstanceWriteOffFromEmployeeDocumentItemMap : SubclassMap<InstanceWriteOffFromEmployeeDocumentItem>
	{
		public InstanceWriteOffFromEmployeeDocumentItemMap()
		{
			DiscriminatorValue(nameof(AccountingType.Instance));
		}
	}
}
