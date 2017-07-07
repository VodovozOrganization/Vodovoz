using FluentNHibernate.Mapping;
using Vodovoz.Domain.Documents;

namespace Vodovoz.HibernateMapping
{
	public class IncomingWaterMap : ClassMap<IncomingWater>
	{
		public IncomingWaterMap ()
		{
			Table ("store_incoming_water");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Amount).Column ("amount").Not.Nullable ();
			Map (x => x.TimeStamp).Column ("time_stamp").Not.Nullable ();
			Map(x => x.LastEditedTime).Column("last_edit_time");
			References (x => x.Author).Column ("author_id");
			References (x => x.LastEditor).Column ("last_editor_id");
			References (x => x.Product).Column ("product_nomenclature_id").Not.Nullable ();
			References (x => x.IncomingWarehouse).Column ("incoming_warehouse_id").Not.Nullable ();
			References (x => x.WriteOffWarehouse).Column ("writeoff_warehouse_id").Not.Nullable ();
			References (x => x.ProduceOperation).Column ("produce_operation_id").Cascade.All ().Not.Nullable ();
			HasMany (x => x.Materials).Cascade.AllDeleteOrphan ().Inverse ().KeyColumn ("incoming_water_id");
		}
	}
}