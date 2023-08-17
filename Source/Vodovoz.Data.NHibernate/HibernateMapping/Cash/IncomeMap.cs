using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Cash
{
	public class IncomeMap : ClassMap<Income>
	{
		public IncomeMap()
		{
			Table("cash_income");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.TypeDocument).Column("type_document");
			Map(x => x.TypeOperation).Column("type_operation");
			Map(x => x.Date).Column("date");
			Map(x => x.Money).Column("money");
			Map(x => x.Description).Column("description");
			Map(x => x.CashierReviewComment).Column("cashier_review_comment");
			Map(x => x.IncomeCategoryId).Column("financial_income_category_id");
			Map(x => x.ExpenseCategoryId).Column("financial_expense_category_id");

			References(x => x.Casher).Column("casher_employee_id");
			References(x => x.Employee).Column("employee_id");
			References(x => x.Customer).Column("customer_id");
			References(x => x.RouteListClosing).Column("route_list_id");
			References(x => x.Order).Column("order_id");
			References(x => x.RelatedToSubdivision).Column("related_to_subdivision_id");
			References(x => x.TransferedBy).Column("income_transfered_item_id");
			References(x => x.CashTransferDocument).Column("cash_transfer_document_id").Cascade.All();
			References(x => x.Organisation).Column("organisation_id");
		}
	}
}

