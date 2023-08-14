using FluentNHibernate.Mapping;
using Vodovoz.Domain.Fuel;

namespace Vodovoz.HibernateMapping.Fuel
{
	public class FuelWriteoffDocumentMap : ClassMap<FuelWriteoffDocument>
	{
		public FuelWriteoffDocumentMap()
		{
			Table("fuel_writeoff_documents");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.Date).Column("date");
			Map(x => x.Reason).Column("reason");
			Map(x => x.ExpenseCategoryId).Column("financial_expense_category_id");

			References(x => x.Cashier).Column("cashier_id");
			References(x => x.Employee).Column("employee_id");
			References(x => x.CashSubdivision).Column("cash_subdivision_id");

			HasMany(x => x.FuelWriteoffDocumentItems).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("fuel_writeoff_document_id");
		}
	}
}
