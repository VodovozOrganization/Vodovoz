using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;

namespace Vodovoz.Core.Data.NHibernate.Clients
{
	public class DeliveryPointMap : ClassMap<DeliveryPointEntity>
	{
		public DeliveryPointMap()
		{
			Table("delivery_points");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CompiledAddress).Column("compiled_address");
			Map(x => x.ShortAddress).Column("compiled_address_short");
			Map(x => x.MinutesToUnload).Column("minutes_to_unload");
			Map(x => x.Floor).Column("floor");
			Map(x => x.EntranceType).Column("entrance_type");
			Map(x => x.Entrance).Column("entrance");
			Map(x => x.City).Column("city");
			Map(x => x.CityFiasGuid).Column("city_fias_guid");
			Map(x => x.LocalityType).Column("locality_type");
			Map(x => x.LocalityTypeShort).Column("locality_type_short");
			Map(x => x.CityDistrict).Column("city_district");
			Map(x => x.Street).Column("street");
			Map(x => x.StreetFiasGuid).Column("street_fias_guid");
			Map(x => x.StreetDistrict).Column("street_district");
			Map(x => x.StreetType).Column("street_type");
			Map(x => x.StreetTypeShort).Column("street_type_short");
			Map(x => x.Building).Column("building");
			Map(x => x.BuildingFiasGuid).Column("building_fias_guid");
			Map(x => x.RoomType).Column("room_type");
			Map(x => x.Room).Column("room");
			Map(x => x.Letter).Column("letter");
			Map(x => x.Placement).Column("placement");
			Map(x => x.Comment).Column("comment");
			Map(x => x.FoundOnOsm).Column("found_on_osm");
			Map(x => x.ManualCoordinates).Column("manual_coordinates");
			Map(x => x.IsFixedInOsm).Column("is_fixed_in_osm");
			Map(x => x.Latitude).Column("latitude");
			Map(x => x.Longitude).Column("longitude");
			Map(x => x.IsActive).Column("is_active");
			Map(x => x.Address1c).Column("address_1c");
			Map(x => x.Code1c).Column("code1c");
			Map(x => x.BottleReserv).Column("bottle_reserv");
			Map(x => x.DistanceFromBaseMeters).Column("distance_from_center_meters");
			Map(x => x.HaveResidue).Column("have_residue");
			Map(x => x.AlwaysFreeDelivery).Column("always_free_delivery");
			Map(x => x.OnlineComment).Column("online_comment");
			Map(x => x.Intercom).Column("intercom");
			Map(x => x.KPP).Column("KPP");
			Map(x => x.Organization).Column("organization");
			Map(x => x.BuildingFromOnline).Column("building_from_online");

			References(x => x.Counterparty).Column("counterparty_id");
			HasMany(x => x.Phones)
				.KeyColumn("delivery_point_id")
				.Cascade.AllDeleteOrphan()
				.Inverse()
				.LazyLoad();
		}
	}
}
