using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.WageCalculation;

namespace Vodovoz.Domain.Employees
{
	public interface IEmployee : IPersonnel
	{
		EmployeeCategory Category { get; set; }
		string AndroidLogin { get; set; }
		string AndroidPassword { get; set; }
		string AndroidSessionKey { get; set; }
		string AndroidToken { get; set; }
		bool IsFired { get; set; }
		User User { get; set; }
		Subdivision Subdivision { get; set; }
		DateTime? FirstWorkDay { get; set; }
		DeliveryDaySchedule DefaultDaySheldule { get; set; }
		Employee DefaultForwarder { get; set; }
		bool LargusDriver { get; set; }
		CarTypeOfUse? DriverOf { get; set; }
		float DriverSpeed { get; set; }
		short TripPriority { get; set; }
		IList<DriverDistrictPriority> Districts { get; set; }
		GenericObservableList<DriverDistrictPriority> ObservableDistricts { get; }
		bool VisitingMaster { get; set; }

		double TimeCorrection(long timeValue);
	}
}