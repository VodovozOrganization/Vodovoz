using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Counterparty
{
	public class DeletedExternalCounterpartyNotificationMap : ClassMap<DeletedExternalCounterpartyNotification>
	{
		public DeletedExternalCounterpartyNotificationMap()
		{
			Table("deleted_external_counterparty_notifications");

			Id(x => x.Id).GeneratedBy.Native();

			Map(x => x.HttpCode).Column("http_code");
			Map(x => x.CreationDate).Column("creation_date").ReadOnly();
			Map(x => x.SentDate).Column("sent_date");
			Map(x => x.CounterpartyFrom).Column("counterparty_from");
			Map(x => x.ExternalCounterpartyId).Column("external_counterparty_id");
			Map(x => x.ErpCounterpartyId).Column("counterparty_id");
		}
	}
}
