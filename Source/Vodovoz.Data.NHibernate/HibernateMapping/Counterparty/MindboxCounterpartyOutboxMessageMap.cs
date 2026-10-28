using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client.Mindbox;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Counterparty
{
	public class MindboxCounterpartyOutboxMessageMap : ClassMap<MindboxCounterpartyOutboxMessage>
	{
		public MindboxCounterpartyOutboxMessageMap()
		{
			Table("mindbox_counterparty_outbox");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.Counterparty).Column("counterparty_id").Not.Nullable();
			Map(x => x.TransactionId).Column("transaction_id").Not.Nullable();
			Map(x => x.Status).Column("status").Not.Nullable();
			Map(x => x.Attempts).Column("attempts").Not.Nullable();
			Map(x => x.LastAttemptAt).Column("last_attempt_at");
			Map(x => x.LastError).Column("last_error").Length(2000);
			Map(x => x.LastHttpStatusCode).Column("last_http_status_code");
			Map(x => x.LastMindboxStatus).Column("last_mindbox_status").Length(50);
			Map(x => x.LastErrorId).Column("last_error_id").Length(100);
			Map(x => x.LastAttemptDurationMs).Column("last_attempt_duration_ms");
			Map(x => x.CreatedAt).Column("created_at").Not.Nullable();
			Map(x => x.NextAttemptAt).Column("next_attempt_at");
			Map(x => x.RetryUntil).Column("retry_until").Not.Nullable();
			Map(x => x.SentAt).Column("sent_at");
		}
	}
}
