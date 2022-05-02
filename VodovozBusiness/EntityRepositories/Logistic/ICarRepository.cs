using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
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
		IList<Car> GetCars(IUnitOfWork uow);
	}
}
