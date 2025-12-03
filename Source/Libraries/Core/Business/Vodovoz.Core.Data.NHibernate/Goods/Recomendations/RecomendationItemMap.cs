using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Goods.Recomendations;

namespace Vodovoz.Core.Data.NHibernate.Goods.Recomendations
{
	public class RecomendationItemMap : ClassMap<RecomendationItem>
	{
		public RecomendationItemMap()
		{
			Table("goods_recomendation_items");

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.RecomendationId)
				.Column("recomendation_id")
				.Not.Nullable();

			Map(x => x.NomenclatureId)
				.Column("nomenclature_id")
				.Not.Nullable();

			Map(x => x.Priority)
				.Column("priority")
				.Not.Nullable();
		}
	}
}
