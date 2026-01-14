using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.Project.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Cash;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Subdivisions;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.EntityRepositories.Employees;

namespace Vodovoz.Infrastructure.Persistance.Employees
{
	internal sealed class EmployeeRepository : IEmployeeRepository
	{
		public EmployeeRepository()
		{

		}

		public Employee GetEmployeeForCurrentUser(IUnitOfWork unitOfWork)
		{
			if(unitOfWork is null)
			{
				throw new ArgumentNullException(nameof(unitOfWork));
			}

			User userAlias = null;

			var userId = ServicesConfig.UserService.CurrentUserId;

			return unitOfWork.Session.QueryOver<Employee>()
				.JoinAlias(e => e.User, () => userAlias)
				.Where(() => userAlias.Id == userId)
				.SingleOrDefault();
		}

		public Employee GetEmployeeByAuthKey(IUnitOfWork uow, string authKey)
		{
			Employee employeeAlias = null;
			ExternalApplicationUser externalAppUserAlias = null;

			return uow.Session.QueryOver(() => employeeAlias)
				.JoinAlias(() => employeeAlias.ExternalApplicationsUsers, () => externalAppUserAlias)
				.Where(() => externalAppUserAlias.SessionKey == authKey)
				.Where(() => employeeAlias.Status != EmployeeStatus.IsFired)
				.SingleOrDefault();
		}

		public Employee GetEmployeeByAndroidLogin(
			IUnitOfWork uow,
			string login,
			ExternalApplicationType externalApplicationType = ExternalApplicationType.DriverApp)
		{
			ExternalApplicationUser externalAppUserAlias = null;

			return uow.Session.QueryOver<Employee>()
				.JoinAlias(e => e.ExternalApplicationsUsers, () => externalAppUserAlias)
				.Where(e => externalAppUserAlias.Login == login)
				.And(e => externalAppUserAlias.ExternalApplicationType == externalApplicationType)
				.SingleOrDefault();
		}

		public IList<Employee> GetEmployeesForUser(IUnitOfWork uow, int userId)
		{
			User userAlias = null;

			return uow.Session.QueryOver<Employee>()
				.JoinAlias(e => e.User, () => userAlias)
				.Where(() => userAlias.Id == userId)
				.List();
		}

		public IList<Employee> GetWorkingDriversAtDay(IUnitOfWork uow, DateTime date)
		{
			Employee employeeAlias = null;
			DriverWorkScheduleSet driverWorkScheduleSetAlias = null;
			DriverWorkSchedule driverWorkScheduleAlias = null;
			DeliveryDaySchedule deliveryDayScheduleAlias = null;
			DeliveryShift shiftAlias = null;

			return uow.Session.QueryOver(() => employeeAlias)
				.Inner.JoinAlias(() => employeeAlias.DriverWorkScheduleSets, () => driverWorkScheduleSetAlias)
				.Inner.JoinAlias(() => driverWorkScheduleSetAlias.DriverWorkSchedules, () => driverWorkScheduleAlias)
				.Inner.JoinAlias(() => driverWorkScheduleAlias.DaySchedule, () => deliveryDayScheduleAlias)
				.Inner.JoinAlias(() => deliveryDayScheduleAlias.Shifts, () => shiftAlias)
				.Where(() =>
					employeeAlias.Status == EmployeeStatus.IsWorking
					&& (int)driverWorkScheduleAlias.WeekDay == (int)date.DayOfWeek
					&& driverWorkScheduleSetAlias.IsActive
				)
				.TransformUsing(Transformers.DistinctRootEntity)
				.List<Employee>();
		}

		public Employee GetEmployeeByINNAndAccount(IUnitOfWork uow, string inn, string account)
		{
			IList<Account> accountsAlias = null;
			var employees = uow.Session.QueryOver<Employee>()
				.JoinAlias(e => e.Accounts, () => accountsAlias)
				.Where(e => e.INN == inn)
				.List();
			return employees.FirstOrDefault(e => e.Accounts.Any(acc => acc.Number == account));
		}

		public IList<EmployeeWorkChart> GetWorkChartForEmployeeByDate(IUnitOfWork uow, Employee employee, DateTime date)
		{
			EmployeeWorkChart ewcAlias = null;

			return uow.Session.QueryOver(() => ewcAlias)
				.Where(() => ewcAlias.Employee.Id == employee.Id)
				.Where(() => ewcAlias.Date.Month == date.Month)
				.Where(() => ewcAlias.Date.Year == date.Year)
				.List();
		}

		public QueryOver<Employee> ActiveEmployeeOrderedQuery()
		{
			return QueryOver.Of<Employee>().Where(e => e.Status != EmployeeStatus.IsFired).OrderBy(e => e.LastName).Asc.ThenBy(e => e.Name).Asc.ThenBy(e => e.Patronymic).Asc;
		}

		public string GetEmployeePushTokenByOrderId(
			IUnitOfWork uow,
			int orderId,
			ExternalApplicationType externalApplicationType = ExternalApplicationType.DriverApp)
		{
			Domain.Orders.Order vodovozOrderAlias = null;
			RouteListItem routeListAddressAlias = null;
			RouteList routeListAlias = null;
			Employee employeeAlias = null;
			ExternalApplicationUser externalApplicationUserAlias = null;

			return uow.Session.QueryOver(() => routeListAddressAlias)
				.Inner.JoinAlias(() => routeListAddressAlias.RouteList, () => routeListAlias)
				.Inner.JoinAlias(() => routeListAddressAlias.Order, () => vodovozOrderAlias)
				.Inner.JoinAlias(() => routeListAlias.Driver, () => employeeAlias)
				.Inner.JoinAlias(
					() => employeeAlias.ExternalApplicationsUsers,
					() => externalApplicationUserAlias,
					u => u.ExternalApplicationType == externalApplicationType)
				.Where(() => vodovozOrderAlias.Id == orderId)
				.And(() => routeListAddressAlias.Status != RouteListItemStatus.Transfered)
				.And(() => routeListAddressAlias.TransferedTo == null)
				.Select(Projections.Property(() => externalApplicationUserAlias.Token))
				.SingleOrDefault<string>();
		}

		public EmployeeRegistration EmployeeRegistrationDuplicateExists(IUnitOfWorkFactory uowFactory, EmployeeRegistration registration)
		{
			using(var uow = uowFactory.CreateWithoutRoot())
			{
				return uow.Session.QueryOver<EmployeeRegistration>()
					.Where(er => er.PaymentForm == registration.PaymentForm)
					.And(er => er.RegistrationType == registration.RegistrationType)
					.And(er => er.TaxRate == registration.TaxRate)
					.And(er => er.Id != registration.Id)
					.SingleOrDefault();
			}
		}

		public IEnumerable<Employee> GetSubscribedToPushNotificationsDrivers(IUnitOfWork uow)
		{
			var query = from externalUser in uow.Session.Query<ExternalApplicationUser>()
						join employee in uow.Session.Query<Employee>()
							on externalUser.Employee.Id equals employee.Id
						where externalUser.Token != null
							&& externalUser.Token.Length > 2
							&& externalUser.ExternalApplicationType == ExternalApplicationType.DriverApp
						select employee;

			return query.ToList();
		}

		public string GetDriverPushTokenById(IUnitOfWork unitOfWork, int notifyableEmployeeId)
		{
			return (from externalUser in unitOfWork.Session.Query<ExternalApplicationUser>()
					where externalUser.Employee.Id == notifyableEmployeeId
					   && externalUser.ExternalApplicationType == ExternalApplicationType.DriverApp
					select externalUser.Token)
					.FirstOrDefault();
		}

		public int? GetEmployeeCounterpartyFromDatabase(IUnitOfWorkFactory uowFactory, int employeeId)
		{
			using(var uow = uowFactory.CreateWithoutRoot())
			{
				var oldEmployeeCounterparty = (from employee in uow.Session.Query<Employee>()
											   join counterparty in uow.Session.Query<Counterparty>()
												   on employee.Counterparty.Id equals counterparty.Id into oldCounterparties
											   from oldCounterparty in oldCounterparties.DefaultIfEmpty()
											   where employee.Id == employeeId
											   select oldCounterparty)
					.SingleOrDefault();

				return oldEmployeeCounterparty?.Id;
			}
		}

		public NamedDomainObjectNode GetOtherEmployeeInfoWithSameCounterparty(
			IUnitOfWorkFactory uowFactory, int employeeId, int counterpartyId)
		{
			using(var uow = uowFactory.CreateWithoutRoot())
			{
				return (from employee in uow.Session.Query<Employee>()
						where employee.Counterparty.Id == counterpartyId && employee.Id != employeeId
						let fullName = employee.FullName
						select new NamedDomainObjectNode
						{
							Id = employee.Id,
							Name = fullName
						})
					.SingleOrDefault();
			}
		}

		public IEnumerable<int> GetControlledByEmployeeSubdivisionIds(IUnitOfWork uow, int employeeId)
		{
			var financialResponcibilitiesCentersIds = uow.Session.Query<FinancialResponsibilityCenter>()
				.Where(FinancialResponsibilityCenterSpecifications
					.ForEmployeeIdIsResponsible(employeeId).Expression)
				.Select(x => x.Id)
				.ToArray();

			var subdivisionsResponsibleByFinancialResponsibilityCentersIds = uow.Session
				.Query<Subdivision>()
				.Where(SubdivisionSpecifications.ForFinancialResponsibilityCenters(financialResponcibilitiesCentersIds).Expression)
				.Select(s => s.Id)
				.ToArray();
			
			var subdivisionChiefIds = uow.Session.Query<Subdivision>()
				.Where(SubdivisionSpecifications.ForEmployeeIsChief(employeeId).Expression)
				.Select(s => s.Id)
				.ToArray();

			return subdivisionsResponsibleByFinancialResponsibilityCentersIds
				.Concat(subdivisionChiefIds)
				.Distinct();
		}

		public IEnumerable<EmployeeLastWageParameterStartDateNode> GetSelectedEmployeesWageParametersStartDate(
			IUnitOfWork uow, IEnumerable<int> employeeIds)
		{
			var employees =
				from employee in uow.Session.Query<Employee>()

				let lastWageParameterStartDate =
					(from wageParameter in uow.Session.Query<EmployeeWageParameter>()
					 where
						 wageParameter.Employee.Id == employee.Id
					 orderby wageParameter.Id descending
					 select wageParameter.StartDate)
					.FirstOrDefault()

				where
					employeeIds.Contains(employee.Id)

				select new EmployeeLastWageParameterStartDateNode
				{
					Employee = employee,
					LastWageParameterStartDate = lastWageParameterStartDate
				};

			return employees.ToList();
		}

		/// <inheritdoc/>
		public IList<EmployeeNode> GetDriverForwarderEmployeesHavingWageDistrictLevelRates(
			IUnitOfWork uow,
			EmployeeCategory? category,
			int? wageDistrictLevelRatesIdFilter,
			bool isExcludeSelectedInFilterWageDistrictLevelRates)
		{
			var availableCategories = new[] { EmployeeCategory.driver, EmployeeCategory.forwarder };

			var query =
				from employee in uow.Session.Query<Employee>()

				let lastWageLevelRatesIdHavingRequiredParameterItem =
					(int?)(from wageParameter in uow.Session.Query<EmployeeWageParameter>()
						   join wpi in uow.Session.Query<RatesLevelWageParameterItem>() on wageParameter.WageParameterItem.Id equals wpi.Id into wpis
						   from wageParameterItem in wpis.DefaultIfEmpty()
						   join wpicc in uow.Session.Query<RatesLevelWageParameterItem>() on wageParameter.WageParameterItemForOurCars.Id equals wpicc.Id into wpiccs
						   from wageParameterItemCompanyCar in wpiccs.DefaultIfEmpty()
						   join wpirc in uow.Session.Query<RatesLevelWageParameterItem>() on wageParameter.WageParameterItemForRaskatCars.Id equals wpirc.Id into wpircs
						   from wageParameterItemRaskatCar in wpircs.DefaultIfEmpty()
						   where
						   wageParameter.Employee.Id == employee.Id
						   && wageParameter.EndDate == null
						   && (wageParameterItem.WageDistrictLevelRates.Id == wageDistrictLevelRatesIdFilter
								|| wageParameterItemCompanyCar.WageDistrictLevelRates.Id == wageDistrictLevelRatesIdFilter
								|| wageParameterItemRaskatCar.WageDistrictLevelRates.Id == wageDistrictLevelRatesIdFilter)
						   orderby wageParameter.Id descending
						   select wageParameter.Id)
					.FirstOrDefault()

				where
					availableCategories.Contains(employee.Category)
					&& (category == null || employee.Category == category)
					&& (wageDistrictLevelRatesIdFilter == null
						|| (isExcludeSelectedInFilterWageDistrictLevelRates && lastWageLevelRatesIdHavingRequiredParameterItem == null)
						|| (!isExcludeSelectedInFilterWageDistrictLevelRates && lastWageLevelRatesIdHavingRequiredParameterItem != null))

				orderby employee.LastName
				orderby employee.Name
				orderby employee.Patronymic

				select new EmployeeNode
				{
					Id = employee.Id,
					LastName = employee.LastName,
					Name = employee.Name,
					Patronymic = employee.Patronymic
				};

			return query.ToList();
		}
	}
}
