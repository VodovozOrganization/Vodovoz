using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods;

namespace Vodovoz.HibernateMapping.Goods
{
	public class AdditionalLoadingNomenclatureDistributionMap : ClassMap<AdditionalLoadingNomenclatureDistribution>
	{
		public AdditionalLoadingNomenclatureDistributionMap()
		{
			Table("additional_loading_nomenclature_distribution");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Percent).Column("percent");

			References(x => x.Nomenclature).Column("nomenclature_id");
		}
	}
}
