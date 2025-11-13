using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.Core.Data.NHibernate.Edo
{
	public class TaxcomEdoDocflowLastProcessTimeMap : ClassMap<TaxcomEdoDocflowLastProcessTime>
	{
		public TaxcomEdoDocflowLastProcessTimeMap()
		{
			Table("taxcom_edo_docflow_last_process_times");
			DynamicUpdate();
			
			Id(x => x.Id).GeneratedBy.Native();
			
			Map(x => x.LastProcessedEventOutgoingDocuments).Column("last_processed_event_outgoing_documents");
			Map(x => x.LastProcessedEventIngoingDocuments).Column("last_processed_event_ingoing_documents");
			Map(x => x.LastProcessedEventWaitingForCancellationDocuments)
				.Column("last_processed_event_waiting_for_cancellation");
			Map(x => x.LastProcessedContactsUpdates).Column("last_processed_contacts_updates");
			Map(x => x.OrganizationId).Column("organization_id");
		}
	}
}
