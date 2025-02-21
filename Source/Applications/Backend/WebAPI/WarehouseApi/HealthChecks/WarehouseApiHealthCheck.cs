using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Vodovoz.Presentation.WebApi.Authentication.Contracts;
using VodovozHealthCheck;
using VodovozHealthCheck.Dto;
using VodovozHealthCheck.Helpers;

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
			IUnitOfWorkFactory unitOfWorkFactory) : base(logger, unitOfWorkFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		protected override async Task<VodovozHealthResultDto> GetHealthResult()
		{
			_logger.LogInformation("Проверка здоровья: в WarehouseApi...");

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

			_logger.LogInformation($"Проверка здоровья: post-запрос в {baseAddress}/api/Authenticate");

			var tokenResponse = await ResponseHelper.PostJsonByUri<LoginRequest, TokenResponse>(
				$"{baseAddress}/api/Authenticate",
				_httpClientFactory,
				loginRequestDto);

			_logger.LogInformation($"Проверка здоровья: Результа post-запроса в {baseAddress}/api/Authenticate, UserName = {tokenResponse.UserName}");

			_logger.LogInformation($"Проверка здоровья: get-запрос в {baseAddress}/api/GetOrder?orderId={orderId}");

			var orderData = await ResponseHelper.GetByUri(
				$"{baseAddress}/api/GetOrder?orderId={orderId}",
				_httpClientFactory,
				tokenResponse.AccessToken);

			var statusCode = ((HttpResponseMessage)orderData).StatusCode;

			_logger.LogInformation($"Проверка здоровья: Результат get-запроса в {baseAddress}/api/GetOrder?orderId={orderId}: {statusCode}. {((HttpResponseMessage)orderData)}");

			var isHealthy = statusCode == HttpStatusCode.OK;

			_logger.LogInformation($"Проверка здоровья: Результат проверки в WarehouseApi: isHealthy = {isHealthy}");

			if(!isHealthy)
			{
				healthResult.AdditionalUnhealthyResults.Add("GetOrder не прошёл проверку.");
			}

			healthResult.IsHealthy = isHealthy;

			return healthResult;
		}
	}
}
