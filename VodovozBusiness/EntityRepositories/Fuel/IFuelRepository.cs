﻿using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.EntityRepositories.Fuel
{
	public interface IFuelRepository
	{
		IEnumerable<FuelType> GetFuelTypes(IUnitOfWork uow);
		Dictionary<FuelType, decimal> GetAllFuelsBalance(IUnitOfWork uow);
		Dictionary<FuelType, decimal> GetAllFuelsBalanceForSubdivision(IUnitOfWork uow, Subdivision subdivision);
		decimal GetFuelBalance(IUnitOfWork uow, FuelType fuelType);
		decimal GetFuelBalance(IUnitOfWork uow, Employee driver, Car car, DateTime? before = null, params int[] excludeOperationsIds);
		decimal GetFuelBalanceForSubdivision(IUnitOfWork uow, Subdivision subdivision, FuelType fuelType);
		FuelType GetDefaultFuel(IUnitOfWork uow);
	}
}
