using FluentNHibernate.Mapping;
using Vodovoz.Domain.Sale;

namespace Vodovoz.HibernateMapping.Order
{
	public class DeliveryPriceRuleMap : ClassMap<DeliveryPriceRule>
	{
		public DeliveryPriceRuleMap()
		{
			Table("delivery_price_rules");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Water19LCount).Column("wtr_19L_qty");
		}
	}
}