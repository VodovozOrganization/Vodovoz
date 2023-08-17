using FluentNHibernate.Mapping;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.HibernateMapping.Logistic
{
	public class CarEventMap : ClassMap<CarEvent>
	{
		public CarEventMap()
		{
			Table("car_events");

			Id(x => x.Id).Column("id").GeneratedBy.Native();
			Map(x => x.CreateDate).Column("create_date").ReadOnly();
			Map(x => x.StartDate).Column("start_date");
			Map(x => x.EndDate).Column("end_date");
			Map(x => x.Comment).Column("comment");
			Map(x => x.Foundation).Column("foundation");
			Map(x => x.DoNotShowInOperation).Column("donot_show_in_operation");
			Map(x => x.CompensationFromInsuranceByCourt).Column("compensation_from_insurance_by_court");
			Map(x => x.RepairCost).Column("repair_cost");

			References(x => x.CarEventType).Column("car_event_type_id");
			References(x => x.Author).Column("author_id");
			References(x => x.Car).Column("car_id");
			References(x => x.Driver).Column("driver_id");
			References(x => x.OriginalCarEvent).Column("original_car_event_id");

			HasManyToMany<Fine>(x => x.Fines)
				.Table("car_event_fines")
				.ParentKeyColumn("car_event_id")
				.ChildKeyColumn("fine_id")
				.LazyLoad();
		}
	}
}
