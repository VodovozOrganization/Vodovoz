using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using static Vodovoz.EntityRepositories.Logistic.CarRepository;

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
		IQueryable<CarInsuranceNode> GetActualCarInsurances(IUnitOfWork unitOfWork, CarInsuranceType insuranceType);
		IQueryable<CarTechInspectNode> GetCarsTechInspectData(IUnitOfWork unitOfWork, int techInspectCarEventTypeId);
		Task<IList<RouteList>> GetCarsRouteLists(IUnitOfWork uow, CarTypeOfUse? carTypeOfUse, CarOwnType carOwnType, Car car,
			int[] includedCarModelIds, int[] excludedCarModelIds, DateTime startDate, DateTime endDate,
			bool isOnlyCarsWithCompletedFastDelivery, bool isOnlyCarsWithCompletedCommonDelivery, CancellationToken cancellationToken);
		Task<IList<int>> GetCarsIdsHavingRouteLists(IUnitOfWork uow, CarTypeOfUse? carTypeOfUse, CarOwnType carOwnType,
			Car car, int[] includedCarModelIds, int[] excludedCarModelIds, DateTime startDate, DateTime endDate,
			bool isOnlyCarsWithCompletedFastDelivery, bool isOnlyCarsWithCompletedCommonDelivery, CancellationToken cancellationToken);
		Task<IList<CarEvent>> GetCarEvents(IUnitOfWork uow, CarTypeOfUse? carTypeOfUse, int[] includedCarModelIds, int[] excludedCarModelIds,
			CarOwnType carOwnType, Car car, DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
		Task<IList<Car>> GetCarsWithoutData(IUnitOfWork uow, CarTypeOfUse? carTypeOfUse, int[] includedCarModelIds, int[] excludedCarModelIds,
			CarOwnType carOwnType, Car car, DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
	}
}
