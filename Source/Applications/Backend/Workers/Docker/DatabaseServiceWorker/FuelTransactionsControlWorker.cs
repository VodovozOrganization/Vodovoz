using DatabaseServiceWorker.Options;
using FuelControl.Contracts.Requests;
using FuelControl.Library.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Domain.Fuel;
using Vodovoz.Infrastructure;

namespace DatabaseServiceWorker
{
	public class FuelTransactionsControlWorker : TimerBackgroundServiceBase
	{
		private string _sessionId;
		private DateTime? _authorizationDate;

		private readonly IOptions<FuelTransactionsControlOptions> _options;
		private readonly ILogger<FuelTransactionsControlWorker> _logger;
		private readonly IFuelManagmentAuthorizationService _authorizationService;
		private readonly IFuelTransactionsDataService _fuelTransactionsDataService;

		public FuelTransactionsControlWorker(
			IOptions<FuelTransactionsControlOptions> options,
			ILogger<FuelTransactionsControlWorker> logger,
			IFuelManagmentAuthorizationService authorizationService,
			IFuelTransactionsDataService fuelTransactionsDataService)
		{
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
			_fuelTransactionsDataService = fuelTransactionsDataService ?? throw new ArgumentNullException(nameof(fuelTransactionsDataService));

			Interval = _options.Value.ScanInterval;
		}

		protected override TimeSpan Interval { get; }

		private bool isAuthorized =>
			!string.IsNullOrWhiteSpace(_sessionId)
			&& _authorizationDate.HasValue
			&& DateTime.Now > _authorizationDate.Value.AddDays(_options.Value.SessionLifetimeInDays);

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			try
			{
				if(!isAuthorized)
				{
					_logger.LogDebug("Для запроса транзакций топлива необходимо авторизоваться");

					await Login();
				}

				var transactions = GetFuelTransactionsForPreviousDay();
			}
			catch (Exception ex)
			{

			}
		}

		private async Task Login()
		{
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

		private IEnumerable<FuelTransaction> GetFuelTransactionsForPreviousDay()
		{
			return _fuelTransactionsDataService.GetFuelTransactionsForPreviousDay();
		}
	}
}
