using FluentNHibernate.Mapping;
using Vodovoz.Domain.Client;

namespace Vodovoz.HibernateMapping.Counterparty
{
    public class DeliveryPointEstimatedCoordinateMap : ClassMap<DeliveryPointEstimatedCoordinate>
    {
        public DeliveryPointEstimatedCoordinateMap()
        {
            Table("delivery_points_estimated_coordinates");

            Id(x => x.Id).Column("id").GeneratedBy.Native();

            Map(x => x.DeliveryPointId).Column("delivery_point_id");

            Map(x => x.Latitude).Column("latitude");
            Map(x => x.Longitude).Column("longitude");
            Map(x => x.RegistrationTime).Column("registration_time");
        }
    }
}
