using Gamma.Utilities;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Presentation.ViewModels.Common;
using static Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters.IncludeExcludeLastRouteListFilterFactory;

namespace Vodovoz.ViewModels.ViewModels.Reports.Logistics.LastRouteListReport
{
	public class LastRouteListReport
	{
		public string SelectedEmployeeStatus => string.Join(", ", _includedEmployeeStatus.Select(x => x.GetEnumTitle()));
		public string SelectedCarsOwn => string.Join(", ", _includedCarOwn.Select(x => x.GetEnumTitle()));
		public string SelectedCarTypeOfUse => string.Join(", ", _includedCarTypeOfUse.Select(x => x.GetEnumTitle()));
		public string SelectedVisitingMaster => _includedVisitingMaster.GetEnumTitle();

		public List<LastRouteListReportRow> Rows = new List<LastRouteListReportRow>();
		private IEnumerable<EmployeeStatus> _includedEmployeeStatus;
		private IEnumerable<CarTypeOfUse> _includedCarTypeOfUse;
		private IEnumerable<CarOwnType> _includedCarOwn;
		private VisitingMasterFilterType _includedVisitingMaster;

		public async Task GenerateRows(
			IUnitOfWork unitOfWork,
			IncludeExludeFiltersViewModel filterViewModel,
			CancellationToken cancellationToken)
		{
			_includedEmployeeStatus = filterViewModel
				.GetFilter<IncludeExcludeEnumFilter<EmployeeStatus>>()
				.GetIncluded();

			_includedCarTypeOfUse = filterViewModel
				.GetFilter<IncludeExcludeEnumFilter<CarTypeOfUse>>()
				.GetIncluded();

			_includedCarOwn = filterViewModel
				.GetFilter<IncludeExcludeEnumFilter<CarOwnType>>()
				.GetIncluded();

			_includedVisitingMaster = filterViewModel
				.GetFilter<IncludeExcludeEnumFilter<VisitingMasterFilterType>>()
				.GetIncluded()
				.FirstOrDefault();

			var rows =
				from employee in unitOfWork.Session.Query<Employee>()
				where
					employee.Category == EmployeeCategory.driver
					&& _includedEmployeeStatus.Contains(employee.Status)
					&& (
							_includedVisitingMaster == VisitingMasterFilterType.IncludeVisitingMaster
							|| (_includedVisitingMaster == VisitingMasterFilterType.ExcludeVisitingMaster && employee.VisitingMaster == false)
							|| (_includedVisitingMaster == VisitingMasterFilterType.OnlyVisitingMaster && employee.VisitingMaster == true)
						)

				let lastRouteListId =
				(
					from routeList in unitOfWork.Session.Query<RouteList>()
					join carVersion in unitOfWork.Session.Query<CarVersion>() on routeList.Car.Id equals carVersion.Car.Id
					where
						routeList.Driver.Id == employee.Id
						&& routeList.Status == RouteListStatus.Closed
						&& carVersion.StartDate <= DateTime.Now
						&& (carVersion.EndDate >= DateTime.Now || carVersion.EndDate == null)
						&& _includedCarOwn.Contains(carVersion.CarOwnType)
						&& _includedCarTypeOfUse.Contains(routeList.Car.CarModel.CarTypeOfUse)

					orderby routeList.Date descending
					select routeList.Id
				).FirstOrDefault()

				let lastRouteListDate =
				(
					from routeList in unitOfWork.Session.Query<RouteList>()
					where routeList.Id == lastRouteListId
					select routeList.Date
				).FirstOrDefault()

				let lastRouteListCarTypeOfUse =
				(
					from routeList in unitOfWork.Session.Query<RouteList>()
					where routeList.Id == lastRouteListId
					select routeList.Car.CarModel.CarTypeOfUse
				).FirstOrDefault()

				let lastRouteListCarsOwn =
				(
					from routeList in unitOfWork.Session.Query<RouteList>()
					join carVersion in unitOfWork.Session.Query<CarVersion>() on routeList.Car.Id equals carVersion.Car.Id
					where routeList.Id == lastRouteListId
						&& carVersion.StartDate <= DateTime.Now
						&& (carVersion.EndDate >= DateTime.Now || carVersion.EndDate == null)
					select carVersion.CarOwnType
				).FirstOrDefault()

				where lastRouteListId != null

				orderby DateTime.Now - lastRouteListDate descending

				select new LastRouteListReportRow
				{
					DriverLastName = employee.LastName,
					DriverFirstName = employee.Name,
					DriverPatronymic = employee.Patronymic,
					DriverStatus = employee.Status,
					FirstWorkDay = employee.FirstWorkDay,
					DateHired = employee.DateHired,
					DateFired = employee.DateFired,
					DateCalculated = employee.DateCalculated,
					LastRouteListId = lastRouteListId,
					LastClosedRouteListDate = lastRouteListDate,
					VisitinMaster = employee.VisitingMaster,
					CarTypeOfUse = lastRouteListCarTypeOfUse,
					CarsOwn = lastRouteListCarsOwn
				};

			var result = (await rows.ToListAsync(cancellationToken));

			var rowNumber = 1;
			result.ForEach(x => x.RowNum = rowNumber++);

			Rows = result;
		}
	}
}
