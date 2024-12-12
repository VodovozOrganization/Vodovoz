using FluentNHibernate.Mapping;
using VodovozInfrastructure.Configuration;

namespace Vodovoz.Data.NHibernate.HibernateMapping
{
	public class InstanceMailingConfigurationMap : ClassMap<InstanceMailingConfiguration>
	{
		public InstanceMailingConfigurationMap()
		{
			Table("current_mailing_instance_settings");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.MessageBrokerHost).Column("message_broker_host");
			Map(x => x.MessageBrokerVirtualHost).Column("message_broker_virtual_host");
			Map(x => x.MessageBrokerUsername).Column("message_broker_username");
			Map(x => x.MessageBrokerPassword).Column("message_broker_password");
			Map(x => x.EmailSendExchange).Column("email_send_exchange");
			Map(x => x.EmailSendKey).Column("email_send_key");
			Map(x => x.Port).Column("port");
		}
	}
}
