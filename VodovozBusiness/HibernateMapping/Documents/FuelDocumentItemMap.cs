using System;
using FluentNHibernate.Mapping;

namespace Vodovoz
{
	public class FuelDocumentItemMap: ClassMap<FuelDocumentItem>
	{
		public FuelDocumentItemMap ()
		{
			Table ("fuel_documents_items");

			Id (x => x.Id).Column ("id").GeneratedBy.Native ();

			Map (x => x.TicketsCount).Column ("tickets_count");

			References (x => x.GasTicket).Column ("gas_ticket_id");
			References (x => x.Document).Column ("fuel_document_id");

			HasMany (x => x.Items).Cascade.AllDeleteOrphan ().Inverse ().KeyColumn ("fuel_document_id");
		}
	}
}

