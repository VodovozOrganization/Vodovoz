using FluentNHibernate.Mapping;
using Vodovoz.Domain.Common;

namespace Vodovoz.HibernateMapping.Common
{
    public class MeasurementUnitMap : ClassMap<MeasurementUnit>
	{
		public MeasurementUnitMap()
		{
			Table("measurement_units");
			Not.LazyLoad();

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.Name).Column("name");
			Map(x => x.Digits).Column("digits");
			Map(x => x.OKEI).Column("okei");
			Map(x => x.BitrixName).Column("bitrix_name");
		}
	}
}
