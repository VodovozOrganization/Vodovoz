using FluentNHibernate.Mapping;
using Vodovoz.Domain.Cash;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Cash
{
	public class PayoutRequestBaseMap : ClassMap<PayoutRequestBase>
	{
		public PayoutRequestBaseMap()
		{
			Table("cash_request");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			DiscriminateSubClassesOnColumn("document_type");
			//string
			Map(x => x.Explanation).Column("explanation");
			Map(x => x.Basis).Column("ground");
			Map(x => x.CancelReason).Column("cancel_reason");
			Map(x => x.ReasonForSendToReappropriate).Column("reason_for_send_to_reappropriate");
			Map(x => x.PayoutRequestState).Column("state");
			Map(x => x.PayoutRequestDocumentType).Column("document_type")
				.Update().Not.Insert();
			//bool
			Map(x => x.PossibilityNotToReconcilePayments).Column("possibility_not_to_reconcile_payments");
			//datetime
			Map(x => x.Date).Column("date");
			//Refs
			Map(x => x.ExpenseCategoryId).Column("financial_expense_category_id");
			References(x => x.Subdivision).Column("subdivision_id");
			References(x => x.Author).Column("employee_id");
			References(x => x.Organization).Column("organization_id");
		}
	}
}
