using FluentNHibernate.Mapping;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Data.NHibernate.HibernateMapping.Logistic.Cars
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
			Map(x => x.Odometer).Column("odometer");
			Map(x => x.CarTechnicalCheckupEndingDate).Column("car_technical_checkup_ending_date");
			Map(x => x.IsWriteOffDocumentNotRequired).Column("is_write_off_document_not_required");

			Map(x => x.ActualFuelBalance).Column("actual_fuel_balance");
			Map(x => x.CurrentFuelBalance).Column("current_fuel_balance");
			Map(x => x.SubstractionFuelBalance).Column("substraction_fuel_balance");
			Map(x => x.FuelCost).Column("fuel_cost");

			References(x => x.CarEventType).Column("car_event_type_id");
			References(x => x.Author).Column("author_id");
			References(x => x.Car).Column("car_id");
			References(x => x.Driver).Column("driver_id");
			References(x => x.OriginalCarEvent).Column("original_car_event_id");
			References(x => x.WriteOffDocument).Column("write_off_document_id");
			References(x => x.CalibrationFuelOperation).Column("calibration_fuel_operation_id").Cascade.All();

			HasManyToMany(x => x.Fines)
				.Table("car_event_fines")
				.ParentKeyColumn("car_event_id")
				.ChildKeyColumn("fine_id")
				.LazyLoad();
		}
	}
}
