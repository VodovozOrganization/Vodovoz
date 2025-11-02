using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.Core.Data.NHibernate.Goods
{
	public class GtinMap : ClassMap<GtinEntity>
	{
		public GtinMap()
		{
			Table("gtins");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.GtinNumber)
				.Column("gtin");

			References(x => x.Nomenclature)
				.Column("nomenclature_id");
		}
	}
}
