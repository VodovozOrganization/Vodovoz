using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Clients;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Counterparty
{
	public class CounterpartyMap : ClassMap<CounterpartyEntity>
	{
		public CounterpartyMap()
		{
			Table("counterparty");

			HibernateMapping.DefaultAccess.CamelCaseField(Prefix.Underscore);

			Id(x => x.Id)
				.Column("id")
				.GeneratedBy.Native();

			Map(x => x.OrderStatusForSendingUpd)
				.Column("order_status_for_sending_upd");

			Map(x => x.ConsentForEdoStatus)
				.Column("consent_for_edo_status");

			Map(x => x.IsNewEdoProcessing)
				.Column("is_new_edo_processing");


			References(x => x.WorksThroughOrganization)
				.Column("works_through_organization_id");
				
		}
	}
}
