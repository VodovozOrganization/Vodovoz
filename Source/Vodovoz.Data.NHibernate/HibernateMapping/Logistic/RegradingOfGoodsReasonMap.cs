using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic
{
	public class RegradingOfGoodsReasonMap : ClassMap<RegradingOfGoodsReason>
	{
		public RegradingOfGoodsReasonMap()
		{
			Table("regrading_of_goods_reasons");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Name).Column("name");
		}
	}
}
