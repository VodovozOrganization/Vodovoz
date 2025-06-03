using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods.NomenclaturesOnlineParameters;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class NomenclatureOnlinePriceMap : ClassMap<NomenclatureOnlinePrice>
	{
		public NomenclatureOnlinePriceMap()
		{
			Table("nomenclatures_online_prices");
			DiscriminateSubClassesOnColumn("type");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.PriceWithoutDiscount).Column("price_without_discount");

			References(x => x.NomenclaturePrice).Column("nomenclature_price_id");
			References(x => x.NomenclatureOnlineParameters).Column("nomenclature_online_parameters_id");
		}
	}
}
