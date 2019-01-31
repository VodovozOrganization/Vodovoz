using FluentNHibernate.Mapping;
using Vodovoz.Domain.Store;

namespace Vodovoz.HibernateMapping
{
	public class WarehouseMap : ClassMap<Warehouse>
	{
		public WarehouseMap ()
		{
			Table ("warehouses");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Name).Column ("name");
			Map (x => x.CanReceiveBottles).Column ("can_receive_bottles");
			Map (x => x.CanReceiveEquipment).Column ("can_receive_equipment");
			Map(x => x.PublishOnlineStore).Column("publish_online_store");
			Map (x => x.IsArchive).Column("is_archive");
			Map(x => x.TypeOfUse).Column("type_of_use").CustomType<WarehouseUsingStringType>();
			References(x => x.OwningSubdivision).Column("owning_subdivision");
			HasManyToMany(x => x.Nomenclatures).Table("nomenclatures_to_warehouses")
								.ParentKeyColumn("warehouse_id")
								.ChildKeyColumn("nomenclature_id")
								.LazyLoad();
		}
	}
}

