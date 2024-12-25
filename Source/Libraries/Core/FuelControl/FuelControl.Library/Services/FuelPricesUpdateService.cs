using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Controllers;
using Vodovoz.EntityRepositories.Fuel;

namespace FuelControl.Library.Services
{
	public class FuelPricesUpdateService : IFuelPricesUpdateService
	{
		private readonly IFuelRepository _fuelRepository;
		private readonly IFuelPriceVersionsController _fuelPriceVersionsController;

		public FuelPricesUpdateService(
			IFuelRepository fuelRepository,
			IFuelPriceVersionsController fuelPriceVersionsController)
		{
			_fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			_fuelPriceVersionsController = fuelPriceVersionsController ?? throw new ArgumentNullException(nameof(fuelPriceVersionsController));
		}

		public async Task UpdateFuelPricesByLastWeekTransaction(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			var fuelAveragePrices =
				await _fuelRepository.GetAverageFuelPricesByLastWeekTransactionsData(uow, cancellationToken);

			var fuelTypes =
				await _fuelRepository.GetFuelTypesByProductGroupIds(uow, fuelAveragePrices.Keys, cancellationToken);

			foreach(var fuelType in fuelTypes)
			{
				var isPriceCalculated = fuelAveragePrices.TryGetValue(fuelType.ProductGroupId, out var price);

				if(isPriceCalculated)
				{
					_fuelPriceVersionsController.SetFuelType(fuelType);
					_fuelPriceVersionsController.CreateAndAddVersion(price);

					uow.Save(fuelType);
				}
			}

			cancellationToken.ThrowIfCancellationRequested();

			uow.Commit();
		}
	}
}
