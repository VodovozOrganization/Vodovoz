using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Core.Data.NHibernate.Mapping.Clients
{
	public class ConnectedCustomerMap : ClassMap<ConnectedCustomer>
	{
		public ConnectedCustomerMap()
		{
			Table("connected_customers");

			Id(x => x.Id).GeneratedBy.Native();

			Map(x => x.LegalCounterpartyId).Column("legal_counterparty_id");
			Map(x => x.NaturalCounterpartyPhoneId).Column("natural_counterparty_phone_id");
			Map(x => x.ConnectState).Column("connect_state");
			Map(x => x.BlockingReason).Column("blocking_reason");
		}
	}
}
