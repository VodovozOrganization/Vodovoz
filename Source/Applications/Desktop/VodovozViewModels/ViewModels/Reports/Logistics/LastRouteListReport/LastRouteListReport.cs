using Gamma.Utilities;
using NHibernate.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Presentation.ViewModels.Reports;
using static Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters.IncludeExcludeLastRouteListFilterFactory;

namespace Vodovoz.ViewModels.ViewModels.Reports.Logistics.LastRouteListReport
{
	[Appellative(Nominative = "Отчет по последнему МЛ по водителям")]
	public partial class LastRouteListReport : IClosedXmlReport
	{
		public string TemplatePath => @".\Reports\Logistic\LastRouteListReport.xlsx";

		private IEnumerable<EmployeeStatus> _includedEmployeeStatus;
		private IEnumerable<CarTypeOfUse> _includedCarTypeOfUse;
		private IEnumerable<CarOwnType> _includedCarOwn;
		private IEnumerable<EmployeeCategoryFilterType> _includedEmployeeCategories;

		private IQueryable<LastRouteListReportRow> GetQuery(IUnitOfWork unitOfWork, bool forDriver)
		{
			var subdivisionIds = new List<int>();
			if(_filterParameters.TryGetValue("Subdivision_include", out var subdivisionIdString))
			{
				if(subdivisionIdString is string[] ids)
				{
					subdivisionIds = ids.Select(int.Parse).ToList();
				}
			}
			
			Expression<Func<Employee, bool>> employeeFilter = (employee) =>
				_includedEmployeeStatus.Contains(employee.Status)
				&& (
					(forDriver && (_includedEmployeeCategories.Contains(EmployeeCategoryFilterType.Driver) &&
					               employee.Category == EmployeeCategory.driver && employee.VisitingMaster == false))
					|| (!forDriver && (_includedEmployeeCategories.Contains(EmployeeCategoryFilterType.Forwarder) &&
					                   employee.Category == EmployeeCategory.forwarder))
					|| (forDriver && (_includedEmployeeCategories.Contains(EmployeeCategoryFilterType.VisitingMaster) && employee.VisitingMaster == true))
					|| (!_includedEmployeeCategories.Any())
				)
				&& (FiredStartDate == null || (employee.DateFired >= FiredStartDate && employee.DateFired <= FiredEndDate))
				&& (HiredStartDate == null || (employee.DateHired >= HiredStartDate && employee.DateHired <= HiredEndDate))
				&& (FirstWorkDayStartDate == null || (employee.FirstWorkDay >= FirstWorkDayStartDate && employee.FirstWorkDay <= FirstWorkDayEndDate))
				&& (CalculateStartDate == null || (employee.DateCalculated >= CalculateStartDate && employee.DateCalculated <= CalculateEndDate))
				&& (!subdivisionIds.Any() || employee.Subdivision != null && subdivisionIds.Contains(employee.Subdivision.Id));

			Expression<Func<Employee, LastRouteListNodeNode>> LastRouteListIdSelector = (employee) => new LastRouteListNodeNode
			{
				Employee = employee,
				LastRouteListId = unitOfWork.Session.Query<RouteList>()
					.Join(unitOfWork.Session.Query<CarVersion>(),
						routeList => routeList.Car.Id,
						carVersion => carVersion.Car.Id,
						(routeList, carVersion) => new { routeList, carVersion })
					.Where(x =>
						(forDriver && x.routeList.Driver.Id == employee.Id)
						|| (!forDriver && x.routeList.Forwarder.Id == employee.Id))
					.Where(x =>
						RouteList.DeliveredRouteListStatuses.Contains(x.routeList.Status)
						&& x.carVersion.StartDate <= DateTime.Now
						&& (x.carVersion.EndDate >= DateTime.Now || x.carVersion.EndDate == null)
						&& _includedCarOwn.Contains(x.carVersion.CarOwnType)
						&& _includedCarTypeOfUse.Contains(x.routeList.Car.CarModel.CarTypeOfUse))
					.OrderByDescending(x => x.routeList.Date)
					.Select(x => x.routeList.Id)
					.FirstOrDefault()
			};

			var query = unitOfWork.Session.Query<Employee>()
				.Where(employeeFilter)
				.Select(LastRouteListIdSelector)
				.Select(x => new
				{
					x.Employee,
					x.LastRouteListId,
					LastRouteListDate = unitOfWork.Session.Query<RouteList>()
						.Where(r => r.Id == x.LastRouteListId)
						.Select(r => r.Date)
						.FirstOrDefault(),
					LastRouteListCarTypeOfUse = unitOfWork.Session.Query<RouteList>()
						.Where(r => r.Id == x.LastRouteListId)
						.Select(r => r.Car.CarModel.CarTypeOfUse)
						.FirstOrDefault(),
					LastRouteListCarsOwn = unitOfWork.Session.Query<RouteList>()
						.Join(unitOfWork.Session.Query<CarVersion>(),
							routeList => routeList.Car.Id,
							carVersion => carVersion.Car.Id,
							(routeList, carVersion) => new { routeList, carVersion })
						.Where(r => r.routeList.Id == x.LastRouteListId
							&& r.carVersion.StartDate <= DateTime.Now
							&& (r.carVersion.EndDate >= DateTime.Now || r.carVersion.EndDate == null))
						.Select(r => r.carVersion.CarOwnType)
						.FirstOrDefault()
				})
				.Where(x => LastRouteListStartDate == null
					|| (x.LastRouteListDate >= LastRouteListStartDate && x.LastRouteListDate < LastRouteListEndDate))

				.Select(x => new LastRouteListReportRow
				{
					DriverLastName = x.Employee.LastName,
					DriverFirstName = x.Employee.Name,
					DriverPatronymic = x.Employee.Patronymic,
					DriverStatus = x.Employee.Status,
					FirstWorkDay = x.Employee.FirstWorkDay,
					EmployeeCategory = x.Employee.Category,
					DateHired = x.Employee.DateHired,
					DateFired = x.Employee.DateFired,
					DateCalculated = x.Employee.DateCalculated,
					LastRouteListId = x.LastRouteListId,
					LastClosedRouteListDate = x.LastRouteListDate,
					VisitinMaster = x.Employee.VisitingMaster,
					CarTypeOfUse = x.LastRouteListCarTypeOfUse,
					CarsOwn = x.LastRouteListCarsOwn
				});

			return query;
		}

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

			_includedEmployeeCategories = filterViewModel
				.GetFilter<IncludeExcludeEnumFilter<EmployeeCategoryFilterType>>()
				.GetIncluded();

			_filterParameters = filterViewModel.GetReportParametersSet(out var sb);
			
			List<LastRouteListReportRow> driverRows = new List<LastRouteListReportRow>();

			if(_includedEmployeeCategories.Contains(EmployeeCategoryFilterType.Driver)
				|| _includedEmployeeCategories.Contains(EmployeeCategoryFilterType.VisitingMaster))
			{
				driverRows = await GetQuery(unitOfWork, true)
					.ToListAsync(cancellationToken);
			}

			List<LastRouteListReportRow> forwarderRows = new List<LastRouteListReportRow>();

			if(_includedEmployeeCategories.Contains(EmployeeCategoryFilterType.Forwarder))
			{

				forwarderRows = await GetQuery(unitOfWork, false)
					.ToListAsync(cancellationToken);
			}

			var reportRows = driverRows.Concat(forwarderRows).OrderByDescending(x => DateTime.Now - x.LastClosedRouteListDate).ToList();

			var rowNumber = 1;
			reportRows.ForEach(x => x.RowNum = rowNumber++);

			Rows = reportRows;
		}		

		public string SelectedEmployeeStatus => string.Join(", ", _includedEmployeeStatus.Select(x => x.GetEnumTitle()));

		public string SelectedCarsOwn => string.Join(", ", _includedCarOwn.Select(x => x.GetEnumTitle()));

		public string SelectedCarTypeOfUse => string.Join(", ", _includedCarTypeOfUse.Select(x => x.GetEnumTitle()));

		public string SelectedEmployeeCategories => string.Join(", ", _includedEmployeeCategories.Select(x => x.GetEnumTitle()));

		public DateTime? FiredStartDate { get; internal set; }
		public DateTime? FiredEndDate { get; internal set; }
		public DateTime? FirstWorkDayStartDate { get; internal set; }
		public DateTime? FirstWorkDayEndDate { get; internal set; }
		public DateTime? LastRouteListStartDate { get; internal set; }
		public DateTime? LastRouteListEndDate { get; internal set; }
		public DateTime? CalculateStartDate { get; internal set; }
		public DateTime? CalculateEndDate { get; internal set; }
		public DateTime? HiredStartDate { get; internal set; }
		public DateTime? HiredEndDate { get; internal set; }

		public List<LastRouteListReportRow> Rows = new List<LastRouteListReportRow>();
		
		private Dictionary<string, object> _filterParameters;
	}
}
