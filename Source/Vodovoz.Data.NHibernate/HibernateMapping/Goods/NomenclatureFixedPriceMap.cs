using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class NomenclatureFixedPriceMap : ClassMap<NomenclatureFixedPrice>
	{
		public NomenclatureFixedPriceMap()
		{
			Table("nomenclature_fixed_prices");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Price).Column("fixed_price");
			Map(x => x.MinCount).Column("min_count");
			Map(x => x.IsEmployeeFixedPrice).Column("is_employee_fixed_price");

			References(x => x.Counterparty).Column("counterparty_id");
			References(x => x.DeliveryPoint).Column("delivery_point_id");
			References(x => x.Nomenclature).Column("nomenclature_id");
		}
	}
}
