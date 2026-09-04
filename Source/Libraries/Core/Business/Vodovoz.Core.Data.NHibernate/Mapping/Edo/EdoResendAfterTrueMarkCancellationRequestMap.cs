using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Edo;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Edo
{
	public class EdoResendAfterTrueMarkCancellationRequestMap : ClassMap<EdoResendAfterTrueMarkCancellationRequest>
	{
		public EdoResendAfterTrueMarkCancellationRequestMap()
		{
			Table("edo_resend_after_truemark_cancellation_requests");

			OptimisticLock.Version();
			Version(x => x.Version).Column("version");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.Order).Column("order_id").Not.Nullable();
			References(x => x.OriginalEdoTask).Column("original_edo_task_id").Not.Nullable().Unique();
			References(x => x.ResendEdoRequest).Column("resend_edo_request_id").Not.Nullable().Unique();
			References(x => x.WithdrawalDocument).Column("withdrawal_document_id").Not.Nullable();
			References(x => x.CancellationDocument).Column("cancellation_document_id").Nullable();

			Map(x => x.Status).Column("status").Not.Nullable();
			Map(x => x.CancellationAttemptsCount).Column("cancellation_attempts_count").Not.Nullable();
			Map(x => x.ErrorMessage).Column("error_message").Length(500).Nullable();
			Map(x => x.CreationTime).Column("creation_time").Not.Nullable();
			Map(x => x.LastUpdateTime).Column("last_update_time").Not.Nullable();
		}
	}
}
