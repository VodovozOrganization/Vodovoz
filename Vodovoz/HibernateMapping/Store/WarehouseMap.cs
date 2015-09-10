using FluentNHibernate.Mapping;
using Vodovoz.Domain.Store;

namespace Vodovoz.HMap
{
	public class WarehouseMap : ClassMap<Warehouse>
	{
		public WarehouseMap ()
		{
			Table ("warehouses");
			Not.LazyLoad ();

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Name).Column ("name");
		}
	}
}

