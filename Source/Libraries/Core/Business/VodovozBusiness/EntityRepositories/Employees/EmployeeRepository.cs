using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.Banks.Domain;
using QS.DomainModel.UoW;
using QS.Project.Services;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.EntityRepositories.Employees
{
	public class EmployeeRepository : IEmployeeRepository
	{
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

			return uow.Session.QueryOver<Employee>(() => employeeAlias)
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

			return uow.Session.QueryOver<EmployeeWorkChart>(() => ewcAlias)
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
			Vodovoz.Domain.Orders.Order vodovozOrderAlias = null;
			RouteListItem routeListAddressAlias = null;
			RouteList routeListAlias = null;
			Employee employeeAlias = null;
			ExternalApplicationUser externalApplicationUserAlias = null;

			return uow.Session.QueryOver<RouteListItem>(() => routeListAddressAlias)
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

		public EmployeeRegistration EmployeeRegistrationDuplicateExists(EmployeeRegistration registration)
		{
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot())
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
				where !string.IsNullOrWhiteSpace(externalUser.Token) &&
					externalUser.ExternalApplicationType == ExternalApplicationType.DriverApp
				select employee;

			return query.ToList();
		}
	}
}
