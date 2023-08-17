using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents.MovementDocuments.InstanceAccounting;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents.MovementDocuments.InstanceAccounting
{
	public class InstanceMovementDocumentItemMap : SubclassMap<InstanceMovementDocumentItem>
	{
		public InstanceMovementDocumentItemMap()
		{
			References(x => x.InventoryNomenclatureInstance).Column("nomenclature_instance_id");
		}
	}
}
