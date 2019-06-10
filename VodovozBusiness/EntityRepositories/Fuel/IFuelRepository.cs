using System;
using System.Collections.Generic;
using Vodovoz.Domain.Logistic;
using QS.DomainModel.UoW;
namespace Vodovoz.EntityRepositories.Fuel
{
	public interface IFuelRepository
	{
		IEnumerable<FuelType> GetFuelTypes(IUnitOfWork uow);
		Dictionary<FuelType, decimal> GetAllFuelsBalance(IUnitOfWork uow);
		Dictionary<FuelType, decimal> GetAllFuelsBalanceForSubdivision(IUnitOfWork uow, Subdivision subdivision);
		decimal GetFuelBalance(IUnitOfWork uow, FuelType fuelType);
		decimal GetFuelBalanceForSubdivision(IUnitOfWork uow, Subdivision subdivision, FuelType fuelType);
	}
}
