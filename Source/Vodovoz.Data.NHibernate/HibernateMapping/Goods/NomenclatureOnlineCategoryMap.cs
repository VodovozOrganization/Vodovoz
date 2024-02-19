using FluentNHibernate.Mapping;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class NomenclatureOnlineCategoryMap : ClassMap<NomenclatureOnlineCategory>
	{
		public NomenclatureOnlineCategoryMap()
		{
			Table("nomenclature_online_categories");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			
			Map(x => x.Name).Column("name");
			
			References(x => x.NomenclatureOnlineGroup).Column("nomenclature_online_group_id");
		}
	}
}
