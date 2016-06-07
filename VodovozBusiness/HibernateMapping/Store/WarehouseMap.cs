using FluentNHibernate.Mapping;
using Vodovoz.Domain.Store;

namespace Vodovoz.HMap
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
		}
	}
}

