using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.WriteOffDocuments;

namespace Vodovoz.HibernateMapping.Documents.WriteOffDocuments
{
	public class InstanceWriteOffDocumentItemMap : SubclassMap<InstanceWriteOffDocumentItem>
	{
		public InstanceWriteOffDocumentItemMap()
		{
			References(x => x.InventoryNomenclatureInstance).Column("nomenclature_instance_id");
		}
	}
}
