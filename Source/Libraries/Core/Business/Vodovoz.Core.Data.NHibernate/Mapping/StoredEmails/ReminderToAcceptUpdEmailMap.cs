using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.StoredEmails;

namespace Vodovoz.Core.Data.NHibernate.Mapping.StoredEmails
{
	public class ReminderToAcceptUpdEmailMap : SubclassMap<ReminderToAcceptUpdEmail>
	{
		public ReminderToAcceptUpdEmailMap()
		{
			DiscriminatorValue(nameof(CounterpartyEmailType.ReminderToAcceptUpd));

			References(x => x.OrderDocument).Column("order_document_id");
		}		
	}
}
