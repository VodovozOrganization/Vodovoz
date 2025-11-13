using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Fuel;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Fuel
{
	public class GazpromFuelProductMap : ClassMap<GazpromFuelProduct>
	{
		public GazpromFuelProductMap()
		{
			Table("gazprom_fuel_products");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.GazpromProductsGroupId).Column("gazprom_products_group_id");
			Map(x => x.GazpromFuelProductId).Column("gazprom_product_id");
			Map(x => x.GazpromFuelProductName).Column("gazprom_product_name");
			Map(x => x.IsArchived).Column("is_archived");
		}
	}
}
