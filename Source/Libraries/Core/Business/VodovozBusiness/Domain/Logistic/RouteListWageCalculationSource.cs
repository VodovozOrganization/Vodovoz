using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.WageCalculation.CalculationServices.RouteList;

namespace Vodovoz.Domain.Logistic
{
	public class RouteListWageCalculationSource : IRouteListWageCalculationSource
	{
		readonly RouteList rl;
		readonly EmployeeCategory employeeCategory;

		public RouteListWageCalculationSource(RouteList rl, EmployeeCategory employeeCategory)
		{
			this.employeeCategory = employeeCategory;
			this.rl = rl ?? throw new ArgumentNullException(nameof(rl));
		}

		#region IRouteListWageCalculationSource implementation

		public decimal TotalSum => rl.Total;

		public bool HasAnyCompletedAddress => rl.Addresses.Any(x => x.Status == RouteListItemStatus.Completed);

		public EmployeeCategory EmployeeCategory => employeeCategory;

		public IEnumerable<IRouteListItemWageCalculationSource> ItemSources {
			get {
				switch(employeeCategory) {
					case EmployeeCategory.driver:
						return rl.Addresses.Select(a => a.DriverWageCalculationSrc);
					case EmployeeCategory.forwarder:
						return rl.Addresses.Select(a => a.ForwarderWageCalculationSrc);
					case EmployeeCategory.office:
					default:
						throw new NotSupportedException();
				}
			}
		}

		public bool DriverOfOurCar => rl.GetCarVersion.CarOwnType == CarOwnType.Company;

		public bool IsRaskatCar => rl.GetCarVersion.IsRaskat;

		public CarTypeOfUse CarTypeOfUse => rl.Car?.CarModel?.CarTypeOfUse
			?? throw new InvalidOperationException("Модель автомобиля в МЛ должна быть заполнена");

		public DateTime RouteListDate => rl.Date;

		public bool IsTruck => rl.Car.CarModel.CarTypeOfUse == CarTypeOfUse.Truck;

		public int RouteListId => rl.Id;

		public decimal FixedWage {
			get {
				switch(EmployeeCategory) {
					case EmployeeCategory.driver:
						return rl.FixedDriverWage;
					case EmployeeCategory.forwarder:
						return rl.FixedForwarderWage;
					case EmployeeCategory.office:
					default:
						throw new NotSupportedException();
				}
			}
		}

		#endregion IRouteListWageCalculationSource implementation
	}
}
