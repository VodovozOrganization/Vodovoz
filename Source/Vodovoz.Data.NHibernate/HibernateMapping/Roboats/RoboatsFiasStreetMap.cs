using FluentNHibernate.Mapping;
using Vodovoz.Domain.Roboats;

namespace Vodovoz.HibernateMapping.Roboats
{
	public class RoboatsFiasStreetMap : ClassMap<RoboatsFiasStreet>
	{
		public RoboatsFiasStreetMap()
		{
			Table("roboats_fias_streets");

			Id(x => x.Id).GeneratedBy.Native();

			References(x => x.RoboatsAddress).Column("roboats_id").Fetch.Join();
			Map(x => x.FiasStreetGuid).Column("street_fias_guid");
		}
	}
}
