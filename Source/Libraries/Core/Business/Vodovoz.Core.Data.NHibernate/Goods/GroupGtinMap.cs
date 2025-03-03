using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Core.Data.NHibernate.Goods
{
	public class GroupGtinMap : ClassMap<GroupGtinEntity>
	{
		public GroupGtinMap()
		{
			Table("group_gtins");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.GtinNumber)
				.Column("gtin");

			Map(x => x.CodesCount)
				.Column("codes_count");

			References(x => x.Nomenclature)
				.Column("nomenclature_id");
		}
	}
}
