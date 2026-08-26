using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Sale
{
	public class PersonalDiscountMap : ClassMap<PersonalDiscount>
	{
		public PersonalDiscountMap()
		{
			Table("personal_discounts");
			
			Id(x => x.Id).Column("id").GeneratedBy.Native();
			
			References(x => x.DiscountReason)
				.Column("discount_reason_id")
				.Access.CamelCaseField(Prefix.Underscore);

			Component(
				x => x.DiscountValue,
				m =>
				{
					m.Map(x => x.Discount).Column("percent_discount");
					m.Map(x => x.DiscountMoney).Column("money_discount");
					m.Map(x => x.IsDiscountMoney).Column("is_discount_money");
				});
		}
	}
}
