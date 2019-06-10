using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Fuel;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate;
using System.Linq;
using System.Collections.Generic;

namespace Vodovoz.Repositories
{
	public static class FuelRepository
	{
		public static FuelType GetDefaultFuel(IUnitOfWork uow)
		{
			return uow.Session.QueryOver<FuelType>()
					  .Where(x => x.Name == "АИ-92")
					  .Take(1)
					  .SingleOrDefault();
		}
	}
}
