﻿using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.HibernateMapping.Logistic.Cars
{
	public class CarVersionMap : ClassMap<CarVersion>
	{
		public CarVersionMap()
		{
			Table("car_versions");
			
			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			
			Map(x => x.StartDate).Column("start_date");
			Map(x => x.EndDate).Column("end_date");
			Map(x => x.CarOwnType).Column("car_own_type").CustomType<CarOwnTypeStringType>();

			References(x => x.Car).Column("car_id");
		}
	}
}
