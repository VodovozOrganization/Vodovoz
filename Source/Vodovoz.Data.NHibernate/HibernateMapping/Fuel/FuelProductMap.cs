using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Fuel;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Fuel
{
	public class FuelProductMap : ClassMap<FuelProduct>
	{
		public FuelProductMap()
		{
			Table("fuel_products");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.FuelTypeId).Column("fuel_type_id");
			Map(x => x.ProductId).Column("product_id");
			Map(x => x.Description).Column("description");
			Map(x => x.IsArchived).Column("is_archived");
		}
	}
}
