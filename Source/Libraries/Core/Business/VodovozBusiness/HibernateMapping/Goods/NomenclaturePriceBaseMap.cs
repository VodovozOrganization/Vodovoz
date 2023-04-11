﻿using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods;

namespace Vodovoz.HibernateMapping
{
	public class NomenclaturePriceBaseMap : ClassMap<NomenclaturePriceBase>
	{
		public NomenclaturePriceBaseMap ()
		{
			Table ("nomenclature_price");

			DiscriminateSubClassesOnColumn("type");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.MinCount).Column ("min_count");
			Map (x => x.Price).Column ("price");
			Map(x => x.Type).Column("type").CustomType<NomenclaturePriceBase.NomenclaturePriceTypeString>().ReadOnly();

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
