using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping.Documents
{
	public class InstanceWriteOffFromWarehouseDocumentItemMap : SubclassMap<InstanceWriteOffFromWarehouseDocumentItem>
	{
		public InstanceWriteOffFromWarehouseDocumentItemMap()
		{
			DiscriminatorValue(nameof(AccountingType.Instance));
		}
	}
}
