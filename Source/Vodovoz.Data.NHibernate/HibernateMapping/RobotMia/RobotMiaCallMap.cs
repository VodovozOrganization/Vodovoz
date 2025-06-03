using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.RobotMia;

namespace Vodovoz.Data.NHibernate.HibernateMapping.RobotMia
{
	public class RobotMiaCallMap : ClassMap<RobotMiaCall>
	{
		public RobotMiaCallMap()
		{
			Table("robot_mia_calls");

			Id(x => x.Id).GeneratedBy.Native();
			
			Map(x => x.CallGuid).Column("call_guid");
			Map(x => x.RegisteredAt).Column("registered_at");
			Map(x => x.NormalizedPhoneNumber).Column("normalized_phone_number");
			Map(x => x.CounterpartyId).Column("counterparty_id");
			Map(x => x.CreatedOrderId).Column("created_order_id");
		}
	}
}
