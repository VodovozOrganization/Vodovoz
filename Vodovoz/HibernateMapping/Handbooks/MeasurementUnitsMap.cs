using System;
using FluentNHibernate.Mapping;
using Vodovoz.Domain;

namespace Vodovoz.HMap
{
	public class MeasurementUnitsMap : ClassMap<MeasurementUnits>
	{
		public MeasurementUnitsMap ()
		{
			Table("measurement_units");
			Not.LazyLoad ();

			Id(x => x.Id).Column ("id").GeneratedBy.Native();
			Map(x => x.Name).Column ("name");
			Map (x => x.Digits).Column ("digits");
			Map (x => x.OKEI).Column ("okei");
		}
	}
}

