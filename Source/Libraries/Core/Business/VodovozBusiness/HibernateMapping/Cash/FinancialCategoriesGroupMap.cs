using FluentNHibernate.Conventions;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash;

namespace Vodovoz.HibernateMapping.Cash
{
	public class FinancialCategoriesGroupMap : ClassMap<FinancialCategoriesGroup>
	{
		public FinancialCategoriesGroupMap()
		{
			Table("financial_categories_groups");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.ParentId).Column("parent_id");
			Map(x => x.Title).Column("title");
			Map(x => x.IsArchive).Column("is_archive");
		}
	}
}
