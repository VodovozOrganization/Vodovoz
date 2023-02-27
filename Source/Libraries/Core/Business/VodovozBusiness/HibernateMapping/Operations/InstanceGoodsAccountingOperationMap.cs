using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.HibernateMapping.Operations
{
	public class InstanceGoodsAccountingOperationMap : SubclassMap<InstanceGoodsAccountingOperation>
	{
		public InstanceGoodsAccountingOperationMap()
		{
			DiscriminatorValue(nameof(GoodsAccountingOperationType.InstanceGoodsAccountingOperation));
			References(x => x.InventoryNomenclatureInstance).Column("nomenclature_instance_id");
		}
	}
}
