using FluentNHibernate.Mapping;
using Vodovoz.Domain.Store;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Store
{
	public class MovementWagonMap : ClassMap<MovementWagon>
	{
		public MovementWagonMap()
		{
			Table("store_movement_wagon");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
		}
	}
}

