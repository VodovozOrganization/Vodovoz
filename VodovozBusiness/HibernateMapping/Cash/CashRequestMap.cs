using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash;

namespace Vodovoz.HibernateMapping
{
	public class CashRequestMap: ClassMap<CashRequest>
	{
		public CashRequestMap()
		{
			Table("cash_request");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			//string
			Map(x => x.Explanation).Column("explanation");
			Map(x => x.Basis).Column("ground");
			Map(x => x.CancelReason).Column("cancel_reason");
			Map(x => x.ReasonForSendToReappropriate).Column ("reason_for_send_to_reappropriate");
			Map(x => x.State).Column("state").CustomType<CashRequest.CashRequestStateStringType>();
			Map(x => x.DocumentType).Column("document_type").CustomType<CashRequest.CashRequestDocTypeStringType>();
			//bool
			Map(x => x.HaveReceipt).Column("have_receipt");
			Map(x => x.PossibilityNotToReconcilePayments).Column("possibility_not_to_reconcile_payments");
			//datetime
			Map(x => x.Date).Column("date");
			//Refs
			References(x => x.Subdivision).Column("subdivision_id");
			References(x => x.Author).Column("employee_id");
			References(x => x.Organization).Column("organization_id");
			References(x => x.ExpenseCategory).Column("expense_category_id");

			HasMany(x => x.Sums).Inverse().Cascade.All().LazyLoad().KeyColumn("cash_request_id");
		}
	}
}
