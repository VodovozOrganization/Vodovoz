using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods;

namespace Vodovoz.HibernateMapping.Goods
{
	public class NomenclatureOnlineGroupMap : ClassMap<NomenclatureOnlineGroup>
	{
		public NomenclatureOnlineGroupMap()
		{
			Table("nomenclature_online_groups");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			
			Map(x => x.Name).Column("name");
			
			HasMany(x => x.NomenclatureOnlineCategories)
				.KeyColumn("nomenclature_online_group_id")
				.Inverse()
				.Cascade
				.AllDeleteOrphan();
		}
	}
}
