using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Goods.Gtins;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class GtinMap : ClassMap<Gtin>
	{
		public GtinMap()
		{
			Table("gtins");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.GtinNumber).Column("gtin");

			References(x => x.Nomenclature).Column("nomenclature_id");
		}
	}
}
