using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Clients.Accounts.Events;

namespace Vodovoz.Core.Data.NHibernate.Clients.Accounts.Events
{
	public class LogoutLegalAccountEventMap : ClassMap<LogoutLegalAccountEvent>
	{
		public LogoutLegalAccountEventMap()
		{
			Table("logout_legal_accounts_events");
			
			Id(x => x.Id).GeneratedBy.Native();
			
			Map(x => x.ErpCounterpartyId)
				.Column("counterparty_id")
				.Not.Nullable();
			
			Map(x => x.Email)
				.Column("email")
				.Not.Nullable();

			HasMany(x => x.SentData)
				.KeyColumn("event_id")
				.Cascade.AllDeleteOrphan()
				.Inverse();
		}
	}
}
