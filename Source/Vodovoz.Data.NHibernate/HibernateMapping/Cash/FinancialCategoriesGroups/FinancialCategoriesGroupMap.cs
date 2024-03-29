using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Cash.FinancialCategoriesGroups
{
	public class FinancialCategoriesGroupMap : ClassMap<FinancialCategoriesGroup>
	{
		public FinancialCategoriesGroupMap()
		{
			Table("financial_categories_groups");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.GroupType).Column("group_type").Access.ReadOnly();
			Map(x => x.FinancialSubtype).Column("financial_subtype");

			Map(x => x.ParentId).Column("parent_id");
			Map(x => x.Title).Column("title");
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.Numbering).Column("numbering");
			Map(x => x.IsHiddenFromPublicAccess).Column("is_hidden_from_public_access");

			Where("group_type = 'Group'");
		}
	}
}
