using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Logistics.Cars;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.EntityRepositories.Logistic;
using VodovozBusiness.EntityRepositories.Logistic;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Infrastructure.Persistance.Logistic
{
	internal sealed class CarRepository : ICarRepository
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

		public IQueryable<CarInsuranceNode> GetActualCarInsurances(IUnitOfWork unitOfWork, CarInsuranceType insuranceType, IEnumerable<int> excludeCarIds)
		{
			var carInsurances =
				from car in unitOfWork.Session.Query<Car>()
				join carVersion in unitOfWork.Session.Query<CarVersion>() on car.Id equals carVersion.Car.Id
				join carModel in unitOfWork.Session.Query<CarModel>() on car.CarModel.Id equals carModel.Id
				where
					!car.IsArchive
					&& !excludeCarIds.Contains(car.Id)
					&& carModel.CarTypeOfUse != CarTypeOfUse.Loader
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

		public IQueryable<CarTechInspectNode> GetCarsTechInspectData(IUnitOfWork unitOfWork, int techInspectCarEventTypeId, IEnumerable<int> excludeCarIds)
		{
			var carTechInspects =
				from car in unitOfWork.Session.Query<Car>()
				join carVersion in unitOfWork.Session.Query<CarVersion>() on car.Id equals carVersion.Car.Id
				join carModel in unitOfWork.Session.Query<CarModel>() on car.CarModel.Id equals carModel.Id
				where
					!car.IsArchive
					&& !excludeCarIds.Contains(car.Id)
					&& carModel.CarTypeOfUse != CarTypeOfUse.Loader
					&& carVersion.StartDate <= DateTime.Now
					&& (carVersion.EndDate >= DateTime.Now || carVersion.EndDate == null)
					&& (carVersion.CarOwnType == CarOwnType.Company || carVersion.CarOwnType == CarOwnType.Raskat)

				let lastTechInspectOdometer =
					(from ce in unitOfWork.Session.Query<CarEvent>()
					 where ce.Car.Id == car.Id && ce.CarEventType.Id == techInspectCarEventTypeId
					 orderby ce.StartDate descending
					 select ce.Odometer
					)
					.FirstOrDefault()

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
					ManualTechInspectForKm = car.TechInspectForKm,
					TeсhInspectInterval = carModel.TeсhInspectInterval,
					LeftUntilTechInspectKm = car.LeftUntilTechInspect
				};

			return carTechInspects;
		}

		public IQueryable<CarTechnicalCheckupNode> GetCarsTechnicalCheckupData(IUnitOfWork unitOfWork, int carTechnicalCheckupEventTypeId, IEnumerable<int> excludeCarIds)
		{
			var data =
				from car in unitOfWork.Session.Query<Car>()
				join carVersion in unitOfWork.Session.Query<CarVersion>() on car.Id equals carVersion.Car.Id
				join carModel in unitOfWork.Session.Query<CarModel>() on car.CarModel.Id equals carModel.Id
				where
					!car.IsArchive
					&& !excludeCarIds.Contains(car.Id)
					&& carModel.CarTypeOfUse != CarTypeOfUse.Loader
					&& carVersion.StartDate <= DateTime.Now
					&& (carVersion.EndDate >= DateTime.Now || carVersion.EndDate == null)
					&& (carVersion.CarOwnType == CarOwnType.Company || carVersion.CarOwnType == CarOwnType.Raskat)

				let lastTechnicalCheckup =
					(from ce in unitOfWork.Session.Query<CarEvent>()
					 where ce.Car.Id == car.Id && ce.CarEventType.Id == carTechnicalCheckupEventTypeId
					 orderby ce.CarTechnicalCheckupEndingDate descending
					 select ce
					)
					.FirstOrDefault()

				select new CarTechnicalCheckupNode
				{
					CarTypeOfUse = carModel.CarTypeOfUse,
					CarRegNumber = car.RegistrationNumber,
					DriverGeography =
						car.Driver != null && car.Driver.Subdivision != null
						? car.Driver.Subdivision.GetGeographicGroup().Name
						: "",
					LastCarTechnicalCheckupEvent = lastTechnicalCheckup
				};

			return data;
		}

		public async Task<IList<CarEventData>> GetCarEvents(
			IUnitOfWork uow,
			CarTypeOfUse[] carTypesOfUse,
			int[] includedCarModelIds,
			int[] excludedCarModelIds,
			CarOwnType[] carOwnTypes,
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
				CarEventType carEventTypeAlias = null;
				CarEventData carEventDataAlias = null;

				var query = uow.Session.QueryOver(() => carEventAlias)
					.Inner.JoinAlias(() => carEventAlias.Car, () => carAlias)
					.Inner.JoinAlias(() => carAlias.CarModel, () => carModelAlias)
					.Left.JoinAlias(() => carAlias.Driver, () => assignedDriverAlias)
					.JoinEntityAlias(() => carVersionAlias,
						() => carVersionAlias.Car.Id == carAlias.Id
							&& carVersionAlias.StartDate <= carEventAlias.StartDate &&
							(carVersionAlias.EndDate == null || carVersionAlias.EndDate >= carEventAlias.StartDate))
					.Left.JoinAlias(() => carEventAlias.CarEventType, () => carEventTypeAlias)
					.Where(() => carEventAlias.StartDate <= endDate && carEventAlias.EndDate >= startDate && !carEventAlias.DoNotShowInOperation)
					.Where(() => !carAlias.IsArchive)
					.And(() => carModelAlias.CarTypeOfUse != CarTypeOfUse.Truck)
					.And(() => assignedDriverAlias.Id == null || !assignedDriverAlias.VisitingMaster)
					.And(Restrictions.In(Projections.Property(() => carVersionAlias.CarOwnType), carOwnTypes))
					.And(Restrictions.In(Projections.Property(() => carModelAlias.CarTypeOfUse), carTypesOfUse));

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

				var result = query.SelectList(list => list
					.Select(() => carEventAlias.Id).WithAlias(() => carEventDataAlias.EventId)
					.Select(() => carAlias.Id).WithAlias(() => carEventDataAlias.CarId)
					.Select(() => carEventAlias.StartDate).WithAlias(() => carEventDataAlias.StartDate)
					.Select(() => carEventAlias.EndDate).WithAlias(() => carEventDataAlias.EndDate)
					.Select(() => carEventTypeAlias.ShortName).WithAlias(() => carEventDataAlias.EventTypeShortName))
					.TransformUsing(Transformers.AliasToBean<CarEventData>())
					.List<CarEventData>();

				return result;
			},
				cancellationToken);
		}

		public async Task<IList<Car>> GetCarsWithoutData(
			IUnitOfWork uow,
			CarTypeOfUse[] carTypesOfUse,
			int[] includedCarModelIds,
			int[] excludedCarModelIds,
			CarOwnType[] carOwnTypes,
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
					.And(() => carModelAlias.CarTypeOfUse != CarTypeOfUse.Truck)
					.And(Restrictions.In(Projections.Property(() => carVersionAlias.CarOwnType), carOwnTypes))
					.And(Restrictions.In(Projections.Property(() => carModelAlias.CarTypeOfUse), carTypesOfUse));

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

				return carsQuery.TransformUsing(Transformers.DistinctRootEntity).List<Car>();
			},
			cancellationToken);
		}

		public async Task<IDictionary<(int CarId, int Day), IEnumerable<RouteListItem>>> GetNotPriorityDistrictsAddresses(
			IUnitOfWork unitOfWork,
			IList<int> routeListsIds,
			CancellationToken cancellationToken)
		{
			var carsRouteListAddresses =
				from rla in unitOfWork.Session.Query<RouteListItem>()
				join rl in unitOfWork.Session.Query<RouteList>() on rla.RouteList.Id equals rl.Id
				join empl in unitOfWork.Session.Query<Employee>() on rl.Driver.Id equals empl.Id into drivers
				from driver in drivers.DefaultIfEmpty()
				join o in unitOfWork.Session.Query<Order>() on rla.Order.Id equals o.Id into orders
				from order in orders.DefaultIfEmpty()
				where
				routeListsIds.Contains(rl.Id)
				&& (driver == null || !driver.DriverDistrictPrioritySets.Any(ddps =>
				ddps.DateActivated <= rl.Date && (ddps.DateDeactivated == null || ddps.DateDeactivated >= rl.Date)
				&& ddps.DriverDistrictPriorities.Any(ddp => ddp.District.Id == order.DeliveryPoint.District.Id)))
				select new { CarId = rl.Car.Id, rl.Date.Day, Address = rla };

			var carsRouteListAddressesGroup = (await carsRouteListAddresses.ToListAsync(cancellationToken))
				.GroupBy(rl => (rl.CarId, rl.Day))
				.ToDictionary(g => g.Key, g => g.Select(item => item.Address));

			return carsRouteListAddressesGroup;
		}

		public IQueryable<Car> GetCarsByRouteLists(IUnitOfWork unitOfWork, IEnumerable<int> routeListIds)
		{
			var cars =
				from rl in unitOfWork.Session.Query<RouteList>()
				join car in unitOfWork.Session.Query<Car>() on rl.Car.Id equals car.Id
				where routeListIds.Contains(rl.Id)
				orderby car.Id
				select car;

			return cars.Distinct();
		}

		public IQueryable<OdometerReading> GetOdometerReadingByCars(IUnitOfWork unitOfWork, IEnumerable<int> carsIds)
		{
			var odometerReading =
				unitOfWork.Session.Query<OdometerReading>()
				.Where(or => carsIds.Contains(or.Car.Id));

			return odometerReading;
		}

		public IDictionary<int, string> GetCarsGeoGroups(IUnitOfWork unitOfWork, IEnumerable<int> carsIds) =>
			unitOfWork.Session.Query<Car>()
			.Where(c => carsIds.Contains(c.Id))
			.Select(c => new { CarId = c.Id, GeoGroups = string.Join(", ", c.GeographicGroups.Select(g => g.Name)) })
			.ToDictionary(c => c.CarId, c => c.GeoGroups);

		public async Task<IDictionary<int, (Employee Driver, bool IsLastRouteListDriver)>> GetDriversByCars(
			IUnitOfWork unitOfWork, IEnumerable<int> carsIds, CancellationToken cancellationToken)
		{
			var carsQuery = unitOfWork.Session.Query<Car>()
				.Where(car => carsIds.Contains(car.Id));

			var routeListsQuery = unitOfWork.Session.Query<RouteList>()
				.Where(rl => carsIds.Contains(rl.Car.Id));

			var driversData =
				from car in carsQuery
				join d in unitOfWork.Session.Query<Employee>() on car.Driver.Id equals d.Id into drivers
				from driver in drivers.DefaultIfEmpty()
				let lastRouteListDriver = (
					from rl in routeListsQuery
					where rl.Car.Id == car.Id
					orderby rl.Date descending
					select rl.Driver
				).FirstOrDefault()
				select new
				{
					CarId = car.Id,
					Driver = driver,
					LastRouteListDriver = lastRouteListDriver
				};

			var list = await driversData.ToListAsync(cancellationToken);

			return list.ToDictionary(
				x => x.CarId,
				x => x.Driver != null
					? (x.Driver, false)
					: x.LastRouteListDriver != null
						? (x.LastRouteListDriver, true)
						: ((Employee)null, false)
			);
		}

		public async Task<IDictionary<int, IEnumerable<CarVersion>>> GetCarOwnTypesForPeriodByCars(
			IUnitOfWork uow,
			IEnumerable<int> carsIds,
			DateTime startDate,
			DateTime endDate,
			CancellationToken cancellationToken)
		{
			var carVersions =
				await (from carVersion in uow.Session.Query<CarVersion>()
					   where
						   carsIds.Contains(carVersion.Car.Id)
						   && carVersion.StartDate <= endDate
						   && (carVersion.EndDate == null || carVersion.EndDate >= startDate)
					   select new { CarId = carVersion.Car.Id, CarVersion = carVersion })
				.ToListAsync(cancellationToken);

			var carVersionsData = carVersions
				.GroupBy(cv => cv.CarId)
				.ToDictionary(
					g => g.Key,
					g => g.Select(v => v.CarVersion)
				);

			return carVersionsData;
		}

		public void ArchiveCar(IUnitOfWork uow, Car car, ArchivingReason reason)
		{
			if(car == null)
			{
				throw new ArgumentNullException(nameof(car));
			}

			car.IsArchive = true;
			car.ArchivingDate = DateTime.Now;
			car.ArchivingReason = reason;
			uow.Save(car);
		}

		public IQueryable<Car> GetCarsByIds(IUnitOfWork unitOfWork, IEnumerable<int> carsIds) =>
			unitOfWork.Session.Query<Car>()
			.Where(c => carsIds.Contains(c.Id))
			.Distinct();
	}
}
