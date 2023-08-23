using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash.CashTransfer;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Cash.Transfer
{
	public class CashTransferDocumentMap : ClassMap<CashTransferDocumentBase>
	{
		public CashTransferDocumentMap()
		{
			Table("cash_transfer_documents");
			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.CreationDate).Column("creation_date");
			DiscriminateSubClassesOnColumn("type");

			References(x => x.Driver).Column("driver_id");
			References(x => x.Car).Column("car_id");

			References(x => x.Author).Column("authtor_id");
			Map(x => x.Status).Column("status");
			Map(x => x.TransferedSum).Column("transfered_sum");
			References(x => x.CashTransferOperation).Column("cash_transfer_operation_id").Cascade.All();

			References(x => x.CashSubdivisionFrom).Column("cash_subdivision_from_id");
			References(x => x.ExpenseOperation).Column("cash_expense_id").Cascade.All();
			Map(x => x.ExpenseCategoryId).Column("financial_cash_expense_category_id");
			Map(x => x.SendTime).Column("send_time");
			References(x => x.CashierSender).Column("cashier_sender_id");

			References(x => x.CashSubdivisionTo).Column("cash_subdivision_to_id");
			References(x => x.IncomeOperation).Column("cash_income_id").Cascade.All();
			Map(x => x.IncomeCategoryId).Column("financial_cash_income_category_id");
			Map(x => x.ReceiveTime).Column("receive_time");
			References(x => x.CashierReceiver).Column("cashier_receiver_id");

			Map(x => x.Comment).Column("comment");
		}
	}

	public class IncomeCashTransferDocumentMap : SubclassMap<IncomeCashTransferDocument>
	{
		public IncomeCashTransferDocumentMap()
		{
			DiscriminatorValue("Income");

			HasMany(x => x.CashTransferDocumentIncomeItems).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("cash_transfered_document_id");
			HasMany(x => x.CashTransferDocumentExpenseItems).Cascade.AllDeleteOrphan().Inverse().LazyLoad().KeyColumn("cash_transfered_document_id");
		}
	}

	public class CommonCashTransferDocumentMap : SubclassMap<CommonCashTransferDocument>
	{
		public CommonCashTransferDocumentMap()
		{
			DiscriminatorValue("Common");
		}
	}
}
