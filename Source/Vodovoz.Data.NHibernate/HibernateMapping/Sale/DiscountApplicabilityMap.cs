using FluentNHibernate.Mapping;
using VodovozBusiness.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Sale
{
	public class DiscountApplicabilityMap : ClassMap<DiscountApplicability>
	{
		public DiscountApplicabilityMap()
		{
			Table("discount_applicabilities");
			
			Id(x => x.Id).GeneratedBy.Native();
			
			Map(x => x.DiscountType).Column("discount_type").Not.Nullable();
			Map(x => x.UseDiscountType).Column("use_discount_type").Not.Nullable();
			
			References(x => x.DiscountReason).Column("discount_reason_id");
		}
	}
}
