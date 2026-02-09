using NHibernate;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz;
using Vodovoz.Core.Domain.Logistics.Drivers;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using VodovozBusiness.Domain.Logistic.Drivers;

namespace VodovozBusiness.EntityRepositories.Logistic
{
	public interface ILogisticRepository
	{
		/// <summary>
		/// Получить событие по автомобилю водителя за указанный период и группу событий
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="carId"></param>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <returns></returns>
		CarEvent GetCarEventByCarId(IUnitOfWork uow, int carId, CarEventGroup group, DateTime endDate);

		/// <summary>
		/// Получить день расписания водителя за указанный день.
		/// На одного водителя может быть только 1 день расписания
		/// </summary>
		/// <param name=""></param>
		/// <param name="driverId"></param>
		/// <param name="date"></param>
		/// <returns></returns>
		DriverScheduleItem GetDriverScheduleItemByDriverId(IUnitOfWork uow, int driverId, DateTime date);

		/// <summary>
		/// Получить события по автомобилям водителей за указанный период
		/// </summary>
		/// <param name="driverIds"></param>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <returns></returns>
		IList<CarEvent> GetCarEventsByDriverIds(IUnitOfWork uow, int[] driverIds,  DateTime startDate, DateTime endDate);

		/// <summary>
		/// Получить дни расписания водителей за указанный период
		/// </summary>
		/// <param name="driverIds"></param>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <returns></returns>
		IList<DriverScheduleItem> GetDriverScheduleItemsByDriverIds(IUnitOfWork uow, int[] driverIds, DateTime startDate, DateTime endDate);

		/// <summary>
		/// Получить запрос водителей с joinами по машинам и графику водителей с фильтром по подразделениям
		/// </summary>
		IQueryOver<Employee, Employee> GetDriversQueryWithJoins(IUnitOfWork uow, int[] subdivisionIds, DateTime startDate, DateTime endDate);

		/// <summary>
		/// Получить подразделения для водителей для графика водителей за указанный период
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="carTypeOfUses"></param>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <returns></returns>
		IList<Subdivision> GetSubdivisionsForDriverSchedule(IUnitOfWork uow, CarTypeOfUse[] carTypeOfUses, DateTime startDate, DateTime endDate);

		/// <summary>
		/// Получить график водителей за указанный период
		/// </summary>
		/// <param name="uow"></param>
		/// <param name="driverIds"></param>
		/// <param name="startDate"></param>
		/// <param name="endDate"></param>
		/// <returns></returns>
		IList<DriverSchedule> GetDriverSchedules(IUnitOfWork uow, int[] driverIds, DateTime startDate, DateTime endDate);
	}
}
