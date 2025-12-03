using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Goods.Recomendations;

namespace Vodovoz.Core.Data.NHibernate.Goods.Recomendations
{
	public class RecomendationMap : ClassMap<Recomendation>
	{
		public RecomendationMap()
		{
			Table("goods_recomendations");

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.Name)
				.Column("name")
				.Not.Nullable();

			Map(x => x.IsArchive)
				.Column("is_archive")
				.Not.Nullable();

			Map(x => x.PersonType)
				.Column("person_type");

			Map(x => x.RoomType)
				.Column("room_type");

			HasMany(x => x.Items)
				.KeyColumn("recomendation_id")
				.Not.LazyLoad()
				.Inverse()
				.Cascade.AllDeleteOrphan()
				.OrderBy(x => x.Priority)
				.AsSet();
		}
	}
}
