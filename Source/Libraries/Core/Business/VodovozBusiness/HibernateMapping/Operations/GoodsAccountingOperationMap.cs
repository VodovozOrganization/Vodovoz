using FluentNHibernate.Mapping;
using Vodovoz.Domain.Operations;

namespace Vodovoz.HibernateMapping
{
	//TODO поправтиь класс
	public class GoodsAccountingOperationMap : ClassMap<GoodsAccountingOperation>
	{
		public GoodsAccountingOperationMap()
		{
			Table("warehouse_movement_operations");
			DiscriminateSubClassesOnColumn("operation_type");
			
			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			
			Map(x => x.OperationTime).Column("operation_time").Not.Nullable();
			Map(x => x.Amount).Column("amount");
			Map(x => x.PrimeCost).Column("prime_cost");
			
			References(x => x.Nomenclature).Column("nomenclature_id").Not.Nullable();
			//References(x => x.Equipment).Column("equipment_id");
			//References(x => x.WriteOffWarehouse).Column("writeoff_warehouse_id");
			//References(x => x.IncomingWarehouse).Column("incoming_warehouse_id");
		}
	}
}

