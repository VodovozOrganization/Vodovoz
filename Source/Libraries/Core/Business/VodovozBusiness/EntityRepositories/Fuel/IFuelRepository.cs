using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Fuel;
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
		Task<int> SaveFuelTransactionsIfNeedAsync(IUnitOfWork uow, IEnumerable<FuelTransaction> fuelTransactions);
		Task<int> SaveNewAndUpdateExistingFuelTransactions(IUnitOfWork uow, IEnumerable<FuelTransaction> fuelTransactions);
		IEnumerable<FuelCard> GetFuelCardsByCardId(IUnitOfWork uow, string cardId);
		IEnumerable<FuelCard> GetFuelCardsByCardNumber(IUnitOfWork uow, string cardNumber);
		Task SaveFuelApiRequest(IUnitOfWork uow, FuelApiRequest request);
		IEnumerable<FuelCardVersion> GetActiveVersionsOnDateHavingFuelCard(IUnitOfWork unitOfWork, DateTime date, int fuelCardId);
		string GetFuelCardIdByNumber(IUnitOfWork unitOfWork, string cardNumber);
		FuelDocument GetFuelDocumentByFuelLimitId(IUnitOfWork unitOfWork, string fuelLimitId);
		decimal GetGivedFuelInLitersOnDate(IUnitOfWork unitOfWork, int carId, DateTime date);
		Task<IDictionary<string, decimal>> GetAverageFuelPricesByLastWeekTransactionsData(IUnitOfWork uow, CancellationToken cancellationToken);
		Task<IEnumerable<FuelType>> GetFuelTypesByProductGroupIds(IUnitOfWork uow, IEnumerable<string> productGroupIds, CancellationToken cancellationToken);
	}
}
