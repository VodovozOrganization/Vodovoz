using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class TaxcomDocflowMap : ClassMap<TaxcomDocflow>
	{
		public TaxcomDocflowMap()
		{
			Table("taxcom_docflows");
			
			Id(x => x.Id).GeneratedBy.Native();
			
			Map(x => x.DocflowId)
				.Column("docflow_id");

			Map(x => x.CreationTime)
				.Column("creation_time");

			Map(x => x.MainDocumentId)
				.Column("main_document_id");

			Map(x => x.EdoDocumentId)
				.Column("edo_document_id");

			Map(x => x.IsReceived)
				.Column("is_received");

			Map(x => x.AcceptingIngoingDocflowTime)
				.Column("accepting_ingoing_docflow_time");

			Map(x => x.IsReminderToAcceptUpdEmailSent)
				.Column("is_reminder_to_accept_upd_email_sent");

			Map(x => x.CancellationOfferMessage)
				.Column("cancellation_offer_message");

			Map(x => x.CorrectionNoticeMessage)
				.Column("correction_notice_message");

			Map(x => x.DiscrepancyMessage)
				.Column("discrepancy_message");

			Map(x => x.DocFlowState).Column("state");
			Map(x => x.StateChangeTime).Column("state_change_time");
			Map(x => x.ErrorMessage).Column("error_message");
			Map(x => x.TrueMarkTraceabilityStatus).Column("true_mark_traceability_status");
		}
	}
}
