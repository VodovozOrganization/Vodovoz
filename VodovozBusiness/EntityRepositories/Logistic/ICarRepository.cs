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
		QueryOver<CarVersion> ActiveCompanyCarVersionsQuery();
		CarVersion GetCarVersionByDriver(IUnitOfWork uow, Employee driver);
		IList<CarVersion> GetCarVersionsByDrivers(IUnitOfWork uow, int[] driversIds);
        bool IsInAnyRouteList(IUnitOfWork uow, CarVersion carVersion);
    }
}
