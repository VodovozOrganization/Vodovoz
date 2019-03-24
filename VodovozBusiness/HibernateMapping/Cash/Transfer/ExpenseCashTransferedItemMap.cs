using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash.CashTransfer;
namespace Vodovoz.HibernateMapping.Cash.Transfer
{
	public class ExpenseCashTransferedItemMap : ClassMap<ExpenseCashTransferedItem>
	{
		public ExpenseCashTransferedItemMap()
		{
			Table("cash_expense_transfered_items");
			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			References(x => x.Document).Column("cash_transfered_document_id");
			References(x => x.Expense).Column("cash_expense_id");
			Map(x => x.Comment).Column("comment");
		}
	}
}
