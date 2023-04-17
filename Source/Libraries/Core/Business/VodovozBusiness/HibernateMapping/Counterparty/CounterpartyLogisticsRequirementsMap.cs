using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.HibernateMapping.Counterparty
{
	public class CounterpartyLogisticsRequirementsMap : ClassMap<CounterpartyLogisticsRequirements>
	{
		public CounterpartyLogisticsRequirementsMap()
		{
			Table("counterparty_logistics_requirement");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.ForwarderRequired).Column("forwarder_required");
			Map(x => x.DocumentsRequired).Column("documents_required");
			Map(x => x.RussianDriverRequired).Column("russian_driver_required");
			Map(x => x.PassRequired).Column("pass_required");
			Map(x => x.LagrusRequired).Column("lagrus_required");

			References(x => x.Counterparty).Column("counterparty_id");
		}
	}
}
