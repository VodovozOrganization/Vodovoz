using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping.Logistic
{
	public class FastDeliveryMaxDistanceParameterVersionMap : ClassMap<FastDeliveryMaxDistanceParameterVersion>
	{
		public FastDeliveryMaxDistanceParameterVersionMap()
		{
			Table("fast_delivery_max_distance_parameter_version");

			Id(x => x.Id, "id").GeneratedBy.Native();

			Map(x => x.StartDate, "start_date");
			Map(x => x.EndDate, "end_date");
			Map(x => x.Value, "value");
		}
	}
}
