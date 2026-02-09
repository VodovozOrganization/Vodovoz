using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Logistics.Drivers;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using VodovozBusiness.Domain.Logistic.Drivers;
using VodovozBusiness.EntityRepositories.Logistic;

namespace Vodovoz.Infrastructure.Persistance.Logistic
{
	public class LogisticRepository : ILogisticRepository
	{
		public IList<CarEvent> GetCarEventsByDriverIds(IUnitOfWork uow, int[] driverIds, DateTime startDate, DateTime endDate)
		{
			return uow.Session.QueryOver<CarEvent>()
				.WhereRestrictionOn(e => e.Driver.Id).IsIn(driverIds)
				.Where(e => e.StartDate <= endDate && e.EndDate >= startDate)
				.List();
		}

		public IList<DriverScheduleItem> GetDriverScheduleItemsByDriverIds(IUnitOfWork uow, int[] driverIds, DateTime startDate, DateTime endDate)
		{
			DriverSchedule driverScheduleAlias = null;
			Employee employeeAlias = null;
			CarEvent carEventAlias = null;

			return uow.Session.QueryOver<DriverScheduleItem>()
					.Left.JoinAlias(i => i.DriverSchedule, () => driverScheduleAlias)
					.Left.JoinAlias(() => driverScheduleAlias.Driver, () => employeeAlias)
					.JoinEntityAlias(
						() => carEventAlias,
						() => carEventAlias.Driver.Id == employeeAlias.Id,
						NHibernate.SqlCommand.JoinType.LeftOuterJoin
					)
					.WhereRestrictionOn(() => employeeAlias.Id).IsIn(driverIds)
					.Where(i => i.Date >= startDate && i.Date <= endDate)
					.List();
		}

		public IQueryOver<Employee, Employee> GetDriversQueryWithJoins(IUnitOfWork uow, int[] subdivisionIds, DateTime startDate, DateTime endDate)
		{
			Employee employeeAlias = null;
			Car carAlias = null;
			CarVersion carVersionAlias = null;
			DriverSchedule driverScheduleAlias = null;

			var query = GetFilteredDriversQuery(uow, DateTime.MinValue, DateTime.MaxValue)
				.JoinEntityAlias(
					() => carVersionAlias,
					() => carVersionAlias.Car.Id == carAlias.Id,
					NHibernate.SqlCommand.JoinType.LeftOuterJoin
				)
				.JoinEntityAlias(
					() => driverScheduleAlias,
					() => driverScheduleAlias.Driver.Id == employeeAlias.Id,
					NHibernate.SqlCommand.JoinType.LeftOuterJoin
				)
				.WhereRestrictionOn(e => e.Subdivision.Id).IsIn(subdivisionIds);
			
			return query;
		}

		public IList<DriverSchedule> GetDriverSchedules(IUnitOfWork uow, int[] driverIds, DateTime startDate, DateTime endDate)
		{
			Employee employeeAlias = null;
			DriverScheduleItem driverScheduleItemAlias = null;

			return uow.Session.QueryOver<DriverSchedule>()
				.Left.JoinAlias(ds => ds.Driver, () => employeeAlias)
				.Left.JoinAlias(ds => ds.Days, () => driverScheduleItemAlias,
					() => driverScheduleItemAlias.Date >= startDate && driverScheduleItemAlias.Date <= endDate)
				.WhereRestrictionOn(() => employeeAlias.Id).IsIn(driverIds)
				.TransformUsing(Transformers.DistinctRootEntity)
				.List();
		}

		public IList<Subdivision> GetSubdivisionsForDriverSchedule(IUnitOfWork uow, CarTypeOfUse[] carTypeOfUses, DateTime startDate, DateTime endDate)
		{
			Subdivision subdivisionAlias = null;
			CarModel carModelAlias = null;

			var subdivisionIds = GetFilteredDriversQuery(uow, startDate, endDate)
				.Left.JoinAlias(e => e.Subdivision, () => subdivisionAlias)
				.WhereRestrictionOn(() => carModelAlias.CarTypeOfUse).IsIn(carTypeOfUses)
				.SelectList(list => list
					.SelectGroup(() => subdivisionAlias.Id)
				)
				.List<int>()
				.Distinct()
				.ToList();

			var subdivisions = subdivisionIds.Count > 0
				? uow.Session.QueryOver<Subdivision>()
					.WhereRestrictionOn(s => s.Id).IsIn(subdivisionIds.ToArray())
					.OrderBy(s => s.Name).Asc
					.List()
					.ToList()
				: new List<Subdivision>();

			return subdivisions;
		}

		private IQueryOver<Employee, Employee> GetFilteredDriversQuery(IUnitOfWork uow, DateTime startDate, DateTime endDate)
		{
			Employee employeeAlias = null;
			Car carAlias = null;
			CarModel carModelAlias = null;

			var query = uow.Session.QueryOver(() => employeeAlias)
				.JoinEntityAlias(
					() => carAlias,
					() => carAlias.Driver.Id == employeeAlias.Id,
					NHibernate.SqlCommand.JoinType.LeftOuterJoin
				)
				.Left.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.Where(() => employeeAlias.Category == EmployeeCategory.driver)
				.Where(Restrictions.Disjunction()
					.Add(Restrictions.Not(Restrictions.Eq(Projections.Property(() => employeeAlias.Status), EmployeeStatus.IsFired)))
					.Add(Restrictions.And(
						Restrictions.Eq(Projections.Property(() => employeeAlias.Status), EmployeeStatus.IsFired),
						Restrictions.Ge(Projections.Property(() => employeeAlias.DateFired), startDate)
					))
					.Add(Restrictions.And(
						Restrictions.Eq(Projections.Property(() => employeeAlias.Status), EmployeeStatus.OnCalculation),
						Restrictions.Ge(Projections.Property(() => employeeAlias.DateCalculated), endDate)
					))
				);

			return query;
		}

		public CarEvent GetCarEventByCarId(IUnitOfWork uow, int carId, CarEventGroup group, DateTime endDate)
		{
			return uow.Session.QueryOver<CarEvent>()
				.Where(e => e.Car.Id == carId)
				.Where(e => e.CarEventType.Id == group.CarEventType.Id)
				.Where(e => e.StartDate >= group.StartDate.Date && e.StartDate <= endDate)
				.SingleOrDefault();
		}

		public DriverScheduleItem GetDriverScheduleItemByDriverId(IUnitOfWork uow, int driverId, DateTime date)
		{
			DriverScheduleItem itemAlias = null;
			DriverSchedule scheduleAlias = null;

			var scheduleItem = uow.Session.QueryOver(() => itemAlias)
				.Where(() => itemAlias.Date == date)
				.JoinAlias(() => itemAlias.DriverSchedule, () => scheduleAlias)
				.Where(() => scheduleAlias.Driver.Id == driverId)
				.SingleOrDefault();

			return scheduleItem;
		}
	}
}
