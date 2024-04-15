using DatabaseServiceWorker.Options;
using FuelControl.Library.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Fuel;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.Infrastructure;
using Vodovoz.Settings.Fuel;
using VodovozInfrastructure.Utils;

namespace DatabaseServiceWorker
{
	public class FuelTransactionsControlWorker : TimerBackgroundServiceBase
	{
		private string _sessionId;
		private DateTime? _sessionExpirationDate;

		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOptions<FuelTransactionsControlOptions> _options;
		private readonly ILogger<FuelTransactionsControlWorker> _logger;
		private readonly IFuelManagmentAuthorizationService _authorizationService;
		private readonly IFuelTransactionsDataService _fuelTransactionsDataService;
		private readonly IFuelRepository _fuelRepository;
		private readonly IFuelControlSettings _fuelControlSettings;

		public FuelTransactionsControlWorker(
			IUnitOfWorkFactory unitOfWorkFactory,
			IOptions<FuelTransactionsControlOptions> options,
			ILogger<FuelTransactionsControlWorker> logger,
			IFuelManagmentAuthorizationService authorizationService,
			IFuelTransactionsDataService fuelTransactionsDataService,
			IFuelRepository fuelRepository,
			IFuelControlSettings fuelControlSettings)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
			_fuelTransactionsDataService = fuelTransactionsDataService ?? throw new ArgumentNullException(nameof(fuelTransactionsDataService));
			_fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			_fuelControlSettings = fuelControlSettings ?? throw new ArgumentNullException(nameof(fuelControlSettings));
		}

		protected override TimeSpan Interval
		{
			get
			{
				return _options.Value.ScanInterval;
			}
		}

		private bool IsAuthorized =>
			!string.IsNullOrWhiteSpace(_sessionId)
			&& _sessionExpirationDate.HasValue
			&& DateTime.Now < _sessionExpirationDate.Value;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Сохранение транзакций топлива");

			await DailyFuelTransactionsUpdate(uow);

			await MonthlyFuelTransactionsUpdate(uow);
		}

		private async Task DailyFuelTransactionsUpdate(IUnitOfWork uow)
		{
			_logger.LogInformation("Начинается обновление транзакций топлива за предыдущие дни...");

			var transactionsPerDayLastUpdateDate = _fuelControlSettings.FuelTransactionsPerDayLastUpdateDate;

			var startDate = transactionsPerDayLastUpdateDate.AddDays(1);

			if(startDate < GeneralUtils.GetCurrentMonthStartDate())
			{
				startDate = GeneralUtils.GetCurrentMonthStartDate();
			}

			var endDate = DateTime.Today.AddDays(-1);

			if(startDate > endDate)
			{
				_logger.LogInformation("Обновление не требуется. Данные по транзакциям до {TransactionsLastUpdateDate} уже сохранены",
					transactionsPerDayLastUpdateDate.ToString("yyyy-MM-dd"));

				return;
			}

			var isTransactionsUpdated = await GetAndSaveFuelTransactions(uow, startDate, endDate);

			if(isTransactionsUpdated)
			{
				_fuelControlSettings.SetFuelTransactionsPerDayLastUpdateDate(DateTime.Today.ToShortDateString());
			}
		}

		private async Task MonthlyFuelTransactionsUpdate(IUnitOfWork uow)
		{
			_logger.LogInformation("Начинается обновление транзакций топлива за предыдущий месяц...");

			var transactionsByMonthLastUpdateDate = _fuelControlSettings.FuelTransactionsPerMonthLastUpdateDate;

			var startDate = GeneralUtils.GetMonthStartDateByDate(transactionsByMonthLastUpdateDate.AddDays(1));

			if(startDate < GeneralUtils.GetPreviousMonthStartDate())
			{
				_logger.LogInformation("Обновление не требуется. Данные по транзакциям до {TransactionsLastUpdateDate} уже сохранены",
					transactionsByMonthLastUpdateDate.ToString("yyyy-MM-dd"));

				startDate = GeneralUtils.GetPreviousMonthStartDate();
			}

			var endDate = GeneralUtils.GetPreviousMonthEndDate();

			if(startDate >= endDate)
			{
				return;
			}

			var isTransactionsUpdated = await GetAndSaveFuelTransactions(uow, startDate, endDate);

			if(isTransactionsUpdated)
			{
				_fuelControlSettings.SetFuelTransactionsPerDayLastUpdateDate(GeneralUtils.GetPreviousMonthEndDate().ToShortDateString());
			}
		}

		private async Task<bool> GetAndSaveFuelTransactions(IUnitOfWork uow, DateTime startDate, DateTime endDate)
		{
			try
			{
				await Login();

				var transactionsCount = 0;

				var pageLimit = _fuelControlSettings.TransactionsPerQueryLimit;
				var pageOffset = 0;

				do
				{
					var transactions = await GetFuelTransactions(startDate, endDate, pageLimit, pageOffset);
					transactionsCount = transactions.Count();
					pageOffset += pageLimit;

					if(transactionsCount > 0)
					{
						var savedTransactionsCount = await _fuelRepository.SaveFuelTransactionsIfNeedAsync(uow, transactions);

						_logger.LogInformation("Сохранено в базе данных {SavedTransactionsCount} транзакций",
							savedTransactionsCount);
					}
				}
				while(transactionsCount == pageLimit);

				return true;
			}
			catch(Exception ex)
			{
				_logger.LogError("При выполнении операции обновления транзакций возникла ошибка: {ErrorMessage}",
					ex.Message);

				return false;
			}
		}

		private async Task Login()
		{
			if(IsAuthorized)
			{
				return;
			}

			_logger.LogDebug("Для запроса транзакций топлива необходимо авторизоваться");

			_sessionId = null;
			_sessionExpirationDate = null;

			var sessionId = await _authorizationService.Login(
				_options.Value.Login,
				_options.Value.Password,
				_options.Value.ApiKey);

			_sessionId = sessionId;
			_sessionExpirationDate = DateTime.Today.AddMinutes(_fuelControlSettings.ApiSessionLifetime.TotalMinutes);
		}

		private async Task<IEnumerable<FuelTransaction>> GetFuelTransactions(DateTime startDate, DateTime endDate, int pageLimit, int pageOffset)
		{
			return await _fuelTransactionsDataService.GetFuelTransactionsForPeriod(
				_sessionId,
				_options.Value.ApiKey,
				startDate,
				endDate,
				pageLimit,
				pageOffset);
		}
	}
}
