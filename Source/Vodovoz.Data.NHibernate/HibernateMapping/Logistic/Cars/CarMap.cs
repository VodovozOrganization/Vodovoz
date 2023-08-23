using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic.Cars
{
	public class CarMap : ClassMap<Car>
	{
		public CarMap()
		{
			Table("cars");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.RegistrationNumber).Column("reg_number");
			Map(x => x.IsArchive).Column("is_archive");
			Map(x => x.Photo).Column("photo").CustomSqlType("BinaryBlob").LazyLoad();
			Map(x => x.MinBottles).Column("min_bottles");
			Map(x => x.MaxBottles).Column("max_bottles");
			Map(x => x.MinBottlesFromAddress).Column("min_bottles_from_address");
			Map(x => x.MaxBottlesFromAddress).Column("max_bottles_from_address");
			Map(x => x.VIN).Column("VIN");
			Map(x => x.ManufactureYear).Column("manufacture_year");
			Map(x => x.MotorNumber).Column("motor_number");
			Map(x => x.ChassisNumber).Column("chassis_number");
			Map(x => x.Carcase).Column("carcase");
			Map(x => x.Color).Column("color");
			Map(x => x.DocSeries).Column("doc_series");
			Map(x => x.DocNumber).Column("doc_number");
			Map(x => x.DocIssuedOrg).Column("doc_issued_org");
			Map(x => x.DocIssuedDate).Column("doc_issued_date");
			Map(x => x.FuelCardNumber).Column("fuel_card_number");
			Map(x => x.DocPTSNumber).Column("doc_pts_num");
			Map(x => x.DocPTSSeries).Column("doc_pts_series");
			Map(x => x.OrderNumber).Column("car_order_number");

			References(x => x.Driver).Column("driver_id");
			References(x => x.FuelType).Column("fuel_type_id");
			References(x => x.CarModel).Column("model_id");

			HasMany(x => x.Attachments).Cascade.AllDeleteOrphan().Inverse().KeyColumn("car_id");
			HasMany(x => x.CarVersions).Cascade.AllDeleteOrphan().Inverse().KeyColumn("car_id")
				.OrderBy("start_date DESC");

			HasMany(x => x.OdometerReadings).Cascade.AllDeleteOrphan().Inverse().KeyColumn("car_id")
				.OrderBy("start_date DESC");

			HasManyToMany(x => x.GeographicGroups)
				.Table("geo_groups_to_entities")
				.ParentKeyColumn("car_id")
				.ChildKeyColumn("geo_group_id")
				.LazyLoad();
		}
	}
}
