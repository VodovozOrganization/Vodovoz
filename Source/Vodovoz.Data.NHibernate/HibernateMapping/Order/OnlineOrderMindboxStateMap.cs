using FluentNHibernate.Mapping;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Order
{
	public class OnlineOrderMindboxStateMap : ClassMap<OnlineOrderMindboxState>
	{
		public OnlineOrderMindboxStateMap()
		{
			Table("online_order_mindbox_state");

			Id(x => x.OnlineOrderId)
				.Column("online_order_id")
				.GeneratedBy.Foreign("OnlineOrder");

			HasOne(x => x.OnlineOrder)
				.Constrained();

			Map(x => x.MindboxOrderId).Column("mindbox_order_id");
			Map(x => x.MindboxTransactionId).Column("mindbox_transaction_id");
			Map(x => x.BonusesWrittenOff).Column("bonuses_written_off").Not.Nullable();
			Map(x => x.BonusesToEarn).Column("bonuses_to_earn");
			Map(x => x.MindboxStatus).Column("mindbox_status").Not.Nullable();
			Map(x => x.MindboxLastError).Column("mindbox_last_error");
			Map(x => x.MindboxLastAttemptAt).Column("mindbox_last_attempt_at");
			Map(x => x.MindboxAttempts).Column("mindbox_attempts").Not.Nullable();
			Map(x => x.MindboxLastHttpStatusCode).Column("mindbox_last_http_status_code");
			Map(x => x.MindboxLastStatus).Column("mindbox_last_status");
			Map(x => x.MindboxLastErrorId).Column("mindbox_last_error_id");
			Map(x => x.MindboxSnapshot).Column("mindbox_snapshot").Length(65535);
			Map(x => x.MindboxSnapshotUpdatedAt).Column("mindbox_snapshot_updated_at");
			Map(x => x.MindboxLastChangeAt).Column("mindbox_last_change_at");
			Map(x => x.MindboxSyncRequestedAt).Column("mindbox_sync_requested_at");
			Map(x => x.BonusesAvailable).Column("bonuses_available").Not.Nullable();
		}
	}
}
