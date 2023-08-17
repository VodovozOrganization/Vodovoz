using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class ReturnTareReasonMap : ClassMap<ReturnTareReason>
	{
		public ReturnTareReasonMap()
		{
			Table("return_tare_reasons");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");

			HasManyToMany(x => x.ReasonCategories)
				.Table("return_tare_reasons_to_categories")
				.ParentKeyColumn("return_tare_reason_id")
				.ChildKeyColumn("return_tare_reason_category_id")
				.LazyLoad();
		}
	}
}