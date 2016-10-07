using System;
using Vodovoz.Domain.Logistic;
using FluentNHibernate.Mapping;

namespace Vodovoz.HMap
{
	public class GazTicketMap : ClassMap<GazTicket>
	{
		public GazTicketMap ()
		{
			Table("gaz_ticket");

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			Map(x => x.Name).Column ("name");
			Map(x => x.Liters).Column("liters");
			References(x => x.FuelType).Column("fuel_type_id");
		}
	}
}

