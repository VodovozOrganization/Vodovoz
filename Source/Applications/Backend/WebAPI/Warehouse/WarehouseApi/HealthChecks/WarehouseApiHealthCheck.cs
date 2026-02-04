using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Presentation.WebApi.Authentication.Contracts;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Extensions;
using VodovozHealthCheck.Helpers;
using VodovozHealthCheck.Providers;

namespace WarehouseApi.HealthChecks
{
	public class WarehouseApiHealthCheck : VodovozHealthCheckBase
	{
		private readonly ILogger<WarehouseApiHealthCheck> _logger;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IConfiguration _configuration;

		public WarehouseApiHealthCheck(
			ILogger<WarehouseApiHealthCheck> logger,
			IHttpClientFactory httpClientFactory,
			IConfiguration configuration,
			IUnitOfWorkFactory unitOfWorkFactory,
			IHealthCheckServiceInfoProvider serviceInfoProvider) : base(logger, serviceInfoProvider, unitOfWorkFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		protected override async Task<VodovozHealthResultDto> CheckServiceHealthAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Проверка работоспособности: в WarehouseApi...");

			var healthSection = _configuration.GetSection("Health");

			var baseAddress = healthSection.GetValue<string>("BaseAddress");
			var orderId = healthSection.GetValue<string>("OrderId");

			var user = healthSection.GetValue<string>("Authorization:User");
			var password = healthSection.GetValue<string>("Authorization:Password");

			var healthResult = new VodovozHealthResultDto();

			var loginRequestDto = new LoginRequest
			{
				Username = user,
				Password = password
			};

			_logger.LogInformation($"Проверка работоспособности: post-запрос в {baseAddress}/api/Authenticate");

			var tokenResponse = await HttpResponseHelper.SendRequestAsync<TokenResponse>(
				HttpMethod.Post,
				$"{baseAddress}/api/Authenticate",
				_httpClientFactory,
				loginRequestDto.ToJsonContent(),
				cancellationToken);

			_logger.LogInformation($"Проверка работоспособности: Результа post-запроса в {baseAddress}/api/Authenticate, UserName = {tokenResponse.Data?.UserName}");

			_logger.LogInformation($"Проверка работоспособности: get-запрос в {baseAddress}/api/GetOrder?orderId={orderId}");

			var orderData = await HttpResponseHelper.GetByUriAsync(
				$"{baseAddress}/api/GetOrder?orderId={orderId}",
				_httpClientFactory,
				tokenResponse.Data?.AccessToken,
				cancellationToken: cancellationToken);

			var statusCode = ((HttpResponseMessage)orderData).StatusCode;

			_logger.LogInformation($"Проверка работоспособности: Результат get-запроса в {baseAddress}/api/GetOrder?orderId={orderId}: {statusCode}. {((HttpResponseMessage)orderData)}");

			var isHealthy = statusCode == HttpStatusCode.OK;

			_logger.LogInformation($"Проверка работоспособности: Результат проверки в WarehouseApi: isHealthy = {isHealthy}");

			if(!isHealthy)
			{
				healthResult.AdditionalUnhealthyResults.Add("GetOrder не прошёл проверку.");
			}

			healthResult.IsHealthy = isHealthy;

			return healthResult;
		}
	}
}
