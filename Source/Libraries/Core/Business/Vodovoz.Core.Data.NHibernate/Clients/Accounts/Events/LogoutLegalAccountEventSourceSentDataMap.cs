using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Clients.Accounts.Events;

namespace Vodovoz.Core.Data.NHibernate.Clients.Accounts.Events
{
	public class LogoutLegalAccountEventSourceSentDataMap : ClassMap<LogoutLegalAccountEventSourceSentData>
	{
		public LogoutLegalAccountEventSourceSentDataMap()
		{
			Table("logout_legal_accounts_events_sources_sent_data");
			
			Id(x => x.Id).GeneratedBy.Native();
			
			Map(x => x.Source)
				.Column("source")
				.Not.Nullable();
			
			Map(x => x.Delivered)
				.Column("delivered")
				.Not.Nullable();

			Map(x => x.LastSentDateTime)
				.Column("last_sent_datetime");
			
			Map(x => x.SentEventsCount)
				.Column("sent_events_count")
				.Not.Nullable();
			
			References(x => x.Event)
				.Column("event_id");
		}
	}
}
