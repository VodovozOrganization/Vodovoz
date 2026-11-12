using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Sale;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Sale
{
	public class OnlineOrderTemplatePromoSetMap : ClassMap<OnlineOrderTemplatePromoSet>
	{
		public OnlineOrderTemplatePromoSetMap()
		{
			Table("online_orders_templates_promo_sets");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Count).Column("count").Not.Nullable();
			Map(x => x.TemplateId).Column("template_id").Not.Nullable();
			Map(x => x.Price).Column("price").Not.Nullable();

			References(x => x.PromoSet).Column("promo_set_id").Not.Nullable();
			References(x => x.PersonalDiscount).Column("personal_discount_id");

			HasManyToMany(x => x.DiscountReasons)
				.Table("discount_reasons_templates_promo_sets")       
				.ParentKeyColumn("promo_set_id")
				.ChildKeyColumn("discount_reason_id")
				.Cascade.None()
				.LazyLoad();
		}
	}
}
