using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash;

namespace Vodovoz.HibernateMapping
{
	public class IncomeCategoryMap : ClassMap<IncomeCategory>
	{
		public IncomeCategoryMap ()
		{
			Table("cash_income_category");

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			Map(x => x.Name).Column ("name");
			Map(x => x.IsArchive).Column ("is_archive");
			Map(x => x.IncomeDocumentType).Column("type_document").CustomType<IncomeInvoiceDocumentTypeStringType>();
			Map(x => x.Numbering).Column ("numbering");
			Map(x => x.FinancialIncomeCategoryId).Column("financial_categories_group_id");

			References(x => x.Subdivision).Column("subdivision_id");
			
			References(x => x.Parent).Column("parent_id");
			HasMany (x => x.Childs).Inverse().Cascade.All ().LazyLoad ().KeyColumn ("parent_id");
		}
	}
}
