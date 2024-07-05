﻿using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.EntityRepositories.Logistic
{
	public interface ICarRepository
	{
		QueryOver<Car> ActiveCarsQuery();
		Car GetCarByDriver(IUnitOfWork uow, Employee driver);
		IList<Car> GetCarsByDrivers(IUnitOfWork uow, int[] driversIds);
		bool IsInAnyRouteList(IUnitOfWork uow, Car car);
		IList<CarEvent> GetCarEventsForCostCarExploitation(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate,
			Car car,
			IEnumerable<int> selectedCarEventTypesIds,
			IEnumerable<CarTypeOfUse> selectedCarTypeOfUse,
			IEnumerable<CarOwnType> selectedCarOwnTypes);
		IQueryable<CarInsuranceNode> GetActualCarInsurances(IUnitOfWork unitOfWork, CarInsuranceType insuranceType, IEnumerable<int> excludeCarIds);
		IQueryable<CarTechInspectNode> GetCarsTechInspectData(IUnitOfWork unitOfWork, int techInspectCarEventTypeId, IEnumerable<int> excludeCarIds);
		Task<IList<CarEventData>> GetCarEvents(IUnitOfWork uow, CarTypeOfUse? carTypeOfUse, int[] includedCarModelIds, int[] excludedCarModelIds,
			CarOwnType carOwnType, Car car, DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
		Task<IList<Car>> GetCarsWithoutData(IUnitOfWork uow, CarTypeOfUse? carTypeOfUse, int[] includedCarModelIds, int[] excludedCarModelIds,
			CarOwnType carOwnType, Car car, DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
		Task<IDictionary<(int CarId, int Day), IEnumerable<RouteListItem>>> GetNotPriorityDistrictsAddresses(
			IUnitOfWork unitOfWork, IList<int> routeListsIds, CancellationToken cancellationToken);
		IQueryable<Car> GetCarsByRouteLists(IUnitOfWork unitOfWork, IEnumerable<int> routeListIds);
		IQueryable<OdometerReading> GetOdometerReadingByCars(IUnitOfWork unitOfWork, IEnumerable<int> carsIds);
		IDictionary<int, string> GetCarsGeoGroups(IUnitOfWork unitOfWork, IEnumerable<int> carsIds);
		Task<IDictionary<int, string>> GetDriversNamesByCars(IUnitOfWork unitOfWork, IEnumerable<int> carsIds, CancellationToken cancellationToken);
		IQueryable<Car> GetCarsByIds(IUnitOfWork unitOfWork, IEnumerable<int> carsIds);
	}
}
