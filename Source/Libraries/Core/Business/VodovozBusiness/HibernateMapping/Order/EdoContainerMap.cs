using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders.Documents;

namespace Vodovoz.HibernateMapping.Order
{
	public class EdoContainerMap : ClassMap<EdoContainer>
	{
		public EdoContainerMap()
		{
			Table("edo_containers");

			Id(x => x.Id).GeneratedBy.Native();

			Map(x => x.Received).Column("received");
			Map(x => x.IsIncoming).Column("is_incoming");
			Map(x => x.DocFlowId).Column("doc_flow_id");
			Map(x => x.InternalId).Column("internal_id");
			Map(x => x.EdoContainerStatus).Column("edo_container_status");
			Map(x => x.ErrorDescription).Column("error_description");
			Map(x => x.MainDocumentId).Column("main_document_id");
			Map(x => x.Container).Column("container");

			References(x => x.Order).Column("order_id");
			References(x => x.Counterparty).Column("counterparty_id");
		}
	}
}
