using FluentNHibernate.Mapping;
using NHibernate.Type;
using Vodovoz.Core.Domain.Clients.Mindbox;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Counterparty
{
	public class MindboxCounterpartyStateMap : ClassMap<MindboxCounterpartyState>
	{
		public MindboxCounterpartyStateMap()
		{
			Table("mindbox_counterparty_state");

			Id(x => x.CounterpartyId).Column("counterparty_id").GeneratedBy.Assigned();

			Map(x => x.Status)
				.Column("status")
				.CustomType<EnumStringType<MindboxCounterpartyOutboxStatus>>()
				.Not.Nullable();
			Map(x => x.TransactionId).Column("transaction_id");
			Map(x => x.Attempts).Column("attempts").Not.Nullable();
			Map(x => x.LastAttemptAt).Column("last_attempt_at");
			Map(x => x.LastError).Column("last_error").Length(2000);
			Map(x => x.LastHttpStatusCode).Column("last_http_status_code");
			Map(x => x.LastMindboxStatus).Column("last_mindbox_status").Length(50);
			Map(x => x.LastErrorId).Column("last_error_id").Length(100);
			Map(x => x.LastAttemptDurationMs).Column("last_attempt_duration_ms");
			Map(x => x.SentAt).Column("sent_at");
			Map(x => x.CreatedAt).Column("created_at");
			Map(x => x.UpdatedAt).Column("updated_at");
		}
	}
}
