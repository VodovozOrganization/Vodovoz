using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash;

namespace Vodovoz.HibernateMapping
{
	public class ExpenseMap : ClassMap<Expense>
	{
		public ExpenseMap ()
		{
			Table("cash_expense");

			Id(x => x.Id).Column ("id").GeneratedBy.Native();

			Map(x => x.TypeDocument)	.Column("type_document").CustomType<ExpenseInvoiceDocumentTypeStringType>();
			Map(x => x.TypeOperation)	.Column("type_operation").CustomType<ExpenseTypeStringType> ();
			Map(x => x.Date)			.Column("date");
			Map(x => x.Money)			.Column("money");
			Map(x => x.AdvanceClosed)	.Column("advance_closed");
			Map(x => x.Description)		.Column("description");

			References(x => x.Casher)			.Column("casher_employee_id");
			References(x => x.Employee)			.Column("employee_id");
			References(x => x.ExpenseCategory)	.Column("cash_expense_category_id");
			References(x => x.RouteListClosing)	.Column("route_list_id");
			References(x => x.WagesOperation)	.Column("wages_movement_operations_id");
			References(x => x.Order)			.Column("order_id");
			References(x => x.RelatedToSubdivision).Column("related_to_subdivision_id");
			References(x => x.TransferedBy).Column("expense_transfered_item_id");
			References(x => x.CashTransferDocument).Column("cash_transfer_document_id").Cascade.All();

			HasMany(x => x.AdvanceCloseItems).Cascade.AllDeleteOrphan ().Inverse ().LazyLoad ().KeyColumn ("expense_id");
		}
	}
}

