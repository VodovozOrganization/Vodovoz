using Gamma.Utilities;
using System;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.ViewModels.ViewModels.Reports.Logistics.LastRouteListReport
{
	public class LastRouteListReportRow
	{
		public int RowNum { get; internal set; }
		public string DriverFio => string.Join(" ", DriverLastName, DriverFirstName, DriverPatronymic);
		public string DriverLastName { get; internal set; }
		public string DriverFirstName { get; internal set; }
		public string DriverPatronymic { get; internal set; }
		public EmployeeStatus DriverStatus { get; internal set; }
		public string DriverStatusString => DriverStatus.GetEnumTitle();
		public DateTime? FirstWorkDay { get; internal set; }
		public DateTime? DateHired { get; internal set; }
		public DateTime? DateFired { get; internal set; }
		public DateTime? DateCalculated { get; internal set; }
		public int? LastRouteListId { get; internal set; }
		public DateTime? LastClosedRouteListDate { get; internal set; }
		public int DaysCountFromLastClosedRouteList => (DateTime.Today - (LastClosedRouteListDate ?? DateTime.Today)).Days;
		public CarTypeOfUse? CarTypeOfUse { get; internal set; }
		public CarOwnType? CarsOwn { get; internal set; }
		public string CarTypeOfUseString => CarTypeOfUse?.GetEnumTitle() ?? "";
		public string CarsOwnString => CarsOwn?.GetEnumTitle() ?? "";
		public bool? VisitinMaster { get; internal set; }
		public EmployeeCategory? EmployeeCategory { get; internal set; }
		public string EmployeeCategoryString =>
			VisitinMaster.HasValue
			? VisitinMaster.Value
				? "Выездной мастер"
				: EmployeeCategory?.GetEnumTitle()
			: "";
	}
}
