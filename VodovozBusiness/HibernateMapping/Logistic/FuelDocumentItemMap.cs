using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

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
		}
	}
}

