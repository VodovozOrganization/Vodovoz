using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Fuel;
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

		/// <summary>
		/// Средние цены на топливо по данным транзакций за предыдущую неделю
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Средняя стоимость топлива сгруппировання по Id типа топлива</returns>
		Task<IDictionary<int, decimal>> GetAverageFuelPricesByLastWeekTransactionsData(IUnitOfWork uow, CancellationToken cancellationToken);

		/// <summary>
		/// Типы топлива, имеющие указанные Id
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="fuelTypeIds">Коллекция Id</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns></returns>
		Task<IEnumerable<FuelType>> GetFuelTypesByIds(IUnitOfWork uow, IEnumerable<int> fuelTypeIds, CancellationToken cancellationToken);

		/// <summary>
		/// Продукты (топливо), относящихся к указанному типу
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="fuelTypeId">Id типа топлива в ДВ</param>
		/// <returns>Продукты (топливо)</returns>
		IEnumerable<GazpromFuelProduct> GetFuelProductsByFuelTypeId(IUnitOfWork uow, int fuelTypeId);

		/// <summary>
		/// Группы продуктов (топлива) Газпромнефти, относящихся к указанному типу топлива в ДВ
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="fuelTypeId">Id типа топлива в ДВ</param>
		/// <returns></returns>
		IEnumerable<GazpromFuelProductsGroup> GetGazpromFuelProductsGroupsByFuelTypeId(IUnitOfWork uow, int fuelTypeId);
	}
}
