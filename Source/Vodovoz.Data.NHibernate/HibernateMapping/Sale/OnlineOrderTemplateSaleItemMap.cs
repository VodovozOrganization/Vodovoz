using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Sale;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Sale
{
	public class OnlineOrderTemplateSaleItemMap : ClassMap<OnlineOrderTemplateSaleItem>
	{
		public OnlineOrderTemplateSaleItemMap()
		{
			Table("online_order_templates_sale_items");
			
			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();
			
			Map(x => x.Count)
				.Column("count")
				.Not.Nullable();
			
			Map(x => x.TemplateId)
				.Column("template_id");

			Map(x => x.Price)
				.Column("price");
			
			Map(x => x.IsFixedPrice)
				.Column("is_fixed_price");
			
			Map(x => x.IsUserPrice)
				.Column("is_user_price");

			References(x => x.Nomenclature)
				.Column("nomenclature_id");
			
			References(x => x.PromoSet)
				.Column("promo_set_id");
			
			References(x => x.PersonalDiscount)
				.Column("personal_discount_id");

			HasManyToMany(x => x.DiscountReasons)
				.Table("template_sale_items_to_discount_reasons")
				.ParentKeyColumn("template_sale_item_id")
				.ChildKeyColumn("discount_reason_id")
				.LazyLoad();
		}
	}
}
