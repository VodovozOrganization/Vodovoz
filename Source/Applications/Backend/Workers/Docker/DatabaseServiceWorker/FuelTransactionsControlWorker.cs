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
using Vodovoz.Zabbix.Sender;
using VodovozInfrastructure.Utils;
using DateTimeHelpers;

namespace DatabaseServiceWorker
{
	public class FuelTransactionsControlWorker : TimerBackgroundServiceBase
	{
		private const string _dateTimeFormatString = "dd.MM.yyyy";

		private string _sessionId;
		private DateTime? _sessionExpirationDate;
		private bool _isWorkInProgress;

		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOptions<FuelTransactionsControlOptions> _options;
		private readonly ILogger<FuelTransactionsControlWorker> _logger;
		private readonly IFuelControlAuthorizationService _authorizationService;
		private readonly IFuelControlTransactionsDataService _fuelControlTransactionsDataService;
		private readonly IFuelPricesUpdateService _fuelPricesUpdateService;
		private readonly IFuelRepository _fuelRepository;
		private readonly IFuelControlSettings _fuelControlSettings;
		private readonly IZabbixSender _zabbixSender;

		public FuelTransactionsControlWorker(
			IUnitOfWorkFactory unitOfWorkFactory,
			IOptions<FuelTransactionsControlOptions> options,
			ILogger<FuelTransactionsControlWorker> logger,
			IFuelControlAuthorizationService authorizationService,
			IFuelControlTransactionsDataService fuelControlTransactionsDataService,
			IFuelPricesUpdateService fuelPricesUpdateService,
			IFuelRepository fuelRepository,
			IFuelControlSettings fuelControlSettings,
			IZabbixSender zabbixSender)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
			_fuelControlTransactionsDataService = fuelControlTransactionsDataService ?? throw new ArgumentNullException(nameof(fuelControlTransactionsDataService));
			_fuelPricesUpdateService = fuelPricesUpdateService ?? throw new ArgumentNullException(nameof(fuelPricesUpdateService));
			_fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			_fuelControlSettings = fuelControlSettings ?? throw new ArgumentNullException(nameof(fuelControlSettings));
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
		}

		protected override TimeSpan Interval => _options.Value.ScanInterval;

		private bool IsAuthorized =>
			!string.IsNullOrWhiteSpace(_sessionId)
			&& _sessionExpirationDate.HasValue
			&& DateTime.Today < _sessionExpirationDate.Value;

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			if(_isWorkInProgress)
			{
				return;
			}

			if(DateTime.Now.Hour < _options.Value.TransactionsDataRequestMinHour)
			{
				return;
			}

			_isWorkInProgress = true;

			using var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(FuelTransactionsControlWorker));

			await DailyFuelTransactionsUpdate(uow, stoppingToken);

			await MonthlyFuelTransactionsUpdate(uow, stoppingToken);

			await FuelPricesUpdate(stoppingToken);

			_isWorkInProgress = false;

			await _zabbixSender.SendIsHealthyAsync(stoppingToken);
		}

		private async Task DailyFuelTransactionsUpdate(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Начинается обновление транзакций топлива за предыдущие дни...");

			var transactionsPerDayLastUpdateDate = _fuelControlSettings.FuelTransactionsPerDayLastUpdateDate;

			var isNeedToUpdateExistingTransactions =
				DateTime.Today.Day == _options.Value.SavedTransactionsUpdateDay
				&& transactionsPerDayLastUpdateDate < DateTime.Today.AddDays(-1);

			var startDate = transactionsPerDayLastUpdateDate.AddDays(1);

			if(startDate < GeneralUtils.GetCurrentMonthStartDate() || isNeedToUpdateExistingTransactions)
			{
				startDate = GeneralUtils.GetCurrentMonthStartDate();
			}

			var endDate = DateTime.Today.AddDays(-1);

			if(startDate > endDate)
			{
				_logger.LogInformation("Обновление не требуется. Данные по транзакциям по {TransactionsLastUpdateDate} уже сохранены",
					transactionsPerDayLastUpdateDate.ToString(_dateTimeFormatString));

				return;
			}

			var isTransactionsUpdated = await GetAndSaveFuelTransactions(uow, startDate, endDate, isNeedToUpdateExistingTransactions, cancellationToken);

			if(isTransactionsUpdated)
			{
				_fuelControlSettings.SetFuelTransactionsPerDayLastUpdateDate(endDate.ToString(_dateTimeFormatString));
			}
		}

		private async Task MonthlyFuelTransactionsUpdate(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Начинается обновление транзакций топлива за предыдущий месяц...");

			var transactionsByMonthLastUpdateDate = _fuelControlSettings.FuelTransactionsPerMonthLastUpdateDate;

			var startDate = GeneralUtils.GetMonthStartDateByDate(transactionsByMonthLastUpdateDate.AddDays(1));

			if(startDate < GeneralUtils.GetPreviousMonthStartDate())
			{
				startDate = GeneralUtils.GetPreviousMonthStartDate();
			}

			var endDate = GeneralUtils.GetPreviousMonthEndDate();

			if(startDate >= endDate)
			{
				_logger.LogInformation("Обновление не требуется. Данные по транзакциям по {TransactionsLastUpdateDate} уже сохранены",
					transactionsByMonthLastUpdateDate.ToString(_dateTimeFormatString));

				return;
			}

			var isTransactionsUpdated = await GetAndSaveFuelTransactions(uow, startDate, endDate, true, cancellationToken);

			if(isTransactionsUpdated)
			{
				_fuelControlSettings.SetFuelTransactionsPerMonthLastUpdateDate(endDate.ToString(_dateTimeFormatString));
			}
		}

		public async Task FuelPricesUpdate(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Начинается обновление цен топлива...");

			var averageFuelPricesLastUpdateDate = _fuelControlSettings.FuelPricesLastUpdateDate;

			if(DateTime.Today.DayOfWeek != DayOfWeek.Monday && DateTime.Today.FirstDayOfWeek() <= averageFuelPricesLastUpdateDate
				|| averageFuelPricesLastUpdateDate >= DateTime.Today)
			{
				_logger.LogInformation(
					"Обновление цен топлива не требуется. Дата последнего обновления: {LastUpdateDate}",
					averageFuelPricesLastUpdateDate.ToString(_dateTimeFormatString));

				return;
			}

			try
			{
				await _fuelPricesUpdateService.UpdateFuelPricesByLastWeekTransaction(cancellationToken);

				_fuelControlSettings.SetFuelPricesLastUpdateDate(DateTime.Today.ToString(_dateTimeFormatString));

				_logger.LogInformation("Цены топлива обновлены успешно");
			}
			catch(Exception ex)
			{
				_logger.LogError("При выполнении операции обновления цен топлива возникла ошибка: {ErrorMessage}",
					ex.Message);
			}
		}

		private async Task<bool> GetAndSaveFuelTransactions(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate,
			bool isNeedToUpdateExistingTransactions,
			CancellationToken cancellationToken)
		{
			try
			{
				await Login(cancellationToken);

				var transactionsCount = 0;
				var pageLimit = _fuelControlSettings.TransactionsPerQueryLimit;

				if(pageLimit <= 0)
				{
					throw new InvalidOperationException("Значение лимита возвращаемого количества транзакций за один запрос должно быть больше нуля");
				}

				var pageOffset = 0;

				do
				{
					var transactions = await GetFuelTransactions(startDate, endDate, pageLimit, pageOffset, cancellationToken);
					transactionsCount = transactions.Count();
					pageOffset += pageLimit;

					if(transactionsCount > 0)
					{
						int savedTransactionsCount = default;

						if(isNeedToUpdateExistingTransactions)
						{
							savedTransactionsCount = await _fuelRepository.SaveNewAndUpdateExistingFuelTransactions(uow, transactions);
						}
						else
						{
							savedTransactionsCount = await _fuelRepository.SaveFuelTransactionsIfNeedAsync(uow, transactions);
						}

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

		private async Task Login(CancellationToken cancellationToken)
		{
			if(IsAuthorized)
			{
				return;
			}

			_logger.LogDebug("Для запроса транзакций топлива необходимо авторизоваться");

			_sessionId = null;
			_sessionExpirationDate = null;

			var session = await _authorizationService.Login(
				_options.Value.Login,
				_options.Value.Password,
				_options.Value.ApiKey,
				cancellationToken);

			_sessionId = session.SessionId;
			_sessionExpirationDate = session.SessionExpirationDate;
		}

		private async Task<IEnumerable<FuelTransaction>> GetFuelTransactions(
			DateTime startDate,
			DateTime endDate,
			int pageLimit,
			int pageOffset,
			CancellationToken cancellationToken)
		{
			return await _fuelControlTransactionsDataService.GetFuelTransactionsForPeriod(
				_sessionId,
				_options.Value.ApiKey,
				startDate,
				endDate,
				cancellationToken,
				pageLimit,
				pageOffset);
		}
	}
}
