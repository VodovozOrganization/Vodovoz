using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters
{
	public partial class IncludeExcludeLastRouteListFilterFactory
	{
		public class LastRouteListInitIncludeFilter
		{
			public EmployeeStatus[] EmployeeStatusesForInclude { get; set; }

			public CarTypeOfUse[] CarTypesOfUseForInclude { get; set; }

			public CarOwnType[] CarOwnTypesForInclude { get; set; }

			public EmployeeCategoryFilterType[] EmployeeCategoryFilterTypeInclude { get; set; }
		}
	}
}
