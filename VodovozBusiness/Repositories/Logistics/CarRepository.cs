using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Repository.Logistics
{
	public static class CarRepository
	{
		public static Car GetCarByDriver(IUnitOfWork uow, Employee driver)
		{
			return uow.Session.QueryOver<Car>()
					  .Where(x => x.Driver == driver)
					  .Take(1)
					  .SingleOrDefault();
		}

		public static IList<Car> GetCarsbyDrivers(IUnitOfWork uow, int[] driversIds)
		{
			return uow.Session.QueryOver<Car>()
				      .Where(x => x.Driver.Id.IsIn(driversIds))
				      .List();
		}

		public static QueryOver<Car> ActiveCompanyCarsQuery()
		{
			return QueryOver.Of<Car>()
							.Where(x => x.IsCompanyHavings && !x.IsArchive);
		}
	}
}
