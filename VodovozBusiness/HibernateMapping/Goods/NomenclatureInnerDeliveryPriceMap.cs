using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods;

namespace Vodovoz.HibernateMapping.Goods
{
	public class NomenclatureInnerDeliveryPriceMap : ClassMap<NomenclatureInnerDeliveryPrice>
	{
		public NomenclatureInnerDeliveryPriceMap()
		{
			Table("nomenclature_inner_delivery_prices");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.StartDate).Column("start_date");
			Map(x => x.EndDate).Column("end_date");
			Map(x => x.Price).Column("price");

			References(x => x.Nomenclature).Column("nomenclature_id").Not.Nullable();
		}

	}
}
