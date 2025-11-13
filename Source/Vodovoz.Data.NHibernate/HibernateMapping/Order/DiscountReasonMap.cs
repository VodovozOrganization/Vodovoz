using FluentNHibernate.Mapping;
using NHibernate.Type;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class DiscountReasonMap : ClassMap<DiscountReason>
	{
		public DiscountReasonMap()
		{
			Table("discount_reasons");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.Value).Column("value");
			Map(x => x.IsPremiumDiscount).Column("is_premium_discount");
			Map(x => x.IsPresent).Column("is_present");
			Map(x => x.ValueType).Column("value_type");
			Map(x => x.IsPromoCode).Column("is_promo_code");
			Map(x => x.PromoCodeName).Column("promo_code_name");
			Map(x => x.PromoCodeOrderMinSum).Column("promo_code_order_min_sum");
			Map(x => x.IsOneTimePromoCode).Column("is_one_time_promo_code");
			Map(x => x.StartTimePromoCode).Column("start_time_promo_code").CustomType<TimeAsTimeSpanType>();
			Map(x => x.EndTimePromoCode).Column("end_time_promo_code").CustomType<TimeAsTimeSpanType>();
			Map(x => x.StartDatePromoCode).Column("start_date_promo_code");
			Map(x => x.EndDatePromoCode).Column("end_date_promo_code");

			HasManyToMany(x => x.NomenclatureCategories)
				.Table("discount_reasons_nomenclature_categories")
				.ParentKeyColumn("discount_reason_id")
				.ChildKeyColumn("discount_nomenclature_category_id")
				.LazyLoad();

			HasManyToMany(x => x.Nomenclatures)
				.Table("discount_reasons_nomenclatures")
				.ParentKeyColumn("discount_reason_id")
				.ChildKeyColumn("nomenclature_id")
				.LazyLoad();

			HasManyToMany(x => x.ProductGroups)
				.Table("discount_reasons_nomenclature_groups")
				.ParentKeyColumn("discount_reason_id")
				.ChildKeyColumn("group_id")
				.LazyLoad();
		}
	}
}
