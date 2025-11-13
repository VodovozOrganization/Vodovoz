using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Documents;

namespace Vodovoz.Core.Data.NHibernate.Documents
{
	public class EdoContainerMap : ClassMap<EdoContainerEntity>
	{
		public EdoContainerMap()
		{
			Table("edo_containers");

			Id(x => x.Id).GeneratedBy.Native();

			Map(x => x.Received).Column("received");
			Map(x => x.IsIncoming).Column("is_incoming");
			Map(x => x.DocFlowId).Column("doc_flow_id");
			Map(x => x.InternalId).Column("internal_id");
			Map(x => x.EdoDocFlowStatus).Column("edo_doc_flow_status");
			Map(x => x.ErrorDescription).Column("error_description");
			Map(x => x.MainDocumentId).Column("main_document_id");
			Map(x => x.Created).Column("created");
			Map(x => x.Type).Column("type");
			Map(x => x.EdoTaskId).Column("edo_task_id");

			References(x => x.Order).Column("order_id");
			References(x => x.Counterparty).Column("counterparty_id");
		}
	}
}
