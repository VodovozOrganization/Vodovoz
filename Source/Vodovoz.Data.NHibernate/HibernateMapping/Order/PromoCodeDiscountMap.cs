using FluentNHibernate.Mapping;
using NHibernate.Type;
using Vodovoz.Core.Domain.Sale;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class PromoCodeDiscountMap : SubclassMap<PromoCodeDiscount>
	{
		public PromoCodeDiscountMap()
		{
			DiscriminatorValue(nameof(DiscountReasonType.PromoCode));
			
			Map(x => x.OrderMinSum).Column("promo_code_order_min_sum");
			Map(x => x.IsOneTimePromoCode).Column("is_one_time_promo_code");
			Map(x => x.StartTime).Column("start_time_promo_code").CustomType<TimeAsTimeSpanType>();
			Map(x => x.EndTime).Column("end_time_promo_code").CustomType<TimeAsTimeSpanType>();
			Map(x => x.StartDate).Column("start_date_promo_code");
			Map(x => x.EndDate).Column("end_date_promo_code");
		}
	}
}
