using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class DiscountReasonNomenclatureCategoryMap : ClassMap<DiscountReasonNomenclatureCategory>
	{
		public DiscountReasonNomenclatureCategoryMap()
		{
			Table("discount_reason_nomenclature_categories");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.NomenclatureCategory).Column("nomenclature_category");
		}
	}
}
