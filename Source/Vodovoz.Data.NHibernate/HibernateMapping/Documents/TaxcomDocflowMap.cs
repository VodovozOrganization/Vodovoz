using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Documents
{
	public class TaxcomDocflowMap : ClassMap<TaxcomDocflow>
	{
		public TaxcomDocflowMap()
		{
			Table("taxcom_docflows");
			
			Id(x => x.Id).GeneratedBy.Native();
			
			Map(x => x.DocflowId).Column("docflow_id");
			Map(x => x.CreationTime).Column("creation_time");
			Map(x => x.MainDocumentId).Column("main_document_id");
			Map(x => x.EdoDocumentId).Column("edo_document_id");
			Map(x => x.IsReceived).Column("is_received");
			Map(x => x.AcceptingIngoingDocflowTime).Column("accepting_ingoing_docflow_time");
			Map(x => x.IsReminderToAcceptUpdEmailSent).Column("is_reminder_to_accept_upd_email_sent");

			HasMany(x => x.Actions)
				.KeyColumn("taxcom_docflow_id")
				.Cascade
				.AllDeleteOrphan();
		}
	}
}
