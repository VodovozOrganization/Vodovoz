using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Operations
{
	public class InstanceGoodsAccountingOperationMap : SubclassMap<InstanceGoodsAccountingOperation>
	{
		public InstanceGoodsAccountingOperationMap()
		{
			References(x => x.InventoryNomenclatureInstance).Column("nomenclature_instance_id");
		}
	}
}
