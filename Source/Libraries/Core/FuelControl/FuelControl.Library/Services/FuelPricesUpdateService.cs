using Microsoft.Extensions.Logging;
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
		private readonly ILogger<FuelPricesUpdateService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IFuelRepository _fuelRepository;
		private readonly IFuelPriceVersionsController _fuelPriceVersionsController;

		public FuelPricesUpdateService(
			ILogger<FuelPricesUpdateService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IFuelRepository fuelRepository,
			IFuelPriceVersionsController fuelPriceVersionsController)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			_fuelPriceVersionsController = fuelPriceVersionsController ?? throw new ArgumentNullException(nameof(fuelPriceVersionsController));
		}

		public async Task UpdateFuelPricesByLastWeekTransaction(CancellationToken cancellationToken)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(FuelPricesUpdateService)))
			{
				var fuelAveragePrices =
					await _fuelRepository.GetAverageFuelPricesByLastWeekTransactionsData(uow, cancellationToken);

				var fuelTypes =
					await _fuelRepository.GetFuelTypesByIds(uow, fuelAveragePrices.Keys, cancellationToken);

				var newVersionsStartDate = DateTime.Today;

				foreach(var fuelType in fuelTypes)
				{
					var isPriceCalculated = fuelAveragePrices.TryGetValue(fuelType.Id, out var price);

					if(!isPriceCalculated)
					{
						continue;
					}

					_fuelPriceVersionsController.SetFuelType(fuelType);

					if(!_fuelPriceVersionsController.IsValidDateForNewCarVersion(newVersionsStartDate))
					{
						var message =
							$"Новая версия стоимости для вида топлива \"{fuelType.Name}\" не может быть добавлена. " +
							$"Указанная дата начала действия новой версии {newVersionsStartDate:dd.MM.yyyy}. " +
							$"В списке уже имеется версия с датой начала больше либо равной указанной.";

						_logger.LogInformation(message);

						continue;
					}

					_fuelPriceVersionsController.CreateAndAddVersion(price, newVersionsStartDate);

					uow.Save(fuelType);

					_logger.LogInformation(
						"Новая версия стоимости топлива для вида топлива \"{FuelTypeName}\" успешно добавлена. " +
						"Дата начала действия версии: {StartDate}. " +
						"Новая стоимость: {FuelPrice}",
						fuelType.Name,
						newVersionsStartDate.ToString("dd.MM.yyyy HH:mm:ss"),
						price.ToString("N2"));
				}

				cancellationToken.ThrowIfCancellationRequested();

				if(!uow.HasChanges)
				{
					return;
				}

				uow.Commit();
			}
		}
	}
}
