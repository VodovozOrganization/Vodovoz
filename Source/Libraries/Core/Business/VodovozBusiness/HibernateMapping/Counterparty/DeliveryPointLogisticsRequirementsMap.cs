using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.HibernateMapping.Counterparty
{
	public class DeliveryPointLogisticsRequirementsMap : ClassMap<DeliveryPointLogisticsRequirements>
	{
		public DeliveryPointLogisticsRequirementsMap()
		{
			Table("delivery_point_logistics_requirement");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.ForwarderRequired).Column("forwarder_required");
			References(x => x.DocumentsRequired).Column("documents_required");
			References(x => x.RussianDriverRequired).Column("russian_driver_required");
			References(x => x.PassRequired).Column("pass_required");
			References(x => x.LagrusRequired).Column("lagrus_required");
			References(x => x.DeliveryPoint).Column("delivery_point_id");
		}
	}
}
