using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Core.Data.NHibernate.Goods
{
	public class NomenclatureMap : ClassMap<NomenclatureEntity>
	{
		public NomenclatureMap()
		{
			Table("nomenclature");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
			Map(x => x.Category).Column("category");
			Map(x => x.IsAccountableInTrueMark).Column("is_accountable_in_chestniy_znak");
			Map(x => x.Gtin).Column("gtin");
			Map(x => x.VAT).Column("vat");

			References(x => x.Unit).Column("unit_id").Fetch.Join().Not.LazyLoad();
			References(x => x.DependsOnNomenclature).Column("depends_on_nomenclature");

			HasMany(x => x.AttachedFileInformations).Cascade.AllDeleteOrphan().Inverse().KeyColumn("nomenclature_id");

			HasMany(x => x.NomenclaturePrice)
				.Where($"type='{NomenclaturePriceEntityBase.NomenclaturePriceType.General}'")
				.Inverse().Cascade.AllDeleteOrphan().LazyLoad().KeyColumn("nomenclature_id");

			HasMany(x => x.AlternativeNomenclaturePrices)
				.Where($"type='{NomenclaturePriceEntityBase.NomenclaturePriceType.Alternative}'")
				.Inverse().Cascade.AllDeleteOrphan().LazyLoad().KeyColumn("nomenclature_id");
		}
	}
}
