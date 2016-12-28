using System;
using System.Linq;
using NHibernate.Transform;
using QSOrmProject;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Operations;
using Order = Vodovoz.Domain.Orders.Order;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using NHibernate.Criterion;

namespace Vodovoz.Repository.Operations
{
	public static class FuelRepository
	{
		public static decimal GetFuelBalance(IUnitOfWork UoW, Employee driver, Car car, FuelType fuel, DateTime? before = null, params int[] excludeOperationsIds)
		{
			FuelOperation operationAlias = null;
			FuelQueryResult result = null;
			var queryResult = UoW.Session.QueryOver<FuelOperation>(() => operationAlias)
				.Where(() => operationAlias.Fuel.Id == fuel.Id);
			if(driver != null)
				queryResult.Where(() => operationAlias.Driver.Id == driver.Id);
			if(car != null)
				queryResult.Where(() => operationAlias.Car.Id == car.Id);
			if (before.HasValue)
				queryResult.Where(() => operationAlias.OperationTime < before);
			if (excludeOperationsIds != null)
				queryResult.Where(() => !operationAlias.Id.IsIn(excludeOperationsIds));
			
			return queryResult.SelectList(list => list
				.SelectSum(() => operationAlias.LitersGived).WithAlias(() => result.Gived)
				.SelectSum(() => operationAlias.LitersOutlayed).WithAlias(() => result.Outlayed)
				).TransformUsing(Transformers.AliasToBean<FuelQueryResult>()).List<FuelQueryResult>()
				.FirstOrDefault()?.FuelBalance ?? 0;
		}

		class FuelQueryResult
		{
			public decimal Gived{get;set;}
			public decimal Outlayed{get;set;}
			public decimal FuelBalance{
				get{
					return Gived - Outlayed;
				}
			}
		}
	}


}