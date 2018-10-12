using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Repository.Logistics
{
	public static class FuelTypeRepository
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
