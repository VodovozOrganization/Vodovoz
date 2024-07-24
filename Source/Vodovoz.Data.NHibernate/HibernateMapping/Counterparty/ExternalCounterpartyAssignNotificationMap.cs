using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Counterparty
{
	public class ExternalCounterpartyAssignNotificationMap : ClassMap<ExternalCounterpartyAssignNotification>
	{
		public ExternalCounterpartyAssignNotificationMap()
		{
			Table(ExternalCounterpartyAssignNotification.TableName);

			Id(x => x.Id).GeneratedBy.Native();

			Map(x => x.HttpCode).Column("http_code");
			Map(x => x.CreationDate).Column("creation_date").ReadOnly();
			Map(x => x.SentDate).Column("sent_date");

			References(x => x.ExternalCounterparty).Column("external_counterparty_id");
		}
	}
}
