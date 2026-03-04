using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Goods;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Goods
{
	public class GtinMap : ClassMap<Gtin>
	{
		public GtinMap()
		{
			Table("gtins");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.GtinNumber).Column("gtin");
			Map(x => x.Priority).Column("priority");

			References(x => x.Nomenclature).Column("nomenclature_id");
		}
	}
}
