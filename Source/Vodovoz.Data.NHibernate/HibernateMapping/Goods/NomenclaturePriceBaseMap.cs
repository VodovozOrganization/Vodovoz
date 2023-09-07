using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class NomenclaturePriceBaseMap : ClassMap<NomenclaturePriceBase>
	{
		public NomenclaturePriceBaseMap()
		{
			Table("nomenclature_price");

			DiscriminateSubClassesOnColumn("type");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.MinCount).Column("min_count");
			Map(x => x.Price).Column("price");

			References(x => x.Nomenclature).Column("nomenclature_id").Not.Nullable();
		}
	}

	public class NomenclaturePriceMap : SubclassMap<NomenclaturePrice>
	{
		public NomenclaturePriceMap()
		{
			DiscriminatorValue(nameof(NomenclaturePriceBase.NomenclaturePriceType.General));
		}
	}

	public class AlternativeNomenclaturePriceMap : SubclassMap<AlternativeNomenclaturePrice>
	{
		public AlternativeNomenclaturePriceMap()
		{
			DiscriminatorValue(nameof(NomenclaturePriceBase.NomenclaturePriceType.Alternative));
		}
	}
}
