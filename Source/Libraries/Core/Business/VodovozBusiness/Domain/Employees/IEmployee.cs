using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Domain.Employees
{
	public interface IEmployee : IPersonnel
	{
		EmployeeCategory Category { get; set; }
		IList<ExternalApplicationUser> ExternalApplicationsUsers { get; set; }
		EmployeeStatus Status { get; set; }
		User User { get; set; }
		Subdivision Subdivision { get; set; }
		DateTime? FirstWorkDay { get; set; }
		Employee DefaultForwarder { get; set; }
		bool LargusDriver { get; set; }
		CarTypeOfUse? DriverOfCarTypeOfUse { get; set; }
		CarOwnType? DriverOfCarOwnType { get; set; }
		float DriverSpeed { get; set; }
		short TripPriority { get; set; }
		IList<DriverDistrictPrioritySet> DriverDistrictPrioritySets { get; set; }
		IList<DriverWorkScheduleSet> DriverWorkScheduleSets { get; set; }
		GenericObservableList<DriverDistrictPrioritySet> ObservableDriverDistrictPrioritySets { get; }
		GenericObservableList<DriverWorkScheduleSet> ObservableDriverWorkScheduleSets { get; }
		bool VisitingMaster { get; set; }

		double TimeCorrection(long timeValue);
	}
}
