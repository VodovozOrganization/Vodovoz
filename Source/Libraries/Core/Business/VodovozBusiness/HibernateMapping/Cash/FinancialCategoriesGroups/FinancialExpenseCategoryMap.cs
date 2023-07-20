using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;

namespace Vodovoz.HibernateMapping.Cash.FinancialCategoriesGroups
{
	public class FinancialExpenseCategoryMap : ClassMap<FinancialExpenseCategory>
	{
		public FinancialExpenseCategoryMap()
		{
			Table("financial_categories_groups");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.GroupType).Column("group_type").Access.ReadOnly();
			Map(x => x.FinancialSubtype).Column("financial_subtype").Access.ReadOnly();

			Map(x => x.ParentId).Column("parent_id");
			Map(x => x.Title).Column("title");
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.TargetDocument).Column("target_document");
			Map(x => x.SubdivisionId).Column("subdivision_id");
			Map(x => x.Numbering).Column("numbering");
			Map(x => x.ExcludeFromCashFlowDds).Column("exclude_from_cash_flow_dds");

			Where("group_type = 'Category' AND financial_subtype = 'Expense'");
		}
	}
}
