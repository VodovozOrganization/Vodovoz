using FluentNHibernate.Mapping;
using Vodovoz.Domain.Store;

namespace Vodovoz.HibernateMapping
{
	public class RegradingOfGoodsTemplateMap : ClassMap<RegradingOfGoodsTemplate>
	{
		public RegradingOfGoodsTemplateMap ()
		{
			Table ("store_regrading_of_goods_template");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();
			Map (x => x.Name).Column ("name");
			HasMany (x => x.Items).Cascade.AllDeleteOrphan ().Inverse ().KeyColumn ("store_regrading_of_goods_template_id");
		}
	}
}