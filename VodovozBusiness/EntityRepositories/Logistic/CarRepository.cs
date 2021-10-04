using System.Linq;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.EntityRepositories.Logistic
{
	public class CarRepository : ICarRepository
	{
		public CarVersion GetCarVersionByDriver(IUnitOfWork uow, Employee driver)
		{
			Car carAlias = null;
			return uow.Session.QueryOver<CarVersion>()
				.Left.JoinAlias(x => x.Car.Id, () => carAlias.Id)
					  .Where(x => carAlias.Driver == driver)
					  .Take(1)
					  .SingleOrDefault();
		}

		public IList<CarVersion> GetCarVersionsByDrivers(IUnitOfWork uow, int[] driversIds)
		{
			Car carAlias = null;
			return uow.Session.QueryOver<CarVersion>()
				.Left.JoinAlias(x => x.Car.Id, () => carAlias.Id)
					  .Where(x => carAlias.Driver.Id.IsIn(driversIds))
					  .List();
		}

		public QueryOver<CarVersion> ActiveCompanyCarVersionsQuery()
		{
			var isCompanyHavingRestriction = Restrictions.In(Projections.Property<ModelCar>(x => x.CarTypeOfUse), Car.GetCompanyHavingsTypes());
			return QueryOver.Of<CarVersion>()
				.Where(isCompanyHavingRestriction)
				.Where(x => !x.Car.Model.IsArchive);
		}

		public QueryOver<Car> ActiveCarsQuery()
		{
			return QueryOver.Of<Car>()
							.Where(x => !x.Model.IsArchive);
		}

		public bool IsInAnyRouteList(IUnitOfWork uow, CarVersion carVersion)
        {
			var rll = uow.Session.QueryOver<RouteList>()
				.Where(rl => rl.CarVersion == carVersion).Take(1).List();

			return rll.Any();
        }
	}
}
