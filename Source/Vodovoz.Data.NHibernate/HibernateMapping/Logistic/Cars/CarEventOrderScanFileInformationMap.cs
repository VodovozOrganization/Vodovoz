using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic.Cars
{
	public class CarEventOrderScanFileInformationMap : ClassMap<CarEventOrderScanFileInformation>
	{
		public CarEventOrderScanFileInformationMap()
		{
			Table("car_event_order_scan_files_informations");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			Map(x => x.CarEventId).Column("car_event_id");
			Map(x => x.FileName).Column("file_name");
		}
	}
}
