using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Core.Data.NHibernate.Goods
{
	public class NomenclaturePriceEntityBaseMap : ClassMap<NomenclaturePriceEntityBase>
	{
		public NomenclaturePriceEntityBaseMap()
		{
			Table("nomenclature_price");

			DiscriminateSubClassesOnColumn("type");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.MinCount).Column("min_count");
			Map(x => x.Price).Column("price");

			References(x => x.Nomenclature).Column("nomenclature_id").Not.Nullable();
		}
	}

	public class NomenclaturePriceEntityMap : SubclassMap<NomenclaturePriceEntity>
	{
		public NomenclaturePriceEntityMap()
		{
			DiscriminatorValue(nameof(NomenclaturePriceGeneralBase.NomenclaturePriceType.General));
		}
	}

	public class AlternativeNomenclaturePriceEntityMap : SubclassMap<AlternativeNomenclaturePriceEntity>
	{
		public AlternativeNomenclaturePriceEntityMap()
		{
			DiscriminatorValue(nameof(NomenclaturePriceEntityBase.NomenclaturePriceType.Alternative));
		}
	}
}
