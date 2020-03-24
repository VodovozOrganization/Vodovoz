using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Domain.Employees
{
	public interface IEmployee : IPersonnel
	{
		EmployeeCategory Category { get; set; }
		string AndroidLogin { get; set; }
		string AndroidPassword { get; set; }
		string AndroidSessionKey { get; set; }
		string AndroidToken { get; set; }
		EmployeeStatus Status { get; set; }
		User User { get; set; }
		Subdivision Subdivision { get; set; }
		DateTime? FirstWorkDay { get; set; }
		Employee DefaultForwarder { get; set; }
		bool LargusDriver { get; set; }
		CarTypeOfUse? DriverOf { get; set; }
		float DriverSpeed { get; set; }
		short TripPriority { get; set; }
		IList<DriverDistrictPriority> Districts { get; set; }
		IList<DriverWorkSchedule> WorkDays { get; set; }
		GenericObservableList<DriverDistrictPriority> ObservableDistricts { get; }
		GenericObservableList<DriverWorkSchedule> ObservableWorkDays { get; }
		bool VisitingMaster { get; set; }

		double TimeCorrection(long timeValue);
	}
}