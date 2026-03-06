using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class OnlineOrderTemplateProductDiscountMap : ClassMap<OnlineOrderTemplateProductDiscount>
	{
		public OnlineOrderTemplateProductDiscountMap()
		{
			Table("template_products_discounts");
			
			Id(x => x.Id).GeneratedBy.Native();
			
			Map(x => x.MoneyDiscount)
				.Column("money_discount")
				.Not.Nullable();
			Map(x => x.PercentDiscount)
				.Column("percent_discount")
				.Not.Nullable();
			Map(x => x.IsDiscountInMoney)
				.Column("is_discount_in_money")
				.Not.Nullable();
			
			References(x => x.DiscountReason)
				.Column("discount_reason");
			
			References(x => x.TemplateProduct)
				.Column("template_product_id")
				.Not.Nullable();
		}
	}
}
