using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Cash.FinancialCategoriesGroups
{
	public class FinancialIncomeCategoryMap : ClassMap<FinancialIncomeCategory>
	{
		public FinancialIncomeCategoryMap()
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
			Map(x => x.IsHiddenFromPublicAccess).Column("is_hidden_from_public_access");

			Where("group_type = 'Category' AND financial_subtype = 'Income'");
		}
	}
}
