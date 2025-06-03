using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Goods;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class GroupGtinMap : ClassMap<GroupGtin>
	{
		public GroupGtinMap()
		{
			Table("group_gtins");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.GtinNumber).Column("gtin");
			Map(x => x.CodesCount).Column("codes_count");

			References(x => x.Nomenclature).Column("nomenclature_id");
		}
	}
}
