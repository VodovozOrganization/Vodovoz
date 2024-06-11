using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.EntityRepositories.Logistic
{
	public partial class CarRepository : ICarRepository
	{
		public Car GetCarByDriver(IUnitOfWork uow, Employee driver)
		{
			return uow.Session.QueryOver<Car>()
					  .Where(x => x.Driver == driver)
					  .Take(1)
					  .SingleOrDefault();
		}

		public IList<Car> GetCarsByDrivers(IUnitOfWork uow, int[] driversIds)
		{
			return uow.Session.QueryOver<Car>()
					  .Where(x => x.Driver.Id.IsIn(driversIds))
					  .List();
		}

		public QueryOver<Car> ActiveCarsQuery()
		{
			return QueryOver.Of<Car>()
				.Where(x => !x.IsArchive);
		}

		public bool IsInAnyRouteList(IUnitOfWork uow, Car car)
		{
			var rll = uow.Session.QueryOver<RouteList>()
				.Where(rl => rl.Car == car).Take(1).List();

			return rll.Any();
		}

		public IList<CarEvent> GetCarEventsForCostCarExploitation(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate,
			Car car,
			IEnumerable<int> selectedCarEventTypesIds,
			IEnumerable<CarTypeOfUse> selectedCarTypeOfUse,
			IEnumerable<CarOwnType> selectedCarOwnTypes)
		{
			Car carAlias = null;
			CarEvent carEventAlias = null;
			CarModel carModelAlias = null;
			CarVersion carVersionAlias = null;

			return uow.Session.QueryOver(() => carEventAlias)
				.JoinAlias(() => carEventAlias.Car, () => carAlias)
				.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
				.JoinEntityAlias(
					() => carVersionAlias,
					() => carAlias.Id == carVersionAlias.Car.Id
						&& carVersionAlias.StartDate <= carEventAlias.EndDate
						&& (carVersionAlias.EndDate == null || carVersionAlias.EndDate >= carEventAlias.EndDate))
				.WhereRestrictionOn(() => carEventAlias.CarEventType.Id).IsInG(selectedCarEventTypesIds)
				.WhereRestrictionOn(() => carModelAlias.CarTypeOfUse).IsInG(selectedCarTypeOfUse)
				.WhereRestrictionOn(() => carVersionAlias.CarOwnType).IsInG(selectedCarOwnTypes)
				.And(() => car == null || car == carEventAlias.Car)
				.And(() => carEventAlias.EndDate >= startDate)      // Ориентируемся только на дату окончания события
				.And(() => carEventAlias.EndDate <= endDate)
				.OrderByAlias(() => carEventAlias.EndDate).Desc()
				.List<CarEvent>();
		}

		public IQueryable<CarInsuranceNode> GetActualCarInsurances(IUnitOfWork unitOfWork, CarInsuranceType insuranceType)
		{
			var carInsurances =
				from car in unitOfWork.Session.Query<Car>()
				join carVersion in unitOfWork.Session.Query<CarVersion>() on car.Id equals carVersion.Car.Id
				join cm in unitOfWork.Session.Query<CarModel>() on car.CarModel.Id equals cm.Id into carModels
				from carModel in carModels.DefaultIfEmpty()
				where
				!car.IsArchive
				&& carVersion.StartDate <= DateTime.Now
				&& (carVersion.EndDate >= DateTime.Now || carVersion.EndDate == null)
				&& (carVersion.CarOwnType == CarOwnType.Company || carVersion.CarOwnType == CarOwnType.Raskat)

				select new CarInsuranceNode
				{
					CarTypeOfUse = carModel.CarTypeOfUse,
					CarRegNumber = car.RegistrationNumber,
					DriverGeography =
						car.Driver != null && car.Driver.Subdivision != null
						? car.Driver.Subdivision.GetGeographicGroup().Name
						: "",
					InsuranceType = insuranceType,
					LastInsurance =
						car.CarInsurances
						.Where(i => i.InsuranceType == insuranceType)
						.OrderByDescending(i => i.EndDate)
						.FirstOrDefault(),
					IsKaskoNotRelevant = car.IsKaskoInsuranceNotRelevant
				};

			return carInsurances;
		}

		public IQueryable<CarTechInspectNode> GetCarsTechInspectData(IUnitOfWork unitOfWork, int techInspectCarEventTypeId)
		{
			var carTechInspects =
				from car in unitOfWork.Session.Query<Car>()
				join carVersion in unitOfWork.Session.Query<CarVersion>() on car.Id equals carVersion.Car.Id
				join cm in unitOfWork.Session.Query<CarModel>() on car.CarModel.Id equals cm.Id into carModels
				from carModel in carModels.DefaultIfEmpty()
				where
				!car.IsArchive
				&& carVersion.StartDate <= DateTime.Now
				&& (carVersion.EndDate >= DateTime.Now || carVersion.EndDate == null)
				&& (carVersion.CarOwnType == CarOwnType.Company || carVersion.CarOwnType == CarOwnType.Raskat)

				let lastTechInspectOdometer =
				(from ce in unitOfWork.Session.Query<CarEvent>()
				 where ce.Car.Id == car.Id && ce.CarEventType.Id == techInspectCarEventTypeId
				 orderby ce.StartDate descending
				 select ce.Odometer
				).FirstOrDefault()

				select new CarTechInspectNode
				{
					CarTypeOfUse = carModel.CarTypeOfUse,
					CarRegNumber = car.RegistrationNumber,
					DriverGeography =
						car.Driver != null && car.Driver.Subdivision != null
						? car.Driver.Subdivision.GetGeographicGroup().Name
						: "",
					LastOdometerReading = car.OdometerReadings.OrderByDescending(r => r.StartDate).FirstOrDefault(),
					LastTechInspectOdometer = lastTechInspectOdometer,
					TeсhInspectInterval = carModel.TeсhInspectInterval,
					LeftUntilTechInspectKm = car.LeftUntilTechInspect
				};

			return carTechInspects;
		}

		public async Task<IList<RouteList>> GetCarsRouteLists(
			IUnitOfWork uow,
			CarTypeOfUse? carTypeOfUse,
			CarOwnType carOwnType,
			Car car,
			int[] includedCarModelIds,
			int[] excludedCarModelIds,
			DateTime startDate,
			DateTime endDate,
			bool isOnlyCarsWithCompletedFastDelivery,
			bool isOnlyCarsWithCompletedCommonDelivery,
			CancellationToken cancellationToken)
		{
			return await Task.Run(() =>
			{
				RouteList routeListAlias = null;
				RouteListItem routeListAddressAlias = null;
				Order orderAlias = null;
				Car carAlias = null;
				CarModel carModelAlias = null;
				CarVersion carVersionAlias = null;
				Employee assignedDriverAlias = null;

				var query = uow.Session.QueryOver(() => routeListAlias)
					.Inner.JoinAlias(() => routeListAlias.Car, () => carAlias)
					.Inner.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
					.Left.JoinAlias(() => carAlias.Driver, () => assignedDriverAlias)
					.JoinEntityAlias(() => carVersionAlias,
						() => carVersionAlias.Car.Id == carAlias.Id
							&& carVersionAlias.StartDate <= routeListAlias.Date &&
							(carVersionAlias.EndDate == null || carVersionAlias.EndDate >= routeListAlias.Date))
					.Where(() => routeListAlias.Date >= startDate && routeListAlias.Date < endDate)
					.Where(() => !carAlias.IsArchive)
					.And(() => carModelAlias.CarTypeOfUse != CarTypeOfUse.Truck)
					.And(() => assignedDriverAlias.Id == null || !assignedDriverAlias.VisitingMaster)
					.And(() => carVersionAlias.CarOwnType == carOwnType);

				if(carTypeOfUse != null)
				{
					query.Where(() => carModelAlias.CarTypeOfUse == carTypeOfUse);
				}

				if(car != null)
				{
					query.Where(() => carAlias.Id == car.Id);
				}

				if(includedCarModelIds.Any())
				{
					query.Where(Restrictions.In(Projections.Property(nameof(carAlias.Id)), includedCarModelIds));
				}

				if(excludedCarModelIds.Any())
				{
					query.Where(Restrictions.Not(Restrictions.In(Projections.Property(nameof(carAlias.Id)), excludedCarModelIds)));
				}

				var completedFastDeliveryAddressesSubquery =
					QueryOver.Of(() => routeListAddressAlias)
					.Left.JoinAlias(() => routeListAddressAlias.Order, () => orderAlias)
					.Where(() => routeListAddressAlias.RouteList.Id == routeListAlias.Id)
					.And(() => routeListAddressAlias.Status == RouteListItemStatus.Completed)
					.And(() => orderAlias.IsFastDelivery)
					.Select(rla => rla.Id);

				var completedCommonAddressesSubquery =
					QueryOver.Of(() => routeListAddressAlias)
					.Left.JoinAlias(() => routeListAddressAlias.Order, () => orderAlias)
					.Where(() => routeListAddressAlias.RouteList.Id == routeListAlias.Id)
					.And(() => routeListAddressAlias.Status == RouteListItemStatus.Completed)
					.And(() => !orderAlias.IsFastDelivery)
					.Select(rla => rla.Id);

				if(isOnlyCarsWithCompletedFastDelivery && !isOnlyCarsWithCompletedCommonDelivery)
				{
					query.Where(Restrictions.IsNotNull(Projections.SubQuery(completedFastDeliveryAddressesSubquery)));
				}

				if(isOnlyCarsWithCompletedCommonDelivery && !isOnlyCarsWithCompletedFastDelivery)
				{
					query.Where(Restrictions.IsNotNull(Projections.SubQuery(completedCommonAddressesSubquery)));
				}

				query.Fetch(SelectMode.Fetch, x => x.Addresses)
					.Fetch(SelectMode.Fetch, x => x.Driver)
					.Fetch(SelectMode.Fetch, x => x.Forwarder);

				return query
					.OrderBy(() => carAlias.Id).Asc
					.ThenBy(() => routeListAlias.Id).Asc
					.TransformUsing(Transformers.DistinctRootEntity)
					.List<RouteList>();
			},
				cancellationToken
			);
		}

		public async Task<IList<CarEvent>> GetCarEvents(
			IUnitOfWork uow,
			CarTypeOfUse? carTypeOfUse,
			int[] includedCarModelIds,
			int[] excludedCarModelIds,
			CarOwnType carOwnType,
			Car car,
			DateTime startDate,
			DateTime endDate,
			CancellationToken cancellationToken)
		{
			return await Task.Run(() =>
			{
				CarEvent carEventAlias = null;
				Car carAlias = null;
				CarModel carModelAlias = null;
				CarVersion carVersionAlias = null;
				Employee assignedDriverAlias = null;

				var query = uow.Session.QueryOver(() => carEventAlias)
					.Inner.JoinAlias(() => carEventAlias.Car, () => carAlias)
					.Inner.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
					.Left.JoinAlias(() => carAlias.Driver, () => assignedDriverAlias)
					.JoinEntityAlias(() => carVersionAlias,
						() => carVersionAlias.Car.Id == carAlias.Id
							&& carVersionAlias.StartDate <= carEventAlias.StartDate &&
							(carVersionAlias.EndDate == null || carVersionAlias.EndDate >= carEventAlias.StartDate))
					.Where(() => carEventAlias.StartDate <= endDate && carEventAlias.EndDate >= startDate && !carEventAlias.DoNotShowInOperation)
					.Where(() => !carAlias.IsArchive)
					.And(() => carModelAlias.CarTypeOfUse != Domain.Logistic.Cars.CarTypeOfUse.Truck)
					.And(() => assignedDriverAlias.Id == null || !assignedDriverAlias.VisitingMaster)
					.And(() => carVersionAlias.CarOwnType == carOwnType);

				if(carTypeOfUse != null)
				{
					query.Where(() => carModelAlias.CarTypeOfUse == carTypeOfUse);
				}

				if(car != null)
				{
					query.Where(() => carAlias.Id == car.Id);
				}

				if(includedCarModelIds.Any())
				{
					query.Where(Restrictions.In(Projections.Property(() => carModelAlias.Id), includedCarModelIds));
				}

				if(excludedCarModelIds.Any())
				{
					query.Where(Restrictions.Not(Restrictions.In(Projections.Property(() => carModelAlias.Id), excludedCarModelIds)));
				}

				return query
					.TransformUsing(Transformers.DistinctRootEntity)
					.List<CarEvent>();
			},
				cancellationToken);
		}

		public async Task<IList<Car>> GetCarsWithoutData(
			IUnitOfWork uow,
			CarTypeOfUse? carTypeOfUse,
			int[] includedCarModelIds,
			int[] excludedCarModelIds,
			CarOwnType carOwnType,
			Car car,
			DateTime startDate,
			DateTime endDate,
			CancellationToken cancellationToken)
		{
			return await Task.Run(() =>
			{
				Car carAlias = null;
				Employee assignedDriverAlias = null;
				CarModel carModelAlias = null;
				CarVersion carVersionAlias = null;

				var carsQuery = uow.Session.QueryOver(() => carAlias)
					.Inner.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
					.Left.JoinAlias(() => carAlias.Driver, () => assignedDriverAlias)
					.JoinEntityAlias(() => carVersionAlias,
						() => carVersionAlias.Car.Id == carAlias.Id
						&& carVersionAlias.StartDate <= endDate
							  && (carVersionAlias.EndDate == null || carVersionAlias.EndDate >= startDate))
					.Where(() => !carAlias.IsArchive)
					.And(() => assignedDriverAlias.Id == null || !assignedDriverAlias.VisitingMaster)
					.And(() => carModelAlias.CarTypeOfUse != Domain.Logistic.Cars.CarTypeOfUse.Truck)
					.And(() => carVersionAlias.CarOwnType == carOwnType);

				if(carTypeOfUse != null)
				{
					carsQuery.Where(() => carModelAlias.CarTypeOfUse == carTypeOfUse);
				}
				if(car != null)
				{
					carsQuery.Where(() => carAlias.Id == car.Id);
				}

				if(includedCarModelIds.Any())
				{
					carsQuery.Where(Restrictions.In(Projections.Property(() => carModelAlias.Id), includedCarModelIds));
				}

				if(excludedCarModelIds.Any())
				{
					carsQuery.Where(Restrictions.Not(Restrictions.In(Projections.Property(() => carModelAlias.Id), excludedCarModelIds)));
				}

				carsQuery.Fetch(SelectMode.Fetch, x => x.GeographicGroups);

				return carsQuery.List<Car>();
			},
			cancellationToken);
		}
	}
}
