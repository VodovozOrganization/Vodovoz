using FluentNHibernate.Mapping;
using QS.BusinessCommon.Domain;

namespace Vodovoz.HibernateMapping
{
	public class MeasurementUnitsMap : ClassMap<MeasurementUnits>
	{
		public MeasurementUnitsMap()
		{
			Table("measurement_units");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.Digits).Column("digits");
			Map(x => x.OKEI).Column("okei");
		}
	}
}

