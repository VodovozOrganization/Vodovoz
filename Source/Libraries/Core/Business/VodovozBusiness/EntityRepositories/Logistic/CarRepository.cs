using NHibernate;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.EntityRepositories.Logistic
{
	public class CarRepository : ICarRepository
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

		public IQueryable<CarInsuranceNode> GetActualCarInsurances(IUnitOfWork unitOfWork)
		{
			var carInsurances =
				from insurance in unitOfWork.Session.Query<CarInsurance>()
				join c in unitOfWork.Session.Query<Car>() on insurance.Car.Id equals c.Id into cars
				from car in cars.DefaultIfEmpty()
				join cm in unitOfWork.Session.Query<CarModel>() on car.CarModel.Id equals cm.Id into carModels
				from carModel in carModels.DefaultIfEmpty()
				join d in unitOfWork.Session.Query<Employee>() on car.Driver.Id equals d.Id into drivers
				from driver in drivers.DefaultIfEmpty()
				join c in unitOfWork.Session.Query<Counterparty>() on insurance.Insurer.Id equals c.Id into counterparties
				from insurer in counterparties.DefaultIfEmpty()
				orderby insurance.EndDate descending
				group new { Insurance = insurance, Car = car, CarModel = carModel, Driver = driver, Insurer = insurer } by new { car.Id, insurance.InsuranceType } into groupedInsurances
				select new CarInsuranceNode
				{
					CarTypeOfUse = groupedInsurances.FirstOrDefault().CarModel.CarTypeOfUse,
					CarRegNumber = groupedInsurances.FirstOrDefault().Car.RegistrationNumber,
					DriverGeography =
						groupedInsurances.FirstOrDefault().Driver != null
						? groupedInsurances.FirstOrDefault().Driver.Subdivision.GetGeographicGroup().Name
						: "",
					CarInsuranceType = groupedInsurances.FirstOrDefault().Insurance.InsuranceType,
					StartDate = groupedInsurances.FirstOrDefault().Insurance.StartDate,
					EndDate = groupedInsurances.FirstOrDefault().Insurance.EndDate,
					Insurer =
						string.IsNullOrWhiteSpace(groupedInsurances.FirstOrDefault().Insurer.FullName)
						? groupedInsurances.FirstOrDefault().Insurer.Name
						: groupedInsurances.FirstOrDefault().Insurer.FullName,
					InsuranceNumber = groupedInsurances.FirstOrDefault().Insurance.InsuranceNumber,
					DaysToExpire = (int)(groupedInsurances.FirstOrDefault().Insurance.EndDate - DateTime.Today).TotalDays
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

		public class CarInsuranceNode
		{
			public CarTypeOfUse CarTypeOfUse { get; set; }
			public string CarRegNumber { get; set; }
			public string DriverGeography { get; set; }
			public CarInsuranceType CarInsuranceType { get; set; }
			public DateTime StartDate { get; set; }
			public DateTime EndDate { get; set; }
			public string Insurer { get; set; }
			public string InsuranceNumber { get; set; }
			public int DaysToExpire { get; set; }
		}

		public class CarTechInspectNode
		{
			public CarTypeOfUse CarTypeOfUse { get; set; }
			public string CarRegNumber { get; set; }
			public string DriverGeography { get; set; }
			public OdometerReading LastOdometerReading { get; set; }
			public int? LastTechInspectOdometer { get; set; }
			public int TeсhInspectInterval { get; set; }
			public int LeftUntilTechInspectKm { get; set; }
			public int UpcomingTechInspectKm =>
				LastTechInspectOdometer.HasValue
				? LastTechInspectOdometer.Value + LeftUntilTechInspectKm
				: LeftUntilTechInspectKm;
		}
	}
}
