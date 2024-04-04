using DatabaseServiceWorker.Options;
using FuelControl.Contracts.Requests;
using FuelControl.Library.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
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

		public FuelTransactionsControlWorker(
			IOptions<FuelTransactionsControlOptions> options,
			ILogger<FuelTransactionsControlWorker> logger,
			IFuelManagmentAuthorizationService authorizationService)
		{
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));

			Interval = _options.Value.ScanInterval;
		}

		protected override TimeSpan Interval { get; }

		protected override async Task DoWork(CancellationToken stoppingToken)
		{
			if(string.IsNullOrWhiteSpace(_sessionId)
				|| !_authorizationDate.HasValue
				|| _authorizationDate.Value < DateTime.Today.AddDays(-30))
			{
				await Login();
			}


		}

		private async Task Login()
		{
			_sessionId = null;
			_authorizationDate = null;

			var sessionId = await _authorizationService.Login(CreateAuthorizationRequestObject());

			if(!string.IsNullOrEmpty(sessionId))
			{
				_sessionId = sessionId;
				_authorizationDate = DateTime.Today;
			}
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
	}
}
