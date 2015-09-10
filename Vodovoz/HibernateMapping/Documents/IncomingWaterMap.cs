using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HMap
{
	public class IncomingWaterMap : ClassMap<IncomingWater>
	{
		public IncomingWaterMap ()
		{
			Table ("incoming_water");
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Amount).Column ("amount").Not.Nullable ();
			Map (x => x.TimeStamp).Column ("time_stamp").Not.Nullable ();
			References (x => x.Product).Column ("product_nomenclature_id").Not.Nullable ();
			References (x => x.IncomingWarehouse).Column ("incoming_warehouse_id").Not.Nullable ();
			References (x => x.WriteOffWarehouse).Column ("writeoff_warehouse_id").Not.Nullable ();
			References (x => x.ProduceOperation).Column ("produce_operation_id").Cascade.All ().Not.Nullable ();
			HasMany (x => x.Materials).Cascade.AllDeleteOrphan ().Inverse ().KeyColumn ("incoming_water_id");
		}
	}
}