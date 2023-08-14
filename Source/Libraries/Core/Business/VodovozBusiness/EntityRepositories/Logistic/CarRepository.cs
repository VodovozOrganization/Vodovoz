using System;
using System.Linq;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Transform;
using QS.DomainModel.UoW;
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
				.And(() => carEventAlias.EndDate >= startDate)		// Ориентируемся только на дату окончания события
				.And(() => carEventAlias.EndDate <= endDate)
				.OrderByAlias(() => carEventAlias.EndDate).Desc()
				.List<CarEvent>();
		}
	}
}
