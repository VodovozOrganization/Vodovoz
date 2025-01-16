using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Fuel;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Fuel
{
	public class GazpromFuelProductsGroupMap : ClassMap<GazpromFuelProductsGroup>
	{
		public GazpromFuelProductsGroupMap()
		{
			Table("gazprom_fuel_categories");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.FuelTypeId).Column("fuel_type_id");
			Map(x => x.GazpromFuelProductGroupId).Column("gazprom_category_id");
			Map(x => x.GazpromFuelProductGroupName).Column("gazprom_category_name");
			Map(x => x.IsArchived).Column("is_archived");
		}
	}
}
