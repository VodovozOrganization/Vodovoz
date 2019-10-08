using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.EntityRepositories.Logistic
{
	public interface ICarRepository
	{
		QueryOver<Car> ActiveCarsQuery();
		QueryOver<Car> ActiveCompanyCarsQuery();
		Car GetCarByDriver(IUnitOfWork uow, Employee driver);
		IList<Car> GetCarsByDrivers(IUnitOfWork uow, int[] driversIds);
	}
}