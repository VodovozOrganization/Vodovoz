using System;
using Vodovoz.Domain.Logistic;
using FluentNHibernate.Mapping;

namespace Vodovoz.HMap
{
	public class CarMap : ClassMap<Car>
	{
		public CarMap ()
		{
			Table("cars");

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			Map(x => x.Model).Column ("model");
			Map(x => x.RegistrationNumber).Column ("reg_number");
			Map(x => x.FuelConsumption).Column ("fuel_consumption");
			Map(x => x.IsArchive).Column ("is_archive");
			Map (x => x.Photo).Column ("photo").CustomSqlType ("BinaryBlob").LazyLoad ();
			References (x => x.Driver).Column ("driver_id");
			References (x => x.FuelType).Column ("fuel_type_id");
		}
	}
}

