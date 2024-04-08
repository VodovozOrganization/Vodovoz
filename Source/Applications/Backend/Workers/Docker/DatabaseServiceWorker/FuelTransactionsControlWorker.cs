using DatabaseServiceWorker.Options;
using FuelControl.Contracts.Requests;
using FuelControl.Library.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Fuel;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.Infrastructure;

namespace DatabaseServiceWorker
{
	public class FuelTransactionsControlWorker : TimerBackgroundServiceBase
	{
		private string _sessionId;
		private DateTime? _authorizationDate;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOptions<FuelTransactionsControlOptions> _options;
		private readonly ILogger<FuelTransactionsControlWorker> _logger;
		private readonly IFuelManagmentAuthorizationService _authorizationService;
		private readonly IFuelTransactionsDataService _fuelTransactionsDataService;
		private readonly IFuelRepository _fuelRepository;

		public FuelTransactionsControlWorker(
			IUnitOfWorkFactory unitOfWorkFactory,
			IOptions<FuelTransactionsControlOptions> options,
			ILogger<FuelTransactionsControlWorker> logger,
			IFuelManagmentAuthorizationService authorizationService,
			IFuelTransactionsDataService fuelTransactionsDataService,
			IFuelRepository fuelRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
			_fuelTransactionsDataService = fuelTransactionsDataService ?? throw new ArgumentNullException(nameof(fuelTransactionsDataService));
			_fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));

			Interval = _options.Value.ScanInterval;
			_authorizationDate = DateTime.Today;
		}

		protected override TimeSpan Interval { get; }

		private bool IsAuthorized =>
			!string.IsNullOrWhiteSpace(_sessionId)
			&& _authorizationDate.HasValue
			&& DateTime.Now < _authorizationDate.Value.AddDays(_options.Value.SessionLifetimeInDays);

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Сохранение транзакций топлива");

			if(!IsYesterdayTransactionsAreSaved(uow))
			{
				await GetAndSaveFuelTransactionsForPreviousDay(uow);
			}

		}

		private async Task GetAndSaveFuelTransactionsForPreviousDay(IUnitOfWork uow)
		{
			try
			{
				await Login();

				var transactions = await GetFuelTransactionsForPreviousDay();

				await _fuelRepository.SaveFuelTransactionsIfNeedAsync(uow, transactions);
			}
			catch(Exception ex)
			{

			}
		}

		private async Task GetAndSaveFuelTransactionsForPreviousMonth(IUnitOfWork uow)
		{
			try
			{
				await Login();

				var transactions = await GetFuelTransactionsForPreviousMonth();

				await _fuelRepository.SaveFuelTransactionsIfNeedAsync(uow, transactions);
			}
			catch(Exception ex)
			{

			}
		}

		private bool IsYesterdayTransactionsAreSaved(IUnitOfWork uow) =>
			_fuelRepository.GetSavedFuelTransactionsMaxDate(uow) < DateTime.Today.AddDays(-1);

		private async Task Login()
		{
			if(IsAuthorized)
			{
				return;
			}

			_logger.LogDebug("Для запроса транзакций топлива необходимо авторизоваться");

			_sessionId = null;
			_authorizationDate = null;

			var sessionId = await _authorizationService.Login(CreateAuthorizationRequestObject());

			_sessionId = sessionId;
			_authorizationDate = DateTime.Today;
		}

		private AuthorizationRequest CreateAuthorizationRequestObject()
		{
			return new AuthorizationRequest
			{
				Login = _options.Value.Login,
				Password = _options.Value.Password,
				ApiKey = _options.Value.ApiKey,
				BaseAddress = _options.Value.BaseAddress
			};
		}

		private async Task<IEnumerable<FuelTransaction>> GetFuelTransactionsForPreviousDay()
		{
			return await _fuelTransactionsDataService.GetFuelTransactionsForPreviousDay(
				_sessionId,
				_options.Value.BaseAddress,
				_options.Value.ApiKey,
				_options.Value.OrganizationContractId);
		}

		private async Task<IEnumerable<FuelTransaction>> GetFuelTransactionsForPreviousMonth()
		{
			return await _fuelTransactionsDataService.GetFuelTransactionsForPreviousMonth(
				_sessionId,
				_options.Value.BaseAddress,
				_options.Value.ApiKey,
				_options.Value.OrganizationContractId);
		}
	}
}
