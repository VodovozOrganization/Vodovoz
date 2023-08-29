using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Cash
{
	public class ExpenseCategoryMap : ClassMap<ExpenseCategory>
	{
		public ExpenseCategoryMap()
		{
			Table("cash_expense_category");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.Numbering).Column("numbering");
			Map(x => x.ExpenseDocumentType).Column("type_document");
			Map(x => x.FinancialExpenseCategoryId).Column("financial_categories_group_id");

			References(x => x.Subdivision).Column("subdivision_id");

			References(x => x.Parent).Column("parent_id");
			HasMany(x => x.Childs).Inverse().Cascade.All().LazyLoad().KeyColumn("parent_id");
		}
	}
}
